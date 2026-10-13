using HarmonyLib;
using SeneaLUI.Core;
using SeneaLUI.Inv;
using UnityEngine;

namespace MultiCraftForSeneaLUI;

[HarmonyPatch(typeof(HintBar))]
internal static class HintBarCraftHint
{
    private const float GapHint = 22f;

    [HarmonyPostfix]
    [HarmonyPatch("Build")]
    private static void HideKeyboardCraftHint(HintBar __instance) => HideCraftHint(__instance);

    [HarmonyPostfix]
    [HarmonyPatch("BuildPadCraft")]
    private static void HidePadCraftHint(HintBar __instance) => HideCraftHint(__instance);

    private static void HideCraftHint(HintBar bar)
    {
        if (!BatchCraft.SeneaLCraftingActive())
            return;
        var gui = InventoryGui.instance;
        var craftHint = string.Format(Loc.HintCraft, gui ? gui.m_multiCraftAmount : 5);
        var removed = 0f;
        foreach (var item in bar._items)
        {
            if (!item.Root.gameObject.activeSelf)
                continue;
            if (removed > 0f)
            {
                item.Root.anchoredPosition -= new Vector2(removed, 0f);
            }
            else if (item.Text.text == craftHint)
            {
                removed = item.Width + GapHint;
                item.Root.gameObject.SetActive(false);
            }
        }
        if (removed > 0f)
            bar._root.sizeDelta -= new Vector2(removed, 0f);
    }
}
