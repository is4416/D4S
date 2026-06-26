# D4S (.NET Framework 4.0 Server)

.NET Framework 4.0 標準機能だけで動作する軽量Webサーバー

フロントエンドを中心に開発し、バックエンドは AddRoute に必要な処理を書くだけで利用できます  
静的ファイル配信や HTTP リクエスト処理は D4S が担当します

Node.js や .NET SDK などをインストールできない会社の Windows PC 上でも、付属の csc だけでビルド・実行できることをコンセプトとしています

[D4S](#d4scs)

## Features

- .NET Framework 4.0 標準機能のみで動作
- 外部ライブラリ不要
- 静的ファイル配信
- GET / POST のルーティング
- JSONレスポンス生成
- MIMEタイプ自動判定

---

## Build

.NET Framework 4.0 に付属する `csc` だけでビルドできます

ビルド例:

```bash
csc Program.cs D4S.cs Json.cs
```

---

## API Reference

### Program.cs

Program

エントリーポイントです。
基本的な使用例は `Program.cs` を参照してください。

```csharp
class Program
{
	static void Main();
}
```

---

### Json.cs

Json  
オブジェクトをJSON文字列に変換します

```csharp
public class Json
{
	public static string Stringify(object obj);
}
```

JsonItem  
JSON形式に変換可能なオブジェクトの基底クラス

```csharp
public class JsonItem
{
	public JsonItem(FileSystemInfo info);
	public virtual object ToObject();
}
```

JsonFile : JsonItem  
JSON形式に変換可能なファイル用オブジェクト

```csharp
public class JsonFile : JsonItem
{
	public JsonFile(FileInfo info);
	public override object ToObject();
}
```

JsonDirectory : JsonItem  
JSON形式に変換可能なディレクトリ用オブジェクト

```csharp
public class JsonDirectory : JsonItem
{
	public JsonDirectory(DirectoryInfo info);
	public override object ToObject();
}
```

---

### D4S.cs

```csharp
public enum HttpMethod
{
	GET,
	POST
}
```

D4S

D4S の本体クラスです

```csharp
public class D4S
{
	public D4S(string address, string rootPath = "./");

	public static string GetMimeType(string path);
	public static Dictionary<string, string> GetParams(HttpListenerContext ctx);

	public Task WriteTextAsync(HttpListenerContext ctx, string mimeType, string text);
	public Task WriteFileAsync(HttpListenerContext ctx, string path);

	public void AddRoute(HttpMethod method, string url, Func<HttpListenerContext, Task> handler);
	public Task Start();
	public void Stop();
}
```

コンストラクタで `rootPath` を省略した場合は exe ファイルが配置されたフォルダがドキュメントルートとなります

---

## ルート追加例

APIを追加する場合は `D4S.AddRoute` を利用してください

以下の例では

- D4S を作成
- ルートを追加
- サーバーを起動

しています

```csharp
D4S server = new D4S("http://localhost:8000/", "dist");

server.AddRoute(HttpMethod.GET, "/api/test", (HttpListenerContext ctx) => {
	var param = D4S.GetParams(ctx);
	string text = param.ContainsKey("text") ? param["text"] : "";
	return server.WriteTextAsync(ctx, "text/plain; charset=utf-8", text);
});

await server.Start();
```

---

## License

MIT License
