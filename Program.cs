// csc: "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" Program.cs Json.cs D4S.cs

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
			var param = D4S.GetParams(ctx);
			string text = param.ContainsKey("text") ? param["text"] : "";
			return server.WriteTextAsync(ctx, "text/plain; charset=utf-8", "GET Text = " + text);
		});

		// ブラウザ起動
		Process.Start(address);

		// サーバー起動
		server.Start().Wait();
	}
}
