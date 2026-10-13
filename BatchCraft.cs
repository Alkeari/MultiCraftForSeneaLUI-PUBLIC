using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MultiCraft;
using SeneaLUI.Inv;

namespace MultiCraftForSeneaLUI;

[HarmonyPatch(typeof(InventoryGui))]
internal static class BatchCraft
{
    private static readonly MethodInfo GetButton = AccessTools.Method(typeof(ZInput), nameof(ZInput.GetButton), new[] { typeof(string) });

    private static int _batchAmount = 1;

    internal static int Amount => MultiCraft_Logic.instance.CurrentCraftAmount;

    internal static bool IsCrafting(InventoryGui gui) => gui.m_craftTimer >= 0f;

    internal static bool SeneaLCraftingActive()
    {
        var craft = SeneaLUI.Patches.InventoryGuiPatches.Craft;
        return SeneaLUI.Plugin.Active && craft != null && craft._applied;
    }

    [HarmonyPrefix]
    [HarmonyPatch("UpdateRecipe")]
    [HarmonyAfter("maximods.valheim.multicraft")]
    private static void SetAmountBeforeUpdateRecipe(InventoryGui __instance)
    {
        if (!SeneaLCraftingActive())
            return;
        __instance.m_multiCraftAmount = IsCrafting(__instance) ? _batchAmount : Amount;
    }

    // Vanilla's Shift test stays false here: a multi-crafting UpdateRecipe appends "xN" to m_recipeName, which MultiCraft reads as a new recipe and resets its amount.
    [HarmonyTranspiler]
    [HarmonyPatch("UpdateRecipe")]
    private static IEnumerable<CodeInstruction> UpdateRecipeIgnoresShift(IEnumerable<CodeInstruction> instructions) =>
        ReplaceGetButton(instructions, nameof(ShiftHeldWhileIdle));

    [HarmonyPostfix]
    [HarmonyPatch("UpdateRecipe")]
    private static void RequireMaterialsForWholeBatch(InventoryGui __instance, Player player)
    {
        if (!SeneaLCraftingActive() || IsCrafting(__instance) || Amount <= 1 || !__instance.m_craftButton.interactable)
            return;
        var selected = __instance.m_selectedRecipe;
        if (!selected.Recipe || selected.ItemData != null)
            return;
        if (player.NoCostCheat() || ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost))
            return;
        if (!player.HaveRequirements(selected.Recipe, false, 1, Amount))
            __instance.m_craftButton.interactable = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("OnCraftPressed")]
    private static void LockBatchAmount(InventoryGui __instance)
    {
        if (!SeneaLCraftingActive())
            return;
        _batchAmount = Amount;
        __instance.m_multiCraftAmount = _batchAmount;
    }

    [HarmonyTranspiler]
    [HarmonyPatch("OnCraftPressed")]
    private static IEnumerable<CodeInstruction> OnCraftPressedUsesAmount(IEnumerable<CodeInstruction> instructions) =>
        ReplaceGetButton(instructions, nameof(MultiCraftRequested));

    internal static IEnumerable<CodeInstruction> ReplaceGetButton(IEnumerable<CodeInstruction> instructions, string hook)
    {
        var replacement = AccessTools.Method(typeof(BatchCraft), hook);
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(GetButton))
                instruction.operand = replacement;
            yield return instruction;
        }
    }

    private static bool IsMultiCraftButton(string name) => name == "AltPlace" || name == "JoyLStick";

    internal static bool ShiftHeldWhileIdle(string name)
    {
        if (SeneaLCraftingActive() && IsMultiCraftButton(name))
            return false;
        return ZInput.GetButton(name);
    }

    internal static bool MultiCraftRequested(string name)
    {
        if (SeneaLCraftingActive() && IsMultiCraftButton(name))
            return name == "AltPlace" && Amount > 1;
        return ZInput.GetButton(name);
    }
}
