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
}