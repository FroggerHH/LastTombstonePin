using BepInEx;
using BepInEx.Configuration;
using static Minimap;
using static Minimap.PinType;

namespace LastTombstonePin;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public static readonly int Hash = "Player_tombstone".GetStableHashCode();

    public const string ModName = "LastTombstonePin",
        ModVersion = "1.4.0",
        ModGUID = $"com.{ModAuthor}.{ModName}",
        ModAuthor = "JustAFrogger";

    private static Sprite mapPingSprite;
    private static ConfigEntry<bool> doubleSize;
    private static ConfigEntry<bool> animate; 
    public static ConfigEntry<bool> distantTeleport;
    private static ConfigEntry<int> pinTypeID;
    private static ConfigEntry<bool> showYourNameOnTombstonePin;


    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        EnableImportantZDOs();
        RegisterImportantZDO(new(Hash, true, UpdateTombstonePins, UpdateTombstonePins));
        mapPingSprite = LoadAssetBundle(ModName.ToLower()).LoadAsset<Sprite>("LastTombstonePin_Sprite");
        bundle?.Unload(false);

        doubleSize = config("General", "Double size ping", true, "Should the ping be doubled in size on minimap?");
        animate = config("General", "Animate ping", true,
            "Should the ping smoothly grow in size and shrink in size on minimap?");
        distantTeleport = config("General", "Distant teleport", false, "");
        pinTypeID = config("General", "Pin type id", 20, "Id will be given to the pin");
        showYourNameOnTombstonePin = config("General", "Show your name on tombstone pin", false, "");
    }

    // ReSharper disable once PossibleNullReferenceException
    internal static void UpdateTombstonePins()
    {
        var minimap = Minimap.instance;
        if (!minimap) return;
        var pingsToDelete = minimap.m_pins.FindAll(x => x.m_icon == mapPingSprite || x.m_type == Death);
        if (pingsToDelete.Count > 0) foreach (var pinData in pingsToDelete) minimap.RemovePin(pinData);

        var zdos = ZDOMan.instance.GetImportantZDOs(Hash)?
            .Where(x => x is not null && x.IsValid())
            .Where(x => x.GetLong(ZDOVars.s_owner) == m_localPlayer?.GetPlayerID())
            .OrderBy(x => x.GetLong(ImportantZDOs.ZDO_Created_Hash)).ToList();
        if (zdos.Count == 0) return;
        var targetZDO = zdos.LastOrDefault();
        zdos.Remove(targetZDO);
        CreatePin(targetZDO);

        foreach (var zdo in zdos)
            minimap.AddPin(zdo.GetPosition(), Death,
                $"$hud_mapday {EnvMan.instance.GetDay(ZNet.instance.GetTimeSeconds())}", false, false);
    }

    private static void CreatePin(ZDO zdo)
    {
        var minimap = Minimap.instance;
        if (!minimap) return;
        var author = zdo.GetString(ZDOVars.s_ownerName);
        var pin = new PinData
        {
            m_doubleSize = doubleSize.Value,
            m_animate = animate.Value,
            m_icon = mapPingSprite,
            m_name = showYourNameOnTombstonePin.Value ? author : string.Empty,
            m_save = false,
            m_checked = false,
            m_pos = zdo.GetPosition(),
            m_type = (PinType)pinTypeID.Value
        };
        pin.m_NamePinData = new PinNameData(pin);
        pin.m_ownerID = 0L;
        pin.m_author = author;
        minimap.m_pins.Add(pin);
    }
}