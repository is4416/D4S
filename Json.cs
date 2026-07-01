using System;
using System.IO;
using System.Collections.Generic;

// .NET Framework
using System.Web.Script.Serialization;

// .NET 8
// using System.Text.Json;

// ---------- ---------- ---------- ---------- ----------
// Json
// ---------- ---------- ---------- ---------- ----------

public static class Json
{
	// public: static
	// ---------- ---------- ----------
	// Stringify
	// ---------- ---------- ----------
	/**
	 * オブジェクトを JSON 化する
	 */
	public static string Stringify(object obj)
	{
		// .NET Framework
		var serializer            = new JavaScriptSerializer();
		serializer.MaxJsonLength  = int.MaxValue;
		serializer.RecursionLimit = 100;
		return serializer.Serialize(obj);

		// .NET 8
		// return JsonSerializer.Serialize(obj);
	}

	// public: static
	// ---------- ---------- ----------
	// Parse
	// ---------- ---------- ----------
	/**
	 * JSON文字列をオブジェクト化する
	 */
	public static Dictionary<string, object> Parse(string json)
	{
		// .NET Framework
		var serializer            = new JavaScriptSerializer();
		serializer.MaxJsonLength  = int.MaxValue;
		serializer.RecursionLimit = 100;
		return (Dictionary<string, object>)serializer.DeserializeObject(json);

		// .NET 8
		// return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
	}
}

// ---------- ---------- ---------- ---------- ----------
// JsonItem
// ---------- ---------- ---------- ---------- ----------

public class JsonItem
{
	// ---------- ---------- ----------
	// Field
	// ---------- ---------- ----------

	protected readonly FileSystemInfo _info;

	// ---------- ---------- ----------
	// Constructor
	// ---------- ---------- ----------

	public JsonItem(FileSystemInfo info)
	{
		_info = info;
	}

	// public: vartual
	// ---------- ---------- ----------
	// ToObject
	// ---------- ---------- ----------
	/**
	 * JsonItem を JSON 変換可能オブジェクトに変換する
	 * 返値は Dictionary<string, object>
	 */
	public virtual Dictionary<string, object> ToObject()
	{
		var result = new Dictionary<string, object>();

		result["attributes"]        = (long)_info.Attributes;
		result["creationTimeUtc"]   = _info.CreationTimeUtc.ToString("o");
		result["lastWriteTimeUtc"]  = _info.LastWriteTimeUtc.ToString("o");
		result["lastAccessTimeUtc"] = _info.LastAccessTimeUtc.ToString("o");

		return result;
	}
}

// ---------- ---------- ---------- ---------- ----------
// JsonFile : JsonItem
// ---------- ---------- ---------- ---------- ----------

public class JsonFile : JsonItem
{
	// ---------- ---------- ----------
	// Constructor
	// ---------- ---------- ----------

	public JsonFile(FileInfo info) : base(info)
	{
	}

	// public: override
	// ---------- ---------- ----------
	// ToObject
	// ---------- ---------- ----------

	public override Dictionary<string, object> ToObject()
	{
		var result = base.ToObject();

		var fileInfo = (FileInfo)_info;

		result["size"]      = fileInfo.Length;
		result["extension"] = fileInfo.Extension;

		return result;
	}
}

// ---------- ---------- ---------- ---------- ----------
// JsonDirectory : JsonItem
// ---------- ---------- ---------- ---------- ----------

public class JsonDirectory : JsonItem
{
	// ---------- ---------- ----------
	// Field
	// ---------- ---------- ----------

	private readonly Dictionary<string, JsonFile>      _files;
	private readonly Dictionary<string, JsonDirectory> _directories;

