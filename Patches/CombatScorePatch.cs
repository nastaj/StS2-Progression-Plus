using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rooms;

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

        Player player = LocalContext.GetMe(state);
        var characterId = player.Character.Id.Entry;
        var actIndex = runState.CurrentActIndex;
        var ascensionLevel = runState.AscensionLevel;

        switch (combatRoom.RoomType)
        {
            case RoomType.Monster:
                EssenceManager.AddRegularCombatWin(characterId, actIndex, ascensionLevel);
                break;

            case RoomType.Elite:
                EssenceManager.AddEliteCombatWin(characterId, actIndex, ascensionLevel);
                break;

            case RoomType.Boss:
                EssenceManager.AddBossCombatWin(characterId, actIndex, ascensionLevel);
                break;
        }
    }
}