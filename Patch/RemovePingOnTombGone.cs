namespace LastTombstonePin;

[HarmonyPatch]
public class RemovePingOnTombGone
{
    [HarmonyPatch(typeof(TombStone), nameof(TombStone.UpdateDespawn)), HarmonyPostfix]
    private static void Postfix(TombStone __instance)
    {
        if (__instance?.m_nview?.GetZDO() != null) return;

        var minimap = Minimap.instance;
        minimap.m_pins.RemoveAll(x => x.m_icon == mapPingSprite);
    }
}