// ReSharper disable VariableHidesOuterVariable

namespace LastTombstonePin;

[HarmonyPatch]
public static class TerminalCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))] [HarmonyPostfix]
    private static void AddCommands()
    {
        new Terminal.ConsoleCommand("TeleportPlayerToLastTombstone",
            "Teleports a player to his tombstone. First argument is player name to teleport.", args =>
            {
                RunCommand(args =>
                {
                    if (!IsAdmin) throw new ConsoleCommandException("You are not an admin on this server");
                    if (args.Length < 2)
                        throw new ConsoleCommandException(
                            "First argument must be a valid player name, who you want to teleport. Also he needs to be online right now.");
                    var playerName = args[1].Replace('_', ' ');
                    var player = s_players.Find(x => x.GetPlayerName() == playerName);
                    if (!player) throw new ConsoleCommandException($"Target player with name {playerName} not found");
                    var tombstone = ZDOMan.instance.GetImportantZDOs(Hash).FirstOrDefault(x =>
                        x.GetString(ZDOVars.s_ownerName) == playerName);
                    if (tombstone == null) throw new ConsoleCommandException("Target player has no tombstone");
                    player.TeleportTo(tombstone.GetPosition(), Quaternion.identity, distantTeleport.Value);

                    args.Context.AddString("Processing...");
                }, args);
            }, true, optionsFetcher: () => ZNet.instance.m_players.Select(x => x.m_name.Replace(' ', '_')).ToList());
    }
}