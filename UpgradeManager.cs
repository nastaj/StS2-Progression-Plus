using System;
using System.Collections.Generic;

namespace ProgressionPlus;

public static class UpgradeManager
{
    public const string BonusStartingGoldUpgradeId = "bonus_starting_gold";
    public const string BonusMaxHpUpgradeId = "bonus_max_hp";

    private static readonly Dictionary<string, UpgradeDefinition> DefinitionsById = new()
    {
        [BonusStartingGoldUpgradeId] = new UpgradeDefinition
        {
            Id = BonusStartingGoldUpgradeId,
            Title = "Starting Gold",
            Description = "Gain +10 starting gold per rank.",
            MaxRank = 30,
            GetCostForNextRank = currentRank => 100 + currentRank * 50
        },
        [BonusMaxHpUpgradeId] = new UpgradeDefinition
        {
            Id = BonusMaxHpUpgradeId,
            Title = "Max HP",
            Description = "Gain +1 maximum HP per rank.",
            MaxRank = 20,
            GetCostForNextRank = currentRank => 100 + currentRank * 100
        }
    };

    private static readonly Dictionary<string, Dictionary<string, int>> UpgradeRanksByCharacterId = new();

    public static event Action? UpgradesChanged;

    public static IReadOnlyCollection<UpgradeDefinition> AllDefinitions => DefinitionsById.Values;

    public static UpgradeDefinition GetDefinition(string upgradeId)
    {
        if (!DefinitionsById.TryGetValue(upgradeId, out var definition))
            throw new InvalidOperationException($"Unknown upgrade id: {upgradeId}");

        return definition;
    }

    public static int GetRank(string? characterId, string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return 0;

        if (!UpgradeRanksByCharacterId.TryGetValue(characterId, out var upgrades))
            return 0;

        return upgrades.GetValueOrDefault(upgradeId, 0);
    }

    public static bool IsMaxRank(string? characterId, string upgradeId)
    {
        var definition = GetDefinition(upgradeId);
        return GetRank(characterId, upgradeId) >= definition.MaxRank;
    }

    public static int GetCostForNextRank(string? characterId, string upgradeId)
    {
        var definition = GetDefinition(upgradeId);
        var currentRank = GetRank(characterId, upgradeId);

        if (currentRank >= definition.MaxRank)
            return -1;

        return definition.GetCostForNextRank(currentRank);
    }

    public static bool CanPurchase(string? characterId, string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return false;

        if (IsMaxRank(characterId, upgradeId))
            return false;

        var cost = GetCostForNextRank(characterId, upgradeId);
        if (cost <= 0)
            return false;

        return EssenceManager.GetEssence(characterId) >= cost;
    }

    public static bool Purchase(string? characterId, string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return false;

        if (!CanPurchase(characterId, upgradeId))
            return false;

        var cost = GetCostForNextRank(characterId, upgradeId);
        var currentRank = GetRank(characterId, upgradeId);

        EssenceManager.AddEssence(characterId, -cost);
        SetRank(characterId, upgradeId, currentRank + 1);

        return true;
    }

    public static int GetStartingGoldBonus(string? characterId)
    {
        return GetRank(characterId, BonusStartingGoldUpgradeId) * 10;
    }

    public static int GetMaxHpBonus(string? characterId)
    {
        return GetRank(characterId, BonusMaxHpUpgradeId);
    }

    public static Dictionary<string, Dictionary<string, int>> ExportSaveData()
    {
        var copy = new Dictionary<string, Dictionary<string, int>>();

        foreach (var characterEntry in UpgradeRanksByCharacterId)
            copy[characterEntry.Key] = new Dictionary<string, int>(characterEntry.Value);

        return copy;
    }

    public static void ImportSaveData(Dictionary<string, Dictionary<string, int>>? saveData)
    {
        UpgradeRanksByCharacterId.Clear();

        if (saveData == null)
        {
            UpgradesChanged?.Invoke();
            return;
        }

        foreach (var characterEntry in saveData)
        {
            var sanitizedUpgrades = new Dictionary<string, int>();

            foreach (var upgradeEntry in characterEntry.Value)
            {
                if (!DefinitionsById.TryGetValue(upgradeEntry.Key, out var definition))
                    continue;

                sanitizedUpgrades[upgradeEntry.Key] = Math.Clamp(upgradeEntry.Value, 0, definition.MaxRank);
            }

            UpgradeRanksByCharacterId[characterEntry.Key] = sanitizedUpgrades;
        }

        UpgradesChanged?.Invoke();
    }

    public static void DebugSetRank(string? characterId, string upgradeId, int rank)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return;

        SetRank(characterId, upgradeId, rank);
    }

    public static void DebugAddRanks(string? characterId, string upgradeId, int rankDelta)
    {
        if (string.IsNullOrWhiteSpace(characterId) || rankDelta == 0)
            return;

        var currentRank = GetRank(characterId, upgradeId);
        SetRank(characterId, upgradeId, currentRank + rankDelta);
    }

    private static void SetRank(string characterId, string upgradeId, int rank)
    {
        var definition = GetDefinition(upgradeId);

        if (!UpgradeRanksByCharacterId.TryGetValue(characterId, out var upgrades))
        {
            upgrades = new Dictionary<string, int>();
            UpgradeRanksByCharacterId[characterId] = upgrades;
        }

        upgrades[upgradeId] = Math.Clamp(rank, 0, definition.MaxRank);

        ProgressionSave.Save();
        UpgradesChanged?.Invoke();
    }
}