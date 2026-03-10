using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Runs;

namespace ProgressionPlus.Patches;

[HarmonyPatch]
public static class EssencePatch
{
    private const string EssenceRootName = "ProgressionPlusEssence";
    private const string EssenceIconName = "EssenceIcon";
    private const string EssenceLabelName = "EssenceLabel";
    private const string EssenceIconPath = "res://images/ui/essence_icon.png";
    private const float EssenceIconYOffset = 8.0f;
    private const float EssenceLabelYOffset = 0.0f;
    private static readonly Vector2 EssenceOffset = new(12.0f, 0.0f);
    private static readonly Vector2 EssenceIconSize = new(32.0f, 32.0f);
    private static readonly HoverTip EssenceHoverTip = new(
        new LocString("static_hover_tips", "ESSENCE.title"),
        new LocString("static_hover_tips", "ESSENCE.description")
    );

    private static NTopBar? _currentTopBar;
    private static bool _essenceChangedSubscribed;

    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar._Ready))]
    private static class TopBarReadyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance)
        {
            _currentTopBar = __instance;
            EnsureEssenceChangedSubscription();
            EnsureEssenceUi(__instance);
            UpdateEssenceUi(__instance);
        }
    }

    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar.Initialize))]
    private static class TopBarInitializePatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance, IRunState runState)
        {
            _currentTopBar = __instance;
            EnsureEssenceChangedSubscription();

            var player = LocalContext.GetMe(runState);
            EssenceManager.SetCurrentCharacter(player.Character.Id.Entry);

            EnsureEssenceUi(__instance);
            UpdateEssenceUi(__instance);
        }
    }

    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar._ExitTree))]
    private static class TopBarExitTreePatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance)
        {
            if (_currentTopBar == __instance)
                _currentTopBar = null;
        }
    }

    [HarmonyPatch(typeof(NTopBar), "UpdateNavigation")]
    private static class TopBarNavigationPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance)
        {
            var essenceRoot = GetEssenceRoot(__instance);
            if (essenceRoot == null)
                return;

            __instance.BossIcon.FocusNeighborRight = essenceRoot.GetPath();
            essenceRoot.FocusNeighborLeft = __instance.BossIcon.GetPath();
            essenceRoot.FocusNeighborRight = essenceRoot.GetPath();
            essenceRoot.FocusNeighborTop = essenceRoot.GetPath();
            essenceRoot.FocusNeighborBottom = __instance.BossIcon.FocusNeighborBottom;
        }
    }

    private static void EnsureEssenceChangedSubscription()
    {
        if (_essenceChangedSubscribed)
            return;

        EssenceManager.EssenceChanged += OnEssenceChanged;
        _essenceChangedSubscribed = true;
    }

    private static void OnEssenceChanged()
    {
        if (_currentTopBar == null || !GodotObject.IsInstanceValid(_currentTopBar))
            return;

        UpdateEssenceUi(_currentTopBar);
    }

    private static void EnsureEssenceUi(NTopBar topBar)
    {
        if (!GodotObject.IsInstanceValid(topBar) || !GodotObject.IsInstanceValid(topBar.BossIcon))
            return;

        var parent = topBar.BossIcon.GetParent() as Control;
        if (parent == null)
            return;

        if (parent.GetNodeOrNull<Control>((NodePath)EssenceRootName) != null)
            return;

        var essenceRoot = new Control
        {
            Name = EssenceRootName,
            FocusMode = Control.FocusModeEnum.All,
            MouseFilter = Control.MouseFilterEnum.Stop,
            Size = new Vector2(140.0f, topBar.BossIcon.Size.Y),
            CustomMinimumSize = new Vector2(140.0f, topBar.BossIcon.Size.Y),
        };

        essenceRoot.MouseEntered += () => ShowEssenceHoverTip(essenceRoot);
        essenceRoot.MouseExited += () => HideEssenceHoverTip(essenceRoot);
        essenceRoot.FocusEntered += () => ShowEssenceHoverTip(essenceRoot);
        essenceRoot.FocusExited += () => HideEssenceHoverTip(essenceRoot);

        parent.AddChild(essenceRoot);

        var essenceIcon = new TextureRect
        {
            Name = EssenceIconName,
            Texture = LoadEssenceTexture(),
            CustomMinimumSize = EssenceIconSize,
            Size = EssenceIconSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        essenceRoot.AddChild(essenceIcon);
        essenceIcon.Position = new Vector2(
            0.0f,
            Mathf.Max(0.0f, (topBar.BossIcon.Size.Y - EssenceIconSize.Y) * 0.5f) + EssenceIconYOffset
        );

        var label = CreateStyledEssenceLabel(topBar);
        essenceRoot.AddChild(label);
        label.Position = new Vector2(EssenceIconSize.X + 14.0f, EssenceLabelYOffset);

        RepositionEssenceUi(topBar, essenceRoot);
    }

    private static MegaLabel CreateStyledEssenceLabel(NTopBar topBar)
    {
        var templateLabel = topBar.FloorIcon.GetNodeOrNull<MegaLabel>((NodePath)"%FloorNumLabel");
        MegaLabel label;

        if (templateLabel != null)
        {
            label = templateLabel.Duplicate() as MegaLabel ?? new MegaLabel();
        }
        else
        {
            label = new MegaLabel();
        }

        label.Name = EssenceLabelName;
        label.MouseFilter = Control.MouseFilterEnum.Ignore;
        label.SetTextAutoSize("0");
        // label.AddThemeColorOverride("font_color", Colors.White);
        // label.AddThemeColorOverride("font_outline_color", Colors.Black);
        return label;
    }

    private static void UpdateEssenceUi(NTopBar topBar)
    {
        var essenceRoot = GetEssenceRoot(topBar);
        if (essenceRoot == null)
            return;

        RepositionEssenceUi(topBar, essenceRoot);
        var essenceLabel = essenceRoot.GetNodeOrNull<MegaLabel>((NodePath)EssenceLabelName);
        if (essenceLabel != null)
            essenceLabel.SetTextAutoSize(GetCurrentEssence().ToString());

        var essenceIcon = essenceRoot.GetNodeOrNull<TextureRect>((NodePath)EssenceIconName);
        if (essenceIcon != null && essenceIcon.Texture == null)
            essenceIcon.Texture = LoadEssenceTexture();
    }

    private static void RepositionEssenceUi(NTopBar topBar, Control essenceRoot)
    {
        essenceRoot.Position = topBar.BossIcon.Position + new Vector2(topBar.BossIcon.Size.X, 0.0f) + EssenceOffset;
    }

    private static Control? GetEssenceRoot(NTopBar topBar)
    {
        var parent = topBar.BossIcon.GetParent() as Control;
        return parent?.GetNodeOrNull<Control>((NodePath)EssenceRootName);
    }

    private static void ShowEssenceHoverTip(Control essenceRoot)
    {
        if (!GodotObject.IsInstanceValid(essenceRoot))
            return;

        var anchor = essenceRoot.GetNodeOrNull<Control>((NodePath)EssenceIconName) ?? essenceRoot;

        NHoverTipSet.CreateAndShow(essenceRoot, EssenceHoverTip).GlobalPosition =
            anchor.GlobalPosition + new Vector2(0.0f, anchor.Size.Y + 20.0f);
    }

    private static void HideEssenceHoverTip(Control essenceRoot)
    {
        if (!GodotObject.IsInstanceValid(essenceRoot))
            return;

        NHoverTipSet.Remove(essenceRoot);
    }

    private static Texture2D? LoadEssenceTexture()
    {
        return ResourceLoader.Exists(EssenceIconPath)
            ? GD.Load<Texture2D>(EssenceIconPath)
            : null;
    }

    private static int GetCurrentEssence()
    {
        return EssenceManager.CurrentEssence;
    }
}