namespace LastTombstonePin;

[HarmonyPatch]
public static class TerminalCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))] [HarmonyPostfix]
    private static void AddCommands()
    {
        new Terminal.ConsoleCommand("TeleportPlayerToLastTombstone",
            "Teleports a player to his tombstone. First argument is player name to teleport.", args =>
                RunCommand(args =>
                {
                    ZRoutedRpc.instance.InvokeRoutedRPC("PrintTombstones");

                    // if (!IsAdmin) throw new ConsoleCommandException("You are not an admin on this server");
                    if (args.Length < 2)
                        throw new ConsoleCommandException(
                            "First argument must be a valid player name, who you want to teleport. Also he needs to be online right now.");
                    var playerName = args[1].Replace(" ", "");
                    var players = ZNet.instance.GetPlayerList();
                    var player = players.FirstOrDefault(x => x.m_name.Replace(" ", "") == playerName);
                    if (!player.m_name.IsGood())
                        throw new ConsoleCommandException($"Target player with name {playerName} not found");
                    ZRoutedRpc.instance.InvokeRoutedRPC("TeleportPlayerToLastTombstone_Server", playerName);
                    args.Context.AddString("Processing...");
                }, args), false,
            optionsFetcher: () =>
                ZNet.instance.GetPlayerList().Select(x => x.m_name.Replace(" ", "")).ToList());
        new Terminal.ConsoleCommand("UpdateTombstonePins", "", _ => UpdateTombstonePins());
        new Terminal.ConsoleCommand("PrintTombstones", "", _ => ZRoutedRpc.instance.InvokeRoutedRPC(
            ZRoutedRpc.Everybody, "PrintTombstones")); 
    }
}