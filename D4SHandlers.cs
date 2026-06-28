using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

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
	// CreateDirectoryTree
	// ---------- ---------- ----------
	/*
	 * 指定したディレクトリから JSON ツリーを作成する
	 * 失敗したときは からの JSON を返す
	 */
	public static Func<HttpListenerContext, Task> CreateDirectoryTree(D4S server)
	{
		return async ctx =>
		{
			var param = server.GetParams(ctx);
			var rootPath = param.ContainsKey("rootPath") ? param["rootPath"] : "";

			if (
				rootPath == "" ||
				!Directory.Exists(rootPath)
			)
			{
				await server.WriteTextAsync(ctx, "{}", "application/json", 403);
			}
			else
			{
				var obj = new JsonDirectory(new DirectoryInfo(rootPath));
				await server.WriteTextAsync(ctx, Json.Stringify(obj.ToObject()), "application/json");
			}
		};
	}
}