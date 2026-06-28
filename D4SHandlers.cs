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
		return async ctx => {
			await server.WriteTextAsync(ctx, "Hello World from .NET Framework D4S Handlers.");
		};
	}

	// ---------- ---------- ----------
	// StartProcess
	// ---------- ---------- ----------
	/**
	 * [app] を [args] 付きで呼び出す
	 */
	public static Func<HttpListenerContext, Task> StartProcess(D4S server)
	{
		return async ctx => {
			var param = D4S.GetParams(ctx);
			var app   = param.ContainsKey("app") ? param["app"] : "";
			var args  = param.ContainsKey("args") ? param["args"] : "";

			if (app == "")
			{
				await server.WriteTextAsync(ctx, "parameter 'app' is required", statusCode: 400);
				return;
			}

			try
			{
				Process.Start(app, args);
				await server.WriteTextAsync(ctx, "success");
			}
			catch (Exception err)
			{
				await server.WriteTextAsync(ctx, err.Message, statusCode: 500);
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
		return async ctx => {
			var param = D4S.GetParams(ctx);
			var data  = param.ContainsKey("data") ? param["data"] : "";
			var path  = param.ContainsKey("path") ? param["path"] : "";

			if (
				path == ""
			)
			{
				await server.WriteTextAsync(ctx, "error: saveToFile path = " + path, statusCode: 403);
				return;
			}

			var parent = Path.GetDirectoryName(path);

			if (!string.IsNullOrEmpty(parent)) {
				Directory.CreateDirectory(parent);
			}

			try
			{
				await File.WriteAllTextAsync(path, data);
				await server.WriteTextAsync(ctx, "success");
			}
			catch (Exception err)
			{
				await server.WriteTextAsync(ctx, err.Message, statusCode: 400);
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
		return async ctx => {
			var param = D4S.GetParams(ctx);
			var path  = param.ContainsKey("path") ? param["path"] : "";

			if (
				path == "" ||
				!File.Exists(path)
			)
			{
				await server.WriteTextAsync(ctx, "error: loadFromFile path = " + path, statusCode: 403);
				return;
			}

			await server.WriteFileAsync(ctx, path);
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
		return async ctx =>
		{
			var param = D4S.GetParams(ctx);
			var path  = param.ContainsKey("path") ? param["path"] : "";

			if (path == "" || !Directory.Exists(path))
			{
				await server.WriteTextAsync(ctx, "{}", "application/json", 403);
				return;
			}

			var obj = new JsonDirectory(new DirectoryInfo(path));
			await server.WriteTextAsync(ctx, Json.Stringify(obj.ToObject()), "application/json");
		};
	}
}