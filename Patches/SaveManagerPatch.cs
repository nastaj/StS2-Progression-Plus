using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace ProgressionPlus.Patches;

[HarmonyPatch]
public static class SaveManagerPatch
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.InitProgressData))]
    [HarmonyPostfix]
    private static void InitProgressDataPostfix()
    {
        ProgressionSave.Load();
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SwitchProfileId))]
    [HarmonyPostfix]
    private static void SwitchProfileIdPostfix()
    {
        ProgressionSave.Load();
    }
}