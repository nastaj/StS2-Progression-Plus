using System;

namespace ProgressionPlus;

public static class ScoreManager
{
    public static int CurrentScore { get; private set; }

    public static event Action? ScoreChanged;

    public static void Reset()
    {
        CurrentScore = 0;
        ScoreChanged?.Invoke();
    }

    public static void AddRegularEncounterWin(int actIndex, int ascensionLevel)
    {
        AddScore(CalculateScoreGain(3, actIndex, ascensionLevel));
    }

    public static void AddEliteEncounterWin(int actIndex, int ascensionLevel)
    {
        AddScore(CalculateScoreGain(5, actIndex, ascensionLevel));
    }

    public static void AddBossEncounterWin(int actIndex, int ascensionLevel)
    {
        AddScore(CalculateScoreGain(10, actIndex, ascensionLevel));
    }

    private static void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        CurrentScore += amount;
        ScoreChanged?.Invoke();
    }

    private static int CalculateScoreGain(int basePoints, int actIndex, int ascensionLevel)
    {
        var actBonus = Math.Max(0, actIndex);
        var ascensionBonus = Math.Max(0, ascensionLevel) * 5;

        return basePoints + actBonus + ascensionBonus;
    }
}