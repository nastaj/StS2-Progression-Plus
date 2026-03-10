using System;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Saves;

namespace ProgressionPlus;

public static class ProgressionSave
{
    private const string SaveFileName = "progression_plus.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static void Load()
    {
        try
        {
            var path = GetGlobalProfileSavePath(SaveFileName);

            if (!File.Exists(path))
            {
                EssenceManager.LoadFromSaveData(new ProgressionSaveData());
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<ProgressionSaveData>(json) ?? new ProgressionSaveData();
            EssenceManager.LoadFromSaveData(data);
        }
        catch
        {
            EssenceManager.LoadFromSaveData(new ProgressionSaveData());
        }
    }

    public static void Save()
    {
        try
        {
            var path = GetGlobalProfileSavePath(SaveFileName);
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var data = EssenceManager.ToSaveData();
            var json = JsonSerializer.Serialize(data, SerializerOptions);

            File.WriteAllText(path, json);
        }
        catch
        {
            // Intentionally swallow for now.
        }
    }

    private static string GetGlobalProfileSavePath(string fileName)
    {
        var godotPath = SaveManager.Instance.GetProfileScopedPath(Path.Combine("saves", fileName));
        return ProjectSettings.GlobalizePath(godotPath);
    }
}