// csc: "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" Program.cs Json.cs D4S.cs D4SHandlers.cs

using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

// ---------- ---------- ---------- ---------- ----------
// Program
// ---------- ---------- ---------- ---------- ----------

class Program
{
	// ---------- ---------- ----------
	// static Main
	// ---------- ---------- ----------

	static void Main()
	{
		string address  = "http://localhost:8000/";
		string rootPath = "dist";

		D4S server = new D4S(address, rootPath);

		// ルーティング追加 (例)
		server.AddRoute(HttpMethod.GET, "/api/test", (HttpListenerContext ctx) => {

			// パラメータ取得
			var param = D4S.GetParams(ctx);

			// パラメータから値を取得
			string text = param.ContainsKey("text") ? param["text"] : "";

			// レスポンスにテキストを返す
			return server.WriteTextAsync(ctx, "GET Text = " + text, "text/plain; charset=utf-8");
		});

		// D4SHandlers利用（例）
		// HttpMethodは、GET でも POST でも自由に設定できます
		server.AddRoute(HttpMethod.GET, "/api/hello", D4SHandlers.Hello(server));
		server.AddRoute(HttpMethod.GET, "/api/startProcess", D4SHandlers.StartProcess(server)); // app, args
		server.AddRoute(HttpMethod.POST, "/api/saveToFile", D4SHandlers.SaveToFile(server)); // data, path
		server.AddRoute(HttpMethod.POST, "/api/loadFromFile", D4SHandlers.LoadFromFile(server)); // path
		server.AddRoute(HttpMethod.POST, "/api/createDirectoryTree", D4SHandlers.CreateDirectoryTree(server)); // path

		// ブラウザ起動
		Process.Start(address);

		// サーバー起動
		server.Start().Wait();
	}
}
