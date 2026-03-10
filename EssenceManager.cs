using System;
using System.Collections.Generic;

namespace ProgressionPlus;

public static class EssenceManager
{
    private static readonly Dictionary<string, int> EssenceByCharacterId = new();

    public static string? CurrentCharacterId { get; private set; }

    public static int CurrentEssence => GetEssence(CurrentCharacterId);

    public static event Action? EssenceChanged;

    public static void SetCurrentCharacter(string? characterId)
    {
        CurrentCharacterId = characterId;
        EssenceChanged?.Invoke();
    }

    public static int GetEssence(string? characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return 0;

        return EssenceByCharacterId.GetValueOrDefault(characterId, 0);
    }

    public static void AddRegularCombatWin(string? characterId, int actIndex, int ascensionLevel)
    {
        AddEssence(characterId, CalculateEssenceGain(3, actIndex, ascensionLevel));
    }

    public static void AddEliteCombatWin(string? characterId, int actIndex, int ascensionLevel)
    {
        AddEssence(characterId, CalculateEssenceGain(5, actIndex, ascensionLevel));
    }

    public static void AddBossCombatWin(string? characterId, int actIndex, int ascensionLevel)
    {
        AddEssence(characterId, CalculateEssenceGain(10, actIndex, ascensionLevel));
    }

    public static void AddEssence(string? characterId, int amount)
    {
        if (string.IsNullOrWhiteSpace(characterId) || amount <= 0)
            return;

        var current = EssenceByCharacterId.GetValueOrDefault(characterId, 0);
        EssenceByCharacterId[characterId] = current + amount;

        if (characterId == CurrentCharacterId)
            EssenceChanged?.Invoke();
    }

    public static void SetEssence(string? characterId, int amount)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return;

        EssenceByCharacterId[characterId] = Math.Max(0, amount);

        if (characterId == CurrentCharacterId)
            EssenceChanged?.Invoke();
    }

    private static int CalculateEssenceGain(int basePoints, int actIndex, int ascensionLevel)
    {
        var actBonus = Math.Max(0, actIndex);
        var ascensionBonus = Math.Max(0, ascensionLevel) * 5;

        return basePoints + actBonus + ascensionBonus;
    }
}