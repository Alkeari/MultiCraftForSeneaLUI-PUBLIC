using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MultiCraft;
using SeneaLUI.Components;
using SeneaLUI.Core;
using SeneaLUI.Inv;
using SeneaLUI.Theme;
using TMPro;
using UnityEngine;

namespace MultiCraftForSeneaLUI;

[HarmonyPatch(typeof(CraftView))]
internal static class AmountControl
{
    private const float ButtonWidth = 38f;
    private const float LabelWidth = 44f;
    private const float Width = ButtonWidth * 2f + LabelWidth;
    private const float RowHeight = 50f;
    private const float Gap = 14f;
    private const float DimmedAlpha = 0.35f;

    internal static CraftView View;
    private static RectTransform _root;
    private static TextMeshProUGUI _label;
    private static CanvasGroup _minus;
    private static CanvasGroup _plus;
    private static int _shownAmount = -1;
    internal static bool Visible;
    internal static bool InPadTick;

    // SeneaL UI 1.1.7's Shift multi line; later versions have no such field, and this class is patched only on 1.1.7.
    private static readonly FieldInfo MultiHostField = typeof(CraftView).GetField("_multiHost", BindingFlags.Instance | BindingFlags.NonPublic);

    private static void HideMultiHost(CraftView view) => ((RectTransform)MultiHostField.GetValue(view)).gameObject.SetActive(false);

    private static void Build(CraftView view)
    {
        View = view;
        _root = Ui.TL((Transform)view._panel, "multicraft_amount", 0f, 0f, Width, RowHeight);

        var minus = new Tb(_root, "minus", "minus", OnMinus, () => new Tip { Body = "Lower the craft amount: Shift+click by the bulk step, Ctrl+click back to 1." });
        minus.SetLabel(null);
        Ui.Place(minus.Root, 0f, 0.5f, 0f, 0f, ButtonWidth, Tb.Height);
        _minus = Ui.Group(minus.Root);

        var style = TextStyles.SegBtn;
        _label = Ui.Label(_root, "amount", style, 0f, ButtonWidth, (RowHeight - style.Size * 1.3f) * 0.5f + style.Baseline(1.3f), LabelWidth, TextAlignmentOptions.Top);
        _label.color = Palette.Parchment;

        var plus = new Tb(_root, "plus", "plus", OnPlus, () => new Tip { Body = "Raise the craft amount: Shift+click by the bulk step, Ctrl+click to the most you can craft." });
        plus.SetLabel(null);
        Ui.Place(plus.Root, 0f, 0.5f, ButtonWidth + LabelWidth, 0f, ButtonWidth, Tb.Height);
        _plus = Ui.Group(plus.Root);

        _root.gameObject.SetActive(false);
        _shownAmount = -1;
        Visible = false;
    }

    private static bool ShouldShow(CraftView view, bool open) =>
        open
        && view._shown
        && BatchCraft.SeneaLCraftingActive()
        && view._button.Root.gameObject.activeSelf
        && view._tab
        && view._dRecipe
        && view._dItem == null
        && !view._dGhost;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CraftView.Tick))]
    private static void Refresh(CraftView __instance, bool open)
    {
        if (View != __instance || !_root)
            Build(__instance);

        var visible = ShouldShow(__instance, open);
        if (visible != Visible)
        {
            Visible = visible;
            _root.gameObject.SetActive(visible);
            if (!visible)
                __instance._button.SetWidth(__instance._detailW);
        }
        if (!visible)
            return;

        HideMultiHost(__instance);
        var y = __instance._button.Root.anchoredPosition.y;
        __instance._button.SetWidth(__instance._detailW - Width - Gap);
        _root.anchoredPosition = new Vector2(__instance._detailX + __instance._detailW - Width, y);

        var amount = BatchCraft.Amount;
        if (amount != _shownAmount)
        {
            _shownAmount = amount;
            _label.text = amount.ToString();
        }

        var idle = !BatchCraft.IsCrafting(__instance._gui);
        SetEnabled(_minus, idle && amount > 1);
        SetEnabled(_plus, idle && !MultiCraft_UI.instance.IsAtMaximumCraftable());
    }

    [HarmonyPostfix]
    [HarmonyPatch("BuildDetail")]
    private static void HideShiftHint(CraftView __instance)
    {
        if (Visible)
            HideMultiHost(__instance);
    }

    [HarmonyTranspiler]
    [HarmonyPatch("TickDetail")]
    private static IEnumerable<CodeInstruction> DetailShowsAmount(IEnumerable<CodeInstruction> instructions) =>
        BatchCraft.ReplaceGetButton(instructions, nameof(BatchCraft.MultiCraftRequested));

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CraftView.PadTick))]
    private static void EnterPadTick() => InPadTick = true;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CraftView.PadTick))]
    private static void LeavePadTick() => InPadTick = false;

    private static void OnMinus()
    {
        if (Input.GetKey(KeyCode.LeftControl))
            MultiCraft_Logic.instance.SetCraftAmount(1);
        else
            MultiCraft_Logic.instance.AddToCraftAmount(Input.GetKey(KeyCode.LeftShift) ? -MultiCraft_UI.BulkStep() : -1);
    }

    private static void OnPlus()
    {
        var recipe = CurrentRecipeInformation.instance;
        if (Input.GetKey(KeyCode.LeftControl))
            MultiCraft_Logic.instance.SetCraftAmount(recipe.HasRequirements ? recipe.GetCurrentMaxCraftable() : MultiCraft_UI.MAX_CRAFT_AMOUNT);
        else
            MultiCraft_Logic.instance.AddToCraftAmount(Input.GetKey(KeyCode.LeftShift) ? MultiCraft_UI.BulkStep() : 1);
    }

    private static void SetEnabled(CanvasGroup group, bool on)
    {
        group.alpha = on ? 1f : DimmedAlpha;
        group.interactable = on;
        group.blocksRaycasts = on;
    }
}

[HarmonyPatch(typeof(Pad), nameof(Pad.Dir))]
internal static class PadAmount
{
    [HarmonyPrefix]
    private static bool DPadStepsAmount(ref Vector2Int __result)
    {
        if (!AmountControl.InPadTick || !AmountControl.Visible || !MultiCraftPlugin.EnableGamepadInput.Value || AmountControl.View == null || BatchCraft.IsCrafting(AmountControl.View._gui))
            return true;
        int delta;
        if (Pad.DownRaw("JoyDPadLeft"))
            delta = -1;
        else if (Pad.DownRaw("JoyDPadRight"))
            delta = 1;
        else
            return true;
        MultiCraft_Logic.instance.AddToCraftAmount(delta);
        __result = Vector2Int.zero;
        return false;
    }
}
