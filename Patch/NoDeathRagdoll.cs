using static EffectList;

namespace LastTombstonePin.Patch;

[HarmonyPatch]
public class NoDeathRagdoll
{
    [HarmonyPatch(typeof(Player), nameof(Player.CreateDeathEffects)), HarmonyPrefix, HarmonyWrapSafe]
    private static void Patch(Player __instance)
    {
        var newList = new List<EffectData>(__instance.m_deathEffects.m_effectPrefabs);
        newList.RemoveAll(x => x.m_prefab.GetComponentInChildren<Ragdoll>());
        __instance.m_deathEffects.m_effectPrefabs = newList.ToArray();
    }
}