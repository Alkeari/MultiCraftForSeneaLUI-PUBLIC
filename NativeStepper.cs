using System.Reflection;
using HarmonyLib;
using MultiCraft;
using SeneaLUI.Core;
using SeneaLUI.Inv;
using UnityEngine;

namespace MultiCraftForSeneaLUI;

// SeneaL UI 1.1.8 and later craft a chosen amount with their own stepper, which writes m_multiCraftAmount every frame. MultiCraft overwrites that field at the start of every UpdateRecipe, the call in which vanilla finishes a craft, so without this a batch crafted one item.
internal static class NativeStepper
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    // Looked up without AccessTools so that SeneaL UI 1.1.7, which lacks them, logs no warning.
    private static readonly FieldInfo StepOnField = typeof(CraftView).GetField("_stepOn", Members);
    private static readonly MethodInfo OnStepMethod = typeof(CraftView).GetMethod("OnStep", Members, null, new[] { typeof(int) }, null);

    internal static bool Present => StepOnField != null && OnStepMethod != null;

    internal static int SeneaLAmount;

    internal static CraftView PadTickView;

    internal static bool StepOn(CraftView view) => (bool)StepOnField.GetValue(view);

    internal static void Step(CraftView view, int delta) => OnStepMethod.Invoke(view, new object[] { delta });
}

[HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
internal static class SaveSeneaLAmount
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix(InventoryGui __instance) => NativeStepper.SeneaLAmount = __instance.m_multiCraftAmount;
}

[HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
internal static class RestoreSeneaLAmount
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    private static void Prefix(InventoryGui __instance)
    {
        if (BatchCraft.SeneaLCraftingActive())
            __instance.m_multiCraftAmount = NativeStepper.SeneaLAmount;
    }
}

[HarmonyPatch(typeof(CraftView), nameof(CraftView.PadTick))]
internal static class NativePadTickScope
{
    [HarmonyPrefix]
    private static void Enter(CraftView __instance) => NativeStepper.PadTickView = __instance;

    [HarmonyPostfix]
    private static void Leave() => NativeStepper.PadTickView = null;
}

// MultiCraft binds D-Pad Left and Right to its craft amount, which SeneaL UI's window no longer shows, while SeneaL UI changes category on the same buttons; the D-Pad steps SeneaL UI's own amount instead.
[HarmonyPatch(typeof(Pad), nameof(Pad.Dir))]
internal static class NativePadAmount
{
    [HarmonyPrefix]
    private static bool DPadStepsAmount(ref Vector2Int __result)
    {
        var view = NativeStepper.PadTickView;
        if (view == null || !MultiCraftPlugin.EnableGamepadInput.Value || !NativeStepper.StepOn(view) || BatchCraft.IsCrafting(view._gui))
            return true;
        int delta;
        if (Pad.DownRaw("JoyDPadLeft"))
            delta = -1;
        else if (Pad.DownRaw("JoyDPadRight"))
            delta = 1;
        else
            return true;
        NativeStepper.Step(view, delta);
        __result = Vector2Int.zero;
        return false;
    }
}
