using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace ProgressionPlus.Patches;

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.EndCombatInternal))]
public static class CombatScorePatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null)
            return;

        var runState = state.RunState;
        if (runState?.CurrentRoom is not CombatRoom combatRoom)
            return;

        var actIndex = runState.CurrentActIndex;
        var ascensionLevel = runState.AscensionLevel;

        switch (combatRoom.RoomType)
        {
            case RoomType.Monster:
                ScoreManager.AddRegularEncounterWin(actIndex, ascensionLevel);
                break;

            case RoomType.Elite:
                ScoreManager.AddEliteEncounterWin(actIndex, ascensionLevel);
                break;

            case RoomType.Boss:
                ScoreManager.AddBossEncounterWin(actIndex, ascensionLevel);
                break;
        }
    }
}