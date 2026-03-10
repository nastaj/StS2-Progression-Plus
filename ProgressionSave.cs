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
                LoadEmpty();
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<ProgressionSaveData>(json) ?? new ProgressionSaveData();

            EssenceManager.ImportSaveData(data.CharacterEssence);
            UpgradeManager.ImportSaveData(data.CharacterUpgrades);
        }
        catch
        {
            LoadEmpty();
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

            var data = new ProgressionSaveData
            {
                CharacterEssence = EssenceManager.ExportSaveData(),
                CharacterUpgrades = UpgradeManager.ExportSaveData()
            };

            var json = JsonSerializer.Serialize(data, SerializerOptions);
            File.WriteAllText(path, json);
        }
        catch
        {
            // Intentionally swallow for now.
        }
    }

    private static void LoadEmpty()
    {
        EssenceManager.ImportSaveData(null);
        UpgradeManager.ImportSaveData(null);
    }

    private static string GetGlobalProfileSavePath(string fileName)
    {
        var godotPath = SaveManager.Instance.GetProfileScopedPath(Path.Combine("saves", fileName));
        return ProjectSettings.GlobalizePath(godotPath);
    }
}