using BepInEx;
using HarmonyLib;

namespace MultiCraftForSeneaLUI;

[BepInPlugin(Guid, Name, PluginVersion.Value)]
[BepInProcess("valheim.exe")]
[BepInDependency(MultiCraftGuid)]
[BepInDependency(SeneaLUiGuid)]
public class Plugin : BaseUnityPlugin
{
    public const string Guid = "alkeari.valheim.multicraftforsenealui";
    public const string Name = "MultiCraft for SeneaL UI";
    private const string MultiCraftGuid = "maximods.valheim.multicraft";
    private const string SeneaLUiGuid = "seneaL.valheim.ui";

    private Harmony _harmony;

    private void Awake()
    {
        _harmony = new Harmony(Guid);
        if (NativeStepper.Present)
        {
            _harmony.PatchAll(typeof(SaveSeneaLAmount));
            _harmony.PatchAll(typeof(RestoreSeneaLAmount));
            _harmony.PatchAll(typeof(NativePadTickScope));
            _harmony.PatchAll(typeof(NativePadAmount));
            Logger.LogInfo("SeneaL UI crafts a chosen amount itself: its own amount stepper is used, and MultiCraft is kept from resetting that amount.");
        }
        else
        {
            _harmony.PatchAll(typeof(BatchCraft));
            _harmony.PatchAll(typeof(AmountControl));
            _harmony.PatchAll(typeof(PadAmount));
            _harmony.PatchAll(typeof(HintBarCraftHint));
        }
        Logger.LogInfo($"{Name} {PluginVersion.Value} loaded");
    }

    private void OnDestroy() => _harmony.UnpatchSelf();
}
