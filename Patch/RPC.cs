namespace LastTombstonePin.Patch;

[HarmonyPatch]
public class RPC
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start)), HarmonyPostfix, HarmonyWrapSafe]
    private static void Patch()
    {
        ZRoutedRpc.instance.Register<string>("UpdateTombstonePins_Server", UpdateTombstonePins_Server);
        ZRoutedRpc.instance.Register<string>("UpdateTombstonePins_Client", UpdateTombstonePins_Client);
        ZRoutedRpc.instance.Register<string>("TeleportPlayerToLastTombstone_Server", Teleport_Server);
        ZRoutedRpc.instance.Register<string, Vector3>("TeleportPlayerToLastTombstone_Client", Teleport_Client);
        ZRoutedRpc.instance.Register("PrintTombstones", async (_) =>
        {
            var zdos = await FindTomds();
            Debug(
                $"All tombstones: {zdos.Count}, {zdos.Select(x => x.GetString(ZDOVars.s_ownerName).Replace(" ", "")).GetString()}");
        });
    }


    private static void Teleport_Server(long _, string playerName)
    {
        var importantZdOs = ZDOMan.instance.GetImportantZDOs(Hash);
        var tombstone =
            importantZdOs.FirstOrDefault(x => x.GetString(ZDOVars.s_ownerName).Replace(" ", "") == playerName);

        //Target player has no tombstone
        if (tombstone == null) return;
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "TeleportPlayerToLastTombstone_Client",
            playerName, tombstone.GetPosition());
    }

    private static void Teleport_Client(long _, string playerName, Vector3 tombstonePos)
    {
        if (!m_localPlayer || m_localPlayer.GetPlayerName().Replace(" ", "") != playerName) return;
        m_localPlayer.TeleportTo(tombstonePos, Quaternion.identity, distantTeleport.Value);
    }
}