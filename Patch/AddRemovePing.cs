namespace LastTombstonePin;

[HarmonyPatch]
public class AddRemovePing
{
    [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone)), HarmonyPostfix]
    private static void Postfix(Player __instance)
    {
        var minimap = Minimap.instance;
        if (!__instance || !minimap || !__instance.IsPlayer() || __instance.GetHealth() > 0) return;
        if (__instance.m_inventory.NrOfItems() == 0) return;

        // minimap.m_pins.RemoveAll(x => x.m_icon == mapPingSprite);
        //
        // var pin = new Minimap.PinData();
        // pin.m_doubleSize = doubleSize.Value;
        // pin.m_animate = animate.Value;
        // pin.m_icon = mapPingSprite;
        // pin.m_name = "";
        // pin.m_save = save.Value;
        // pin.m_checked = false;
        // pin.m_pos = __instance.transform.position;
        // pin.m_type = Minimap.PinType.Death;
        // pin.m_NamePinData = new Minimap.PinNameData(pin);
        // pin.m_ownerID = __instance.GetPlayerID();
        // pin.m_author = __instance.GetPlayerName();
        // minimap.m_pins.Add(pin);
    }
}