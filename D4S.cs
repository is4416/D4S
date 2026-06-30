using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// ---------- ---------- ---------- ---------- ----------
// HttpMethod
// ---------- ---------- ---------- ---------- ----------

public enum HttpMethod
{
	GET,
	POST
}

// ---------- ---------- ---------- ---------- ----------
// D4S (.NET Framework 4.0 Server)
// ---------- ---------- ---------- ---------- ----------

public class D4S
{
	// ---------- ---------- ----------
	// Field
	// ---------- ---------- ----------

	private readonly string _address;
	private readonly HttpListener _listener;
	private readonly Dictionary<Tuple<HttpMethod, string>, Func<HttpListenerContext, Task>> _routes;
	private bool _isListen;
	private readonly string _root;

	// ---------- ---------- ----------
	// Constructor
	// ---------- ---------- ----------

	public D4S(
		string address  = "http://localhost:8000/",
		string rootPath = "./"
	)
	{
		_address  = address;
		_listener = new HttpListener();
		_routes   = new Dictionary<Tuple<HttpMethod, string>, Func<HttpListenerContext, Task>>();
		_isListen = false;
		_root     = Path.GetFullPath(
			Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootPath)
		);

		if (!Directory.Exists(_root))
		{
			throw new DirectoryNotFoundException(_root);
		}

