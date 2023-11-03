using System.Threading.Tasks;
using static Minimap;
using static Minimap.PinType;

namespace LastTombstonePin;

[HarmonyPatch]
public class RemovePin
{
    [HarmonyWrapSafe, HarmonyPostfix, HarmonyPatch(typeof(TombStone), nameof(TombStone.UpdateDespawn))]
    private static async void TombStoneDestroy_Postfix(TombStone __instance)
    {
        await Task.Yield();
        var valid = __instance.m_nview.IsValid();
        if (valid == true) return;
        Debug(
            $"TombStoneDestroy Postfix: zdos: {ZDOMan.instance.GetImportantZDOs(hash).Count}, " +
            $"pins: {Minimap.instance.m_pins.FindAll(x => x.m_icon == mapPingSprite).GetString()}");
        GetPlugin<Plugin>().Invoke(nameof(UpdateTombstonePins), 3);
    }

    [HarmonyWrapSafe, HarmonyPrefix, HarmonyPatch(typeof(TombStone), nameof(TombStone.UpdateDespawn))]
    private static void TombStoneAwake_Prefix(TombStone __instance)
    {
        Debug(
            $"TombStoneDestroy Prefix:  zdos: {ZDOMan.instance.GetImportantZDOs(hash).Count}, " +
            $"pins: {Minimap.instance.m_pins.FindAll(x => x.m_icon == mapPingSprite).GetString()}");
    }
}