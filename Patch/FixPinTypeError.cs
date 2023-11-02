namespace LastTombstonePin;

[HarmonyPatch]
public class FixPinTypeError
{
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start)), HarmonyPostfix, HarmonyWrapSafe]
    private static void Patch(Minimap __instance)
    {
        var bools = new List<bool>(__instance.m_visibleIconTypes);
        bools.AddRange(Enumerable.Repeat(true, 100));
        __instance.m_visibleIconTypes = bools.ToArray();
    }
}