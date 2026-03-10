using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;

namespace ProgressionPlus.Patches;

[HarmonyPatch(typeof(Player), nameof(Player.CreateForNewRun),
    new[] { typeof(CharacterModel), typeof(UnlockState), typeof(ulong) })]
public static class ApplyUpgradePatch
{
    private const bool EnableDebugUpgrades = false;

    [HarmonyPostfix]
    private static void Postfix(Player __result)
    {
        var characterId = __result.Character.Id.Entry;

        if (EnableDebugUpgrades)
        {
            UpgradeManager.DebugSetRank(characterId, UpgradeManager.BonusStartingGoldUpgradeId, 3);
            UpgradeManager.DebugSetRank(characterId, UpgradeManager.BonusMaxHpUpgradeId, 5);
        }

        ApplyStartingGoldBonus(__result, characterId);
        ApplyMaxHpBonus(__result, characterId);
    }

    private static void ApplyStartingGoldBonus(Player player, string characterId)
    {
        var bonusGold = UpgradeManager.GetStartingGoldBonus(characterId);
        if (bonusGold <= 0)
            return;

        player.Gold += bonusGold;
    }

    private static void ApplyMaxHpBonus(Player player, string characterId)
    {
        var bonusMaxHp = UpgradeManager.GetMaxHpBonus(characterId);
        if (bonusMaxHp <= 0)
            return;

        var creature = player.Creature;
        var currentMaxHp = creature.MaxHp;
        var newMaxHp = currentMaxHp + bonusMaxHp;

        var setMaxHpInternal = AccessTools.Method(creature.GetType(), "SetMaxHpInternal");
        var setCurrentHpInternal = AccessTools.Method(creature.GetType(), "SetCurrentHpInternal");

        if (setMaxHpInternal == null || setCurrentHpInternal == null)
            return;

        setMaxHpInternal.Invoke(creature, new object[] { (decimal)newMaxHp });
        setCurrentHpInternal.Invoke(creature, new object[] { (decimal)newMaxHp });
    }
}