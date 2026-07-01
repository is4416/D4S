using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

// ---------- ---------- ---------- ---------- ----------
// D4SHandlers
// ---------- ---------- ---------- ---------- ----------
/**
 * D4S.AddRoute に渡すハンドラのプリセット
 */
public static class D4SHandlers
{
	// ---------- ---------- ----------
	// Hello
	// ---------- ---------- ----------
	/**
	 * ハンドラ実装例
	 */
	public static Func<HttpListenerContext, Task> Hello(D4S server)
	{
		return ctx => {
			return server.WriteTextAsync(ctx, "Hello World from .NET Framework D4S Handlers.");
		};
	}

	// ---------- ---------- ----------
	// ExecuteProcess
	// ---------- ---------- ----------
	/**
	* [app]  外部 exe のパス
	* [args] 外部 exe に渡す引数
	*/
	public static Func<HttpListenerContext, Task> ExecuteProcess(D4S server)
	{
		return ctx => {
			var param = D4S.GetParams(ctx);

			var app  = param.ContainsKey("app")  ? param["app"]  : "";
			var args = param.ContainsKey("args") ? param["args"] : "";

			if (app == "")
			{
				return server.WriteTextAsync(
					ctx,
					"parameter 'app' is required",
					statusCode: 400
				);
			}

			// 相対パスから絶対パスへの変換
			if (Path.IsPathRooted(app))
			{
				app = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, app);
			}

			try
			{
				var info = new ProcessStartInfo
				{
					FileName               = app,
					Arguments              = args,
					UseShellExecute        = false,
					RedirectStandardOutput = true,
					RedirectStandardError  = true,
					CreateNoWindow         = true
				};

				var process = Process.Start(info);
				if (process == null)
				{
					return server.WriteTextAsync(
						ctx,
						"Failed to start process.",
						statusCode: 500
					);
				}

				using (process)
				{
					var outputTask = process.StandardOutput.ReadToEndAsync();
					var errorTask  = process.StandardError.ReadToEndAsync();

					if (!process.WaitForExit(30000))
					{
						process.Kill();

						return server.WriteTextAsync(
							ctx,
							"Process timeout.",
							statusCode: 500
						);
					}

					var output = outputTask.Result;
					var error  = errorTask.Result;

					if (!string.IsNullOrEmpty(error))
					{
						return server.WriteTextAsync(ctx, error, statusCode: 500);
					}

					return server.WriteTextAsync(ctx, output);
				}
			}
			catch (Exception err)
			{
				return server.WriteTextAsync(
					ctx,
					err.Message,
					statusCode: 500
				);
			}
		};
	}

	// ---------- ---------- ----------
	// SaveToFile
	// ---------- ---------- ----------
	/**
	 * [data] を [path] で指定した場所に保存する
	 * とりあえずテキストデータだけを想定
	 */
	public static Func<HttpListenerContext, Task> SaveToFile(D4S server)
	{
		return ctx => {
			var param = D4S.GetParams(ctx);
			var data  = param.ContainsKey("data") ? param["data"] : "";
			var path  = param.ContainsKey("path") ? param["path"] : "";

			if (
				path == ""
			)
			{
				return server.WriteTextAsync(ctx, "error: saveToFile path = " + path, statusCode: 403);
			}

			var parent = Path.GetDirectoryName(path);

			if (!string.IsNullOrEmpty(parent)) {
				Directory.CreateDirectory(parent);
			}

			try
			{
				File.WriteAllText(path, data);
				return server.WriteTextAsync(ctx, "success");
			}
			catch (Exception err)
			{
				return server.WriteTextAsync(ctx, err.Message, statusCode: 400);
			}
		};
	}

	// ---------- ---------- ----------
	// LoadFromFile
	// ---------- ---------- ----------
	/**
	 * [path] からデータを取得
	 */
	public static Func<HttpListenerContext, Task> LoadFromFile(D4S server)
	{
		return ctx => {
			var param = D4S.GetParams(ctx);
			var path  = param.ContainsKey("path") ? param["path"] : "";

			if (
				path == "" ||
				!File.Exists(path)
			)
			{
				return server.WriteTextAsync(ctx, "error: loadFromFile path = " + path, statusCode: 403);
			}

			return server.WriteFileAsync(ctx, path);
		};
	}

	// ---------- ---------- ----------
	// CreateDirectoryTree
	// ---------- ---------- ----------
	/*
	 * [path] で指定したディレクトリパスから、JSONツリーを作成する
	 */
	public static Func<HttpListenerContext, Task> CreateDirectoryTree(D4S server)
	{
		return ctx =>
		{
			var param = D4S.GetParams(ctx);
			var path  = param.ContainsKey("path") ? param["path"] : "";

			if (path == "" || !Directory.Exists(path))
			{
				return server.WriteTextAsync(ctx, "{}", "application/json", 403);
			}

			var obj = new JsonDirectory(new DirectoryInfo(path));
			return server.WriteTextAsync(ctx, Json.Stringify(obj.ToObject()), "application/json");
		};
	}

	// ---------- ---------- ----------
	// CreateDirectoryTreeDiff
	// ---------- ---------- ----------
	/*
	 * 作成済みの JSONツリーを差分更新する
	 * [path] ディレクトリパス
	 * [json] 更新前の JSON データ
	 */
	public static Func<HttpListenerContext, Task> CreateDirectoryTreeDiff(D4S server)
	{
		return ctx =>
		{
			var param = D4S.GetParams(ctx);
			var path  = param.ContainsKey("path") ? param["path"] : "";
			var json  = param.ContainsKey("json") ? param["json"] : "";

			if (path == "" || json == "" || !Directory.Exists(path))
			{
				return server.WriteTextAsync(ctx, "{}", "application/json", 403);
			}

			try
			{
				var obj = Json.Parse(json);

				JsonDirectory.Diff(obj, new DirectoryInfo(path));
				return server.WriteTextAsync(ctx, Json.Stringify(obj), "application/json");
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return server.WriteTextAsync(ctx, "{}", "application/json", 400);
			}
		};
	}

	// ---------- ---------- ----------
	// pathCombine
	// ---------- ---------- ----------
	/**
	 * パス要素を結合してフルパスを返す
	 * [parts] パス要素のJSON配列
	 */
	public static Func<HttpListenerContext, Task> PathCombine(D4S server)
	{
		return ctx => {
			var    param = D4S.GetParams(ctx);
			string json  = param.ContainsKey("parts") ? param["parts"] : "[]";

			try
			{
				var parts = Json.Parse<string[]>(json);
				return server.WriteTextAsync(ctx, Path.Combine(parts));
			}
			catch (Exception err)
			{
				Console.WriteLine("error: PathCombine " + err.Message);
				return server.WriteTextAsync(ctx, "", statusCode: 400);
			}
		};
	}
}