		_listener.Prefixes.Add(address);
	}

	// private: static
	// ---------- ---------- ----------
	// BuildDirectoryListing
	// ---------- ---------- ----------
	/**
	 * ディレクトリの内容を一覧表示する HTML を生成する。
	 *
	 * path - 実ディレクトリのパス、
	 * url  - リンク生成に使用する URL パス。
	 */
	private static string BuildDirectoryListing(string path, string url)
	{
		var sb = new StringBuilder();

		string baseUrl = url.EndsWith("/") ? url : url + "/";

		sb.Append("<html><body><h1>Index of " + url + "</h1><ul>");

		foreach (var dir in Directory.GetDirectories(path))
		{
			var name = Path.GetFileName(dir);
			string encoded = Uri.EscapeDataString(name);
			sb.Append("<li><a href=\"" + baseUrl + encoded + "\">" + name + "/</a></li>");
		}

		foreach (var file in Directory.GetFiles(path))
		{
			var name = Path.GetFileName(file);
			string encoded = Uri.EscapeDataString(name);
			sb.Append("<li><a href=\"" + baseUrl + encoded + "\">" + name + "</a></li>");
		}

		sb.Append("</ul></body></html>");
		return sb.ToString();
	}

	// public: static
	// ---------- ---------- ----------
	// GetMimeType
	// ---------- ---------- ----------
	/**
	 * path から mimeType 文字列を作成する
	 */
	public static string GetMimeType(string path)
	{
		var ext = Path.GetExtension(path).ToLowerInvariant();

		switch (ext)
		{
			case ".html" : return "text/html; charset=utf-8";
			case ".css"  : return "text/css; charset=utf-8";
			case ".js"   : return "application/javascript; charset=utf-8";
			case ".json" : return "application/json";
			case ".png"  : return "image/png";
			case ".jpg"  :
			case ".jpeg" : return "image/jpeg";
			case ".gif"  : return "image/gif";
			case ".svg"  : return "image/svg+xml";
			case ".webp" : return "image/webp";
			case ".avif" : return "image/avif";
			case ".ico"  : return "image/x-icon";
			case ".bmp"  : return "image/bmp";
			case ".tif"  :
			case ".tiff" : return "image/tiff";
			case ".mp3"  : return "audio/mpeg";
			case ".wav"  : return "audio/wave";
			case ".ogg"  : return "audio/ogg";
			case ".mp4"  : return "video/mp4";
			case ".webm" : return "video/webm";
			case ".ogv"  : return "video/ogg";
			case ".woff" : return "font/woff";
			case ".woff2": return "font/woff2";
			case ".ttf"  : return "font/ttf";
			case ".otf"  : return "font/otf";
			case ".zip"  : return "application/zip";
			case ".pdf"  : return "application/pdf";
			case ".txt"  : return "text/plain; charset=utf-8";
			case ".xml"  : return "application/xml";
			case ".csv"  : return "text/csv; charset=utf-8";
			default      : return "application/octet-stream";
		}
	}

	// public: static
	// ---------- ---------- ----------
	// GetParams
	// ---------- ---------- ----------
	/**
	 * GET または POST のパラメータを取得する
	 */
	public static Dictionary<string, string> GetParams(HttpListenerContext ctx)
	{
		var req = ctx.Request;

		var result = new Dictionary<string, string>();

		// GET
		foreach (string key in req.QueryString.AllKeys)
		{
			var value = req.QueryString[key];
			if (!string.IsNullOrEmpty(key) && value != null)
			{
				result[key] = value;
			}
		}

		// POST
		if (req.HasEntityBody)
		{
			using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
			{
				string body = reader.ReadToEnd();
				string contentType = req.ContentType ?? "";

				// application/x-www-form-urlencoded
				if (contentType.StartsWith("application/x-www-form-urlencoded"))
				{
					var parsed = System.Web.HttpUtility.ParseQueryString(body);

					foreach (string key in parsed.AllKeys)
					{
						var value = parsed[key];
						if (!string.IsNullOrEmpty(key) && value != null)
						{
							result[key] = value;
						}
					}
				}

				// multipart/form-data (FormData)
				else if (contentType.StartsWith("multipart/form-data"))
				{
					int p = contentType.IndexOf("boundary=");
					if (p >= 0)
					{
						string boundary = "--" + contentType.Substring(p + 9);

						string[] parts = body.Split(
							new string[] { boundary },
							StringSplitOptions.RemoveEmptyEntries
						);

						foreach (string part in parts)
						{
							if (part == "--\r\n" || part == "--")
								continue;

							int headerEnd = part.IndexOf("\r\n\r\n");
							if (headerEnd < 0)
								continue;

							string header = part.Substring(0, headerEnd);
							string value = part.Substring(headerEnd + 4);

							value = value.TrimEnd('\r', '\n', '-');

							int namePos = header.IndexOf("name=\"");
							if (namePos < 0)
								continue;

							namePos += 6;
							int nameEnd = header.IndexOf("\"", namePos);
							if (nameEnd < 0)
								continue;

							string name = header.Substring(namePos, nameEnd - namePos);

							result[name] = value;
						}
					}
				}
			}
		}

		return result;
	}

	// public
	// ---------- ---------- ----------
	// WriteTextAsync
	// ---------- ---------- ----------
	/**
	 * レスポンスに text を書き込む
	 * 8KB を超える場合で gzip 可能な場合は圧縮する
	 */
	public async Task WriteTextAsync(
		HttpListenerContext ctx,
		string text,
		string mimeType   = "text/plain; charset=utf-8",
		int    statusCode = 200
	)
	{
		var req = ctx.Request;
		var res = ctx.Response;

		res.ContentType = mimeType;
		res.StatusCode  = statusCode;

		byte[] buffer = Encoding.UTF8.GetBytes(text);

		// gzip対応チェック
		var enc = req.Headers["Accept-Encoding"];

		// クライアントが gzip を受け付け、レスポンスが 8KB を超える場合は圧縮
		if (
			!string.IsNullOrEmpty(enc) &&
			enc.Contains("gzip")       &&
			buffer.Length > 1024 * 8
		)
		{
			res.AddHeader("Content-Encoding", "gzip");

			using (var stream = res.OutputStream)
			using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress))
			{
				await gzip.WriteAsync(buffer, 0, buffer.Length);
			}
		}
		else
		{
			res.ContentLength64 = buffer.Length;

			using (var stream = res.OutputStream)
			{
				await stream.WriteAsync(buffer, 0, buffer.Length);
			}
		}
	}

	// public
	// ---------- ---------- ----------
	// WriteFileAsync
	// ---------- ---------- ----------
	/**
	 * ファイルをレスポンスに書き込む
	 */
	public async Task WriteFileAsync(HttpListenerContext ctx, string path)
	{
		var res = ctx.Response;

		if (!File.Exists(path))
		{
			await WriteTextAsync(ctx, "404 Not Found", statusCode: 404);
			return;
		}

		string mimeType = GetMimeType(path);

		using (var fs = File.OpenRead(path))
		using (var stream = res.OutputStream)
		{
			res.ContentType = mimeType;
			res.ContentLength64 = fs.Length;

			await fs.CopyToAsync(stream);
		}
	}

	// public
	// ---------- ---------- ----------
	// AddRoute
	// ---------- ---------- ----------
	/**
	 * ルートを追加する
	 */
	public void AddRoute(HttpMethod method, string url, Func<HttpListenerContext, Task> handler)
	{
		_routes[Tuple.Create(method, url)] = handler;
	}

	// private
	// ---------- ---------- ----------
	// HandleRequest
	// ---------- ---------- ----------
	/**
	 * Start から呼ばれるリクエスト処理
	 */
	private async Task HandleRequest(HttpListenerContext ctx)
	{
		var req = ctx.Request;
		var res = ctx.Response;

		var url = req.Url != null ? req.Url.AbsolutePath : "/";
		if (string.IsNullOrEmpty(url)) url = "/";

		// method
		HttpMethod method;
		var m = req.HttpMethod.ToUpperInvariant();

		if (m == "GET") method = HttpMethod.GET;
		else if (m == "POST") method = HttpMethod.POST;
		else
		{
			await WriteTextAsync(ctx, "405 Method Not Allowed", statusCode: 405);
			return;
		}

		// routing
		var key = Tuple.Create(method, url);
		Func<HttpListenerContext, Task> handler;

		if (_routes.TryGetValue(key, out handler))
		{
			await handler(ctx);
			return;
		}

		// static
		string root = _root;
		string path = Path.GetFullPath(
			Path.Combine(_root , url.TrimStart('/'))
		);

		// traversal防止
		if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
		{
			await WriteTextAsync(ctx, "403 Forbidden", statusCode: 403);
			return;
		}

		// directory
		if (Directory.Exists(path))
		{
			string indexPath = Path.Combine(path, "index.html");

			if (File.Exists(indexPath))
			{
				path = indexPath;
			}
			else
			{
				// ディレクトリ一覧
				string html = BuildDirectoryListing(path, url);
				await WriteTextAsync(ctx, html, "text/html; charset=utf-8");
				return;
			}
		}

		// file
		if (File.Exists(path))
		{
			await WriteFileAsync(ctx, path);
			return;
		}

		// 404
		await WriteTextAsync(ctx, "404 Not Found", statusCode: 404);
	}

	// public
	// ---------- ---------- ----------
	// Start
	// ---------- ---------- ----------
	/**
	 * サーバーを起動してメッセージを処理する
	 */
	public async Task Start()
	{
		_listener.Start();
		Console.WriteLine("listening: " + _address);

		_isListen = true;

		try
		{
			while (_isListen)
			{
				var ctx = await _listener.GetContextAsync();

				var _ = Task.Run(async () =>
				{
					try
					{
						await HandleRequest(ctx);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				});
			}
		}

		catch (HttpListenerException)
		{
			Console.WriteLine("stop LightServer");
		}
	}

	// ---------- ---------- ----------
	// public Stop
	// ---------- ---------- ----------
	/**
	 * サーバーを停止する
	 */
	public void Stop()
	{
		_isListen = false;
		_listener.Stop();
	}
}

