# D4S (.NET Framework 4.0 Server)

.NET Framework 4.0 標準機能だけで動作する、  
フロントエンド開発者向けの軽量ローカルWebサーバーです。

フロントエンド開発を中心に、バックエンドは AddRoute に必要な処理を書くだけで利用できます  
静的ファイル配信や HTTP リクエスト処理は D4S が担当します

Node.js や .NET SDK などをインストールできない会社の Windows PC 上でも、付属の csc だけでビルド・実行できることをコンセプトとしています

**構成**

```
D4S/
 ├ D4S.cs (ライブラリ本体)
 │
 ├ Program.cs (サンプル)
 ├ Json.cs (サンプルで使用する補助クラス)
 ├ D4SHandlers.cs (サンプルハンドラ)
 │
 └ README.md (README ファイル)
```

ライブラリ本体は `D4S.cs` のみです。

`Json.cs` は JSON を扱うための補助クラス、  
`D4SHandlers.cs` は `AddRoute` に登録できるハンドラのサンプル実装です。

- [Program.cs](#programcs)
- [Json.cs](#jsoncs)
- [D4S.cs](#d4scs)
- [D4SHandlers.cs](#d4shandlerscs)

## 特徴

- 単一ファイルライブラリ (`D4S.cs`)
- 外部ライブラリ不要
- .NET Framework 4.0 標準機能のみで動作
- 静的ファイル配信
- GET / POST ルーティング
- JSON レスポンス
- MIME タイプ自動判定

---

## Build

.NET Framework 4.0 に付属する `csc` だけでビルドできます

最小構成

```bash
csc Program.cs D4S.cs
```

サンプルを利用する場合は、Json.cs と D4SHandlers.cs も一緒にコンパイルしてください

---

## Sample

```csharp
var server = new D4S();
server.AddRoute(HttpMethod.GET, "/api/hello", D4SHandlers.Hello(server));
server.Start().Wait();
```

---

## API Reference

### Program.cs

**Program**

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

**Json**

- オブジェクトを JSON 文字列へ変換
- JSON 文字列をオブジェクトへ変換

```csharp
public static class Json
{
	public static string Stringify(object obj);
	public static Dictionary<string, object> Parse(string json);
	public static T Parse<T>(string json);
}
```

**JsonItem**

JSON形式に変換可能なオブジェクトの基底クラス

```csharp
public class JsonItem
{
	public JsonItem(FileSystemInfo info);
	public virtual Dictionary<string, object> ToObject();
}
```

**JsonFile : JsonItem**

JSON形式に変換可能なファイル用オブジェクト

```csharp
public class JsonFile : JsonItem
{
	public JsonFile(FileInfo info);
	public override Dictionary<string, object> ToObject();
}
```

**JsonDirectory : JsonItem**

JSON形式に変換可能なディレクトリ用オブジェクト

```csharp
public class JsonDirectory : JsonItem
{
	public JsonDirectory(DirectoryInfo info, int depth = -1);
	public static Dictionary<string, object> Diff(Dictionary<string, object> json, DirectoryInfo info);
	public override Dictionary<string, object> ToObject();
}
```

**JsonDirectory**

- depth: 子ディレクトリを再帰的に取得する深さ
- `-1`: 全階層取得 (規定値)
- `0` : 子ディレクトリを取得しない (ファイルのみ)
- `1`以上 : 指定した階層まで取得

**Diff**

既存のディレクトリツリー JSON オブジェクトを現在のディレクトリの状態へ更新します  
変更があったファイル・ディレクトリのみ更新し、変更のないオブジェクトは再利用します

---

### D4S.cs

**HttpMethod**

```csharp
public enum HttpMethod
{
	GET,
	POST
}
```

**D4S**

D4S の本体クラスです

```csharp
public class D4S
{
	public D4S(
		string address  = "http://localhost:8000/",
		string rootPath = "./"
	);

	public static string GetMimeType(string path);
	public static Dictionary<string, string> GetParams(HttpListenerContext ctx);

	public Task WriteTextAsync
	(
		HttpListenerContext ctx,
		string text,
		string mimeType   = "text/plain; charset=utf-8",
		int    statusCode = 200
	);
	public Task WriteFileAsync(HttpListenerContext ctx, string path);

	public void AddRoute(HttpMethod method, string url, Func<HttpListenerContext, Task> handler);
	public Task Start();
	public void Stop();
}
```

コンストラクタで `rootPath` を省略した場合は、exe ファイルが配置されたフォルダがドキュメントルートとなります。

- GetMimeType   : 拡張子から MIME タイプを取得します
- GetParams     : GET または POST のパラメータを取得します
- WriteTextAsync: レスポンスにテキストを書き込みます
- WriteFileAsync: 指定したファイルをレスポンスとして返します
- AddRoute      : ルートを追加します
- Start         : サーバーを起動します
- Stop          : サーバーを停止します

**D4SLog**

開発時に利用できる簡易的なロガーです

```csharp
public static class D4SLog
{
	public static void Write(string text);
	public static List<string> Get();
	public static void Clear();
}
```

- Write: ログを追加し、コンソールへ出力します
- Get  : 現在のログを取得します
- Clear: ログを消去します

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
	return server.WriteTextAsync(ctx, text);
});

server.Start().Wait();
```

---

### D4SHandlers.cs

`D4S.AddRoute` に登録できるハンドラのサンプル集

```csharp
public static class D4SHandlers
{
	public static Func<HttpListenerContext, Task> Hello(D4S server);
	public static Func<HttpListenerContext, Task> StartProcess(D4S server);
	public static Func<HttpListenerContext, Task> SaveToFile(D4S server);
	public static Func<HttpListenerContext, Task> LoadFromFile(D4S server);
	public static Func<HttpListenerContext, Task> CreateDirectoryTree(D4S server);
	public static Func<HttpListenerContext, Task> CreateDirectoryTreeDiff(D4S server);
	public static Func<HttpListenerContext, Task> PathCombine(D4S server);
}
```

- Hello                  : ハンドラ実装例
- StartProcess           : `app` を `args` 付きで呼び出す
- SaveToFile             : `path` に `data` を保存する (現在テキストデータだけ)
- LoadFromFile           : `path` のファイルを読み込む
- CreateDirectoryTree    : `path` のディレクトリツリーを取得する
- CreateDirectoryTreeDiff: `path` の現在の状態に合わせて `json` を差分更新する
- PathCombine            : JSON 配列 `parts` を Path.Combine() で結合し、フルパスを返す

---

## License

MIT License
