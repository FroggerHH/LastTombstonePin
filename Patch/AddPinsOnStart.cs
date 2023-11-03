namespace LastTombstonePin.Patch;

[HarmonyPatch]
public class AddPinsOnStart
{
    [HarmonyPatch(typeof(Player), nameof(Player.Start)), HarmonyPostfix, HarmonyWrapSafe]
    private static void Patch() => UpdateTombstonePins();
}