// ---------- ---------- ---------- ---------- ----------
// D4SLog
// ---------- ---------- ---------- ---------- ----------
/**
 * 簡易的なロガー
 */
public static class D4SLog
{
	// ---------- ---------- ----------
	// Field
	// ---------- ---------- ----------

	private const int MAX_LOG = 100;
	private static readonly object _lock = new object();
	private static readonly List<string> _logs = new List<string>();

	// public: static
	// ---------- ---------- ----------
	// Write
	// ---------- ---------- ----------
	/**
	 * ログ及びコンソールへの書き込み
	 */
	public static void Write(string text)
	{
		lock(_lock)
		{
			_logs.Add(text);

			if (_logs.Count > MAX_LOG)
			{
				_logs.RemoveAt(0);
			}

			Console.WriteLine(text);
		}
	}

	// public: static
	// ---------- ---------- ----------
	// Get
	// ---------- ---------- ----------
	/**
	 * 現在のログを取得する
	 */
	public static List<string> Get()
	{
		lock(_lock)
		{
			return new List<string>(_logs);
		}
	}

	// public: static
	// ---------- ---------- ----------
	// Clear
	// ---------- ---------- ----------
	/**
	 * ログを初期化する
	 */
	public static void Clear()
	{
		lock(_lock)
		{
			_logs.Clear();
		}
	}
}
