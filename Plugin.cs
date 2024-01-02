using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using fastJSON;
using static Minimap;
using static Minimap.PinType;

namespace LastTombstonePin;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string ModName = "LastTombstonePin",
        ModVersion = "1.6.1",
        ModGUID = $"com.{ModAuthor}.{ModName}",
        ModAuthor = "JustAFrogger";

    public static readonly int Hash = "Player_tombstone".GetStableHashCode();

    private static Sprite mapPingSprite;
    private static ConfigEntry<bool> doubleSize;
    private static ConfigEntry<bool> animate;
    public static ConfigEntry<bool> distantTeleport;
    private static ConfigEntry<int> pinTypeID;
    private static ConfigEntry<bool> showYourNameOnTombstonePin;


    private void Awake()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        JSON.Parameters = new JSONParameters
        {
            UseExtensions = false,
            SerializeNullValues = false,
            DateTimeMilliseconds = false,
            UseUTCDateTime = true,
            UseOptimizedDatasetSchema = true,
            UseValuesOfEnums = true
        };
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        EnableImportantZDOs();
        RegisterImportantZDO(new ImportantZDO_Settings(Hash, true, UpdateTombstonePins, UpdateTombstonePins));
        mapPingSprite = LoadAssetBundle(ModName.ToLower()).LoadAsset<Sprite>("LastTombstonePin_Sprite");
        bundle?.Unload(false);

        doubleSize = config("General", "Double size ping", true, "Should the ping be doubled in size on minimap?");
        animate = config("General", "Animate ping", true,
            "Should the ping smoothly grow in size and shrink in size on minimap?");
        pinTypeID = config("General", "Pin type id", 20, "Id will be given to the pin");
        showYourNameOnTombstonePin = config("General", "Show your name on tombstone pin", false, "");
        distantTeleport = config("General", "Distant teleport", false, "");
    }

    // ReSharper disable once PossibleNullReferenceException
    internal static void UpdateTombstonePins()
    {
        if (!m_localPlayer) return;
        var plName = m_localPlayer.GetPlayerName().Replace(" ", "");
        ZRoutedRpc.instance?.InvokeRoutedRPC("UpdateTombstonePins_Server", plName);
    }

    internal static async void UpdateTombstonePins_Server(long _, string requestedByPlayerName)
    {
        try
        {
            if (!ZNet.instance) return;
            List<RPC_SendData.PinSendData> deathPins = new();
            RPC_SendData.PinSendData tombstone = null;
            var zdos = await FindTomds(requestedByPlayerName);
            if (zdos.Count == 0)
                DebugWarning($"UpdateTombstonePins_Server: no tombstones of player {requestedByPlayerName} found",
                    false);
            // return;
            var targetZDO = zdos.LastOrDefault();
            if (targetZDO != null)
            {
                // if (zdos.Where(x => x.GetBool("IsLastTombstone", true)).Count() == 0)
                // {
                // }

                // if (targetZDO.GetBool("IsLastTombstone", true))
                // {
                // targetZDO.Set("IsLastTombstone", true);
                zdos.Remove(targetZDO);
                tombstone = new RPC_SendData.PinSendData(targetZDO.GetPosition(),
                    targetZDO.GetString(ZDOVars.s_ownerName));
                // }
            }

            foreach (var zdo in zdos)
                deathPins.Add(new RPC_SendData.PinSendData(zdo.GetPosition(),
                    $"$hud_mapday {EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds())}"));
            // zdo.Set("IsLastTombstone", false);
            var rpc_data = new RPC_SendData(deathPins, tombstone, requestedByPlayerName);
            var rpc_data_str = JSON.ToJSON(rpc_data);
            DebugWarning($"UpdateTombstonePins_Server 1, rpc_data_str: {rpc_data_str}, rpc_data:{rpc_data}", false);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "UpdateTombstonePins_Client", rpc_data_str);
        }
        catch (Exception e)
        {
            DebugError($"UpdateTombstonePins_Server error: {e}");
        }
    }

    public static async Task<List<ZDO>> FindTomds(string requestedByPlayerName)
    {
        var zdos = await ZoneSystem.instance.GetWorldObjectsAsync(
            searchZdo => searchZdo is not null
                         && searchZdo.GetPrefab() == Hash
                         && searchZdo.IsValid()
                         && searchZdo.GetString(ZDOVars.s_ownerName).Replace(" ", "") == requestedByPlayerName);
        zdos = zdos.OrderBy(x => x.GetLong(ImportantZDOs.ZDO_Created_Hash)).ToList();
        return zdos;
    }

    public static async Task<List<ZDO>> FindTomds()
    {
        var zdos = await ZoneSystem.instance.GetWorldObjectsAsync(
            searchZdo => searchZdo is not null
                         && searchZdo.GetPrefab() == Hash
                         && searchZdo.IsValid());
        zdos = zdos.OrderBy(x => x.GetLong(ImportantZDOs.ZDO_Created_Hash)).ToList();
        return zdos;
    }

    internal static void UpdateTombstonePins_Client(long _, string rpc_data_str)
    {
        try
        {
            if (!m_localPlayer) return;
            RPC_SendData rpc_data = null;
            try
            {
                rpc_data = JSON.ToObject<RPC_SendData>(rpc_data_str);
            }
            catch (Exception e)
            {
                DebugError($"rpc_data deserialization failed: {rpc_data_str}, error: {e.Message}");
            }

            Debug($"UpdateTombstonePins_Client 3, rpc_data {rpc_data?.ToString() ?? "null"}");
            if (rpc_data == null) return;
            var deathPins = rpc_data.deathPins;
            var tombstone = rpc_data.tombstone;
            if (m_localPlayer.GetPlayerName().Replace(" ", "") != rpc_data.playerName.Replace(" ", "")) return;
            var minimap = Minimap.instance;
            if (!minimap) return;
            var pingsToDelete = minimap.m_pins.FindAll(x => x.m_icon == mapPingSprite || x.m_type == Death);
            if (pingsToDelete.Count > 0)
                foreach (var pinData in pingsToDelete)
                    minimap.RemovePin(pinData);
            foreach (var data in deathPins) minimap.AddPin(data.pos.ToVector3(), Death, data.str, false, false);
            if (tombstone is not null)
                CreatePin(tombstone.pos.ToVector3(), tombstone.str);
        }
        catch (Exception e)
        {
            DebugError($"UpdateTombstonePins_Client error: {e}");
        }
    }

    private static void CreatePin(Vector3 pos, string ownerName)
    {
        var minimap = Minimap.instance;
        if (!minimap) return;
        var author = ownerName;
        var pin = new PinData
        {
            m_doubleSize = doubleSize.Value,
            m_animate = animate.Value,
            m_icon = mapPingSprite,
            m_name = showYourNameOnTombstonePin.Value ? author : string.Empty,
            m_save = false,
            m_checked = false,
            m_pos = pos,
            m_type = (PinType)pinTypeID.Value
        };
        pin.m_NamePinData = new PinNameData(pin);
        pin.m_ownerID = 0L;
        pin.m_author = author;
        minimap.m_pins.Add(pin);
    }

    [Serializable]
    public class RPC_SendData
    {
        public string playerName;
        public PinSendData[] deathPins = new PinSendData[0];
        public PinSendData tombstone;

        public RPC_SendData(IEnumerable<PinSendData> deathPins, PinSendData tombstone, string playerName)
        {
            this.deathPins = deathPins.ToArray();
            this.tombstone = tombstone;
            this.playerName = playerName;
        }

        public RPC_SendData() { }

        public override string ToString()
        {
            return $"\nDeathPins: '{deathPins.GetString()}'\nTombstone: '{tombstone?.ToString() ?? "null"}'";
        }

        [Serializable]
        public class PinSendData
        {
            [SerializeField] public string str = "";
            [SerializeField] public SimpleVector3 pos;

            public PinSendData(Vector3 pos, string str)
            {
                this.pos = pos.ToSimpleVector3();
                this.str = str;
            }

            public PinSendData() { }

            public override string ToString() { return $"Pos: {pos}, Str: {str}"; }
        }
    }
}