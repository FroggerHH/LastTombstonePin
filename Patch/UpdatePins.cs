using static Minimap;

namespace LastTombstonePin;

[HarmonyPatch]
public class UpdatePins
{
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateDynamicPins)), HarmonyPrefix, HarmonyWrapSafe]
    private static void Patch(Minimap __instance)
    {
        var minimap = Minimap.instance;
        if (!__instance || !minimap || __instance != minimap) return;
        var pingsToDelete = minimap.m_pins.FindAll(x => x.m_icon == mapPingSprite);
        if (pingsToDelete != null && pingsToDelete.Count > 0)
            foreach (var pinData in pingsToDelete)
            {
                if (pinData == null) continue;
                if (pinData.m_uiElement?.gameObject != null) Destroy(pinData.m_uiElement.gameObject);
                if (pinData.m_NamePinData?.PinNameGameObject != null) Destroy(pinData.m_NamePinData.PinNameGameObject);
                minimap.m_pins.Remove(pinData);
            }

        var zdo = ZDOMan.instance.GetImportantZDOs(hash)?
            .Where(x => x?.GetLong(ZDOVars.s_owner) == m_localPlayer?.GetPlayerID())?
            .OrderBy(x => x?.GetLong(ImportantZDOs.ZDO_Created_Hash))?.LastOrDefault();
        if (zdo != null)
        {
            var author = zdo.GetString(ZDOVars.s_ownerName);
            var pin = new PinData();
            pin.m_doubleSize = doubleSize.Value;
            pin.m_animate = animate.Value;
            pin.m_icon = mapPingSprite;
            pin.m_name = showYourNameOnTombstonePin.Value ? author : string.Empty;
            pin.m_save = false;
            pin.m_checked = false;
            pin.m_pos = zdo.GetPosition();
            pin.m_type = (PinType)pinTypeID.Value;
            pin.m_NamePinData = new PinNameData(pin);
            pin.m_ownerID = 0L;
            pin.m_author = author;
            minimap.m_pins.Add(pin);
        }
    }
}