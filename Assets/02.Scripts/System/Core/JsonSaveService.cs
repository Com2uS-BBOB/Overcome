using System.IO;
using UnityEngine;

public static class JsonSaveService
{
    private static readonly string SavePath = Application.persistentDataPath;

    public static void Save<T>(string key, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        string path = GetFilePath(key);
        File.WriteAllText(path, json);
    }

    public static T Load<T>(string key) where T : new()
    {
        string path = GetFilePath(key);

        if (!File.Exists(path))
        {
            return new T();
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    public static bool Exists(string key)
    {
        return File.Exists(GetFilePath(key));
    }

    public static void Delete(string key)
    {
        string path = GetFilePath(key);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string GetFilePath(string key)
    {
        return Path.Combine(SavePath, $"{key}.json");
    }
}
