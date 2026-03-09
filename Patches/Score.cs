using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ProgressionPlus.Patches;

[HarmonyPatch]
public static class Score
{
    private const string ScoreRootName = "ProgressionPlusScore";
    private const string ScoreIconName = "ScoreIcon";
    private const string ScoreLabelName = "ScoreLabel";
    private const string ScoreIconPath = "res://images/ui/score_icon.png";
    private const float ScoreIconYOffset = 8.0f;
    private const float ScoreLabelYOffset = 0.0f;
    private static readonly Vector2 ScoreOffset = new(12.0f, 0.0f);
    private static readonly Vector2 ScoreIconSize = new(32.0f, 32.0f);

    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar._Ready))]
    private static class TopBarReadyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance)
        {
            EnsureScoreUi(__instance);
            UpdateScoreUi(__instance);
        }
    }

    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar.Initialize))]
    private static class TopBarInitializePatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance)
        {
            EnsureScoreUi(__instance);
            UpdateScoreUi(__instance);
        }
    }

    [HarmonyPatch(typeof(NTopBar), "UpdateNavigation")]
    private static class TopBarNavigationPatch
    {
        [HarmonyPostfix]
        private static void Postfix(NTopBar __instance)
        {
            var scoreRoot = GetScoreRoot(__instance);
            if (scoreRoot == null)
                return;

            __instance.BossIcon.FocusNeighborRight = scoreRoot.GetPath();
            scoreRoot.FocusNeighborLeft = __instance.BossIcon.GetPath();
            scoreRoot.FocusNeighborRight = scoreRoot.GetPath();
            scoreRoot.FocusNeighborTop = scoreRoot.GetPath();
            scoreRoot.FocusNeighborBottom = __instance.BossIcon.FocusNeighborBottom;
        }
    }

    private static void EnsureScoreUi(NTopBar topBar)
    {
        if (!GodotObject.IsInstanceValid(topBar) || !GodotObject.IsInstanceValid(topBar.BossIcon))
            return;

        var parent = topBar.BossIcon.GetParent() as Control;
        if (parent == null)
            return;

        if (parent.GetNodeOrNull<Control>((NodePath)ScoreRootName) != null)
            return;

        var scoreRoot = new Control
        {
            Name = ScoreRootName,
            FocusMode = Control.FocusModeEnum.All,
            MouseFilter = Control.MouseFilterEnum.Pass,
            Size = new Vector2(140.0f, topBar.BossIcon.Size.Y)
        };

        parent.AddChild(scoreRoot);

        var scoreIcon = new TextureRect
        {
            Name = ScoreIconName,
            Texture = LoadScoreTexture(),
            CustomMinimumSize = ScoreIconSize,
            Size = ScoreIconSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        scoreRoot.AddChild(scoreIcon);
        scoreIcon.Position = new Vector2(
            0.0f,
            Mathf.Max(0.0f, (topBar.BossIcon.Size.Y - ScoreIconSize.Y) * 0.5f) + ScoreIconYOffset
        );

        var label = CreateStyledScoreLabel(topBar);
        scoreRoot.AddChild(label);
        label.Position = new Vector2(ScoreIconSize.X + 14.0f, ScoreLabelYOffset);

        RepositionScoreUi(topBar, scoreRoot);
    }

    private static MegaLabel CreateStyledScoreLabel(NTopBar topBar)
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

        label.Name = ScoreLabelName;
        label.MouseFilter = Control.MouseFilterEnum.Ignore;
        label.SetTextAutoSize("0");
        // label.AddThemeColorOverride("font_color", Colors.White);
        // label.AddThemeColorOverride("font_outline_color", Colors.Black);
        return label;
    }

    private static void UpdateScoreUi(NTopBar topBar)
    {
        var scoreRoot = GetScoreRoot(topBar);
        if (scoreRoot == null)
            return;

        RepositionScoreUi(topBar, scoreRoot);

        var scoreLabel = scoreRoot.GetNodeOrNull<MegaLabel>((NodePath)ScoreLabelName);
        if (scoreLabel != null)
            scoreLabel.SetTextAutoSize(GetCurrentScore().ToString());

        var scoreIcon = scoreRoot.GetNodeOrNull<TextureRect>((NodePath)ScoreIconName);
        if (scoreIcon != null && scoreIcon.Texture == null)
            scoreIcon.Texture = LoadScoreTexture();
    }

    private static void RepositionScoreUi(NTopBar topBar, Control scoreRoot)
    {
        scoreRoot.Position = topBar.BossIcon.Position + new Vector2(topBar.BossIcon.Size.X, 0.0f) + ScoreOffset;
    }

    private static Control? GetScoreRoot(NTopBar topBar)
    {
        var parent = topBar.BossIcon.GetParent() as Control;
        return parent?.GetNodeOrNull<Control>((NodePath)ScoreRootName);
    }

    private static Texture2D? LoadScoreTexture()
    {
        return ResourceLoader.Exists(ScoreIconPath)
            ? GD.Load<Texture2D>(ScoreIconPath)
            : null;
    }

    private static int GetCurrentScore()
    {
        return 0;
    }
}