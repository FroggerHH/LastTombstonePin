using BepInEx;
using BepInEx.Configuration;
using ServerSync;

namespace LastTombstonePin;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public static readonly int hash = "Player_tombstone".GetStableHashCode();

    public const string ModName = "LastTombstonePin",
        ModVersion = "1.3.0",
        ModGUID = $"com.{ModAuthor}.{ModName}",
        ModAuthor = "JustAFrogger";

    public static Sprite mapPingSprite;
    public static ConfigEntry<bool> doubleSize;
    public static ConfigEntry<bool> animate;
    public static ConfigEntry<bool> distantTeleport;
    public static ConfigEntry<int> pinTypeID;
    public static ConfigEntry<bool> showYourNameOnTombstonePin;


    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        EnableImportantZDOs();
        RegisterImportantZDO(hash, trackCreationTime: true);
        mapPingSprite = LoadAssetBundle(ModName.ToLower()).LoadAsset<Sprite>("LastTombstonePin_Sprite");
        bundle?.Unload(false);

        doubleSize = config("General", "Double size ping", true, "Should the ping be doubled in size on minimap?");
        animate = config("General", "Animate ping", true,
            "Should the ping smoothly grow in size and shrink in size on minimap?");
        distantTeleport = config("General", "Distant teleport", false, "");
        pinTypeID = config("General", "Pin type id", 20, "Id will be given to the pin");
        showYourNameOnTombstonePin = config("General", "Show your name on tombstone pin", false, "");
    }
}