using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;

// ---------- ---------- ---------- ---------- ----------
// Json
// ---------- ---------- ---------- ---------- ----------

public class Json
{
	// static
	// ---------- ---------- ----------
	// Stringify
	// ---------- ---------- ----------
	/**
	 * オブジェクトを JSON 化する
	 */
	static string Stringify(object obj)
	{
		var serializer = new JavaScriptSerializer();

		serializer.MaxJsonLength  = int.MaxValue;
		serializer.RecursionLimit = 100;

		return serializer.Serialize(obj);
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
	public virtual object ToObject()
	{
		var result = new Dictionary<string, object>();

		result["name"]           = _info.Name;
		result["attributes"]     = (long)_info.Attributes;
		result["creationTime"]   = _info.CreationTimeUtc.ToString("o");
		result["lastWriteTime"]  = _info.LastWriteTimeUtc.ToString("o");
		result["lastAccessTime"] = _info.LastAccessTimeUtc.ToString("o");

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

	public override object ToObject()
	{
		var result = (Dictionary<string, object>)base.ToObject();

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

	public JsonDirectory(DirectoryInfo info) : base(info)
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
			foreach (var directoryInfo in info.EnumerateDirectories())
			{
				if ((directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0)
				{
					continue;
				}

				try
				{
					_directories[directoryInfo.Name] = new JsonDirectory(directoryInfo);
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

	// public: override
	// ---------- ---------- ----------
	// ToObject
	// ---------- ---------- ----------

	public override object ToObject()
	{
		var result = (Dictionary<string, object>)base.ToObject();

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
			directories[kv.Key] = kv.Value.ToObject();
		}

		result["directories"] = directories;

		// result
		return result;
	}
}
