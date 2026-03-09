namespace ProgressionPlus.Patches;

using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;

[HarmonyPatch(typeof(Player), nameof(Player.CreateForNewRun), 
    new Type[] { typeof(CharacterModel), typeof(UnlockState), typeof(ulong) })]
public class GoldPatch
{
    static void Postfix(Player __result)
    {
        // Give 999 gold at the start of the run
        __result.Gold = 999;
    }
}