	// ---------- ---------- ----------
	// Constructor
	// ---------- ---------- ----------
	/**
	 * depth: 子ディレクトリを再帰的に取得する深さ
	 * -1: 制限なし
	 *  0: 子ディレクトリを取得しない
	 */
	public JsonDirectory(DirectoryInfo info, int depth = -1) : base(info)
	{
		_files       = new Dictionary<string, JsonFile>();
		_directories = new Dictionary<string, JsonDirectory>();

		// add files
		try
		{
			foreach (var fileInfo in info.EnumerateFiles())
			{
				try
				{
					_files[fileInfo.Name] = new JsonFile(fileInfo);
				}
				catch (Exception err)
				{
					Console.WriteLine("error: add file " + fileInfo.Name + err);
				}
			}
		}
		catch (Exception err)
		{
			Console.WriteLine("error: GetFiles " + err);
		}

		// add directories
		try
		{
			if (depth == 0)
			{
				return;
			}

			int nextDepth = depth < 0 ? -1 : depth - 1;

			foreach (var directoryInfo in info.EnumerateDirectories())
			{
				if ((directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0)
				{
					continue;
				}

				try
				{
					_directories[directoryInfo.Name] = new JsonDirectory(directoryInfo, nextDepth);
				}
				catch (Exception err)
				{
					Console.WriteLine("error: add directory " + directoryInfo.Name + err);
				}
			}
		}
		catch (Exception err)
		{
			Console.WriteLine("error: GetDirectories " + err);
		}
	}

	// public: static
	// ---------- ---------- ----------
	// Diff
	// ---------- ---------- ----------
	/**
	 * オブジェクトの差分更新
	 */
	public static Dictionary<string, object> Diff(Dictionary<string, object> json, DirectoryInfo info)
	{
		if (!json.ContainsKey("files"))
		{
			Console.WriteLine("JsonDirectory.Diff(): 'files' not found.");
			throw new ArgumentException("JsonDirectory.Diff(): 'files' not found.");
		}

		if (!json.ContainsKey("directories"))
		{
			Console.WriteLine("JsonDirectory.Diff(): 'directories' not found.");
			throw new ArgumentException("JsonDirectory.Diff(): 'directories' not found.");
		}

		json["attributes"]        = (long)info.Attributes;
		json["creationTimeUtc"]   = info.CreationTimeUtc.ToString("o");
		json["lastAccessTimeUtc"] = info.LastAccessTimeUtc.ToString("o");
		json["lastWriteTimeUtc"]  = info.LastWriteTimeUtc.ToString("o");

		var files       = (Dictionary<string, object>) json["files"];
		var directories = (Dictionary<string, object>) json["directories"];

		// files
		try
		{
			var fileInfoList    = new List<FileInfo>(info.EnumerateFiles());
			var fileNames       = new HashSet<string>();
			var removeFileNames = new List<string>();

			// create fileNames
			foreach(var f in fileInfoList)
			{
				fileNames.Add(f.Name);
			}

			// create removeFileList
			foreach(var key in files.Keys)
			{
				if (!fileNames.Contains(key))
				{
					removeFileNames.Add(key);
				}
			}

			// delete file
			foreach(var key in removeFileNames)
			{
				files.Remove(key);
			}

			// new + update
			foreach(var f in fileInfoList)
			{
				string key = f.Name;

				if (files.ContainsKey(key))
				{
					// update
					var item = (Dictionary<string, object>) files[key];
					string lastWriteTimeUtc = (string) item["lastWriteTimeUtc"];
					if (lastWriteTimeUtc != f.LastWriteTimeUtc.ToString("o"))
					{
						files[key] = new JsonFile(f).ToObject();
					}
				}
				else
				{
					// new
					files[key] = new JsonFile(f).ToObject();
				}
			}

		}
		catch (Exception err)
		{
			Console.WriteLine("error: diff EnumerateFiles " + err.Message);
		}

		// directories
		try
		{
			var directoryInfoList    = new List<DirectoryInfo>(info.EnumerateDirectories());
			var directoryNames       = new HashSet<string>();
			var removeDirectoryNames = new List<string>();

			// create directoryNames
			foreach(var d in directoryInfoList)
			{
				if ((d.Attributes & FileAttributes.ReparsePoint) != 0)
				{
					continue;
				}
				directoryNames.Add(d.Name);
			}

			// create removeDirectoryNames
			foreach(var key in directories.Keys)
			{
				if (!directoryNames.Contains(key))
				{
					removeDirectoryNames.Add(key);
				}
			}

			// delete directory
			foreach(var key in removeDirectoryNames)
			{
				directories.Remove(key);
			}

			// new + update
			foreach(var d in directoryInfoList)
			{
				if ((d.Attributes & FileAttributes.ReparsePoint) != 0)
				{
					continue;
				}

				string key = d.Name;

				if (directories.ContainsKey(key))
				{
					// update
					var item = (Dictionary<string, object>) directories[key];
					string lastWriteTimeUtc = (string) item["lastWriteTimeUtc"];
					if (lastWriteTimeUtc != d.LastWriteTimeUtc.ToString("o"))
					{
						directories[key] = JsonDirectory.Diff((Dictionary<string, object>) directories[key], d);
					}
				}
				else
				{
					// new
					directories[key] = new JsonDirectory(d).ToObject();
				}
			}
		}
		catch (Exception err)
		{
			Console.WriteLine("error: diff EnumerateDirectories " + err.Message);
		}

		return json;
	}

	// private
	// ---------- ---------- ----------
	// ToObjectCore
	// ---------- ---------- ----------

	private Dictionary<string, object> ToObjectCore()
	{
		var result = base.ToObject();

		// files
		var files = new Dictionary<string, object>();

		foreach (var kv in _files)
		{
			files[kv.Key] = kv.Value.ToObject();
		}

		result["files"] = files;

		// directories
		var directories = new Dictionary<string, object>();

		foreach (var kv in _directories)
		{
			directories[kv.Key] = kv.Value.ToObjectCore();
		}

		result["directories"] = directories;

		// result
		return result;
	}

	// public: override
	// ---------- ---------- ----------
	// ToObject
	// ---------- ---------- ----------

	public override Dictionary<string, object> ToObject()
	{
		var result = ToObjectCore();

		result["path"] = ((DirectoryInfo)_info).FullName;

		return result;
	}
}
