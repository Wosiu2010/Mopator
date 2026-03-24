#if BEPINEX
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using WaterGunLib.Modules;
[BepInPlugin(GUID,NAME,VER)]
public class Plugin : BaseUnityPlugin
{
    #pragma warning disable CS8618
    const string GUID = "WaterGun.Mopator";
    const string NAME = "MopatorEnemy";
    const string VER = "1.5.2";

    public static Plugin Instance;

    static AssetBundle bundle;

    static EnemyType MopatorEnemy;

    public static Dictionary<string, int> spawnValues;

    static ConfigEntry<bool> MopatorPlushEnable;
    static ConfigEntry<int> MopatorPlushRarity;

    static ConfigEntry<float> Powerlevel;
    static ConfigEntry<int> SpawnCap;
    static ConfigEntry<int> Rarity;

    void Awake()
    {
        Instance = this;
        
        MopatorPlushEnable = Config.Bind("MopatorPlushie", "Enable", true, new ConfigDescription("Should the plushie be enabled?"));
        MopatorPlushRarity = Config.Bind("MopatorPlushie", "Rarity", 20, new ConfigDescription("Rarity of the plushie to be spawned on the moon"));

        Rarity = Config.Bind("Spawn", "Rarity", 70, new ConfigDescription("What is the rarity for mopator?"));
        SpawnCap = Config.Bind("Spawn", "SpawnCap", 2, new ConfigDescription("What is the maximum amount of mopators that can spawn?"));
        Powerlevel = Config.Bind("Spawn", "PowerLevel", 1f, new ConfigDescription("What is the mopator PowerLevel?"));

        Harmony harmony = new Harmony(GUID);
        harmony.PatchAll(typeof(Plugin));

        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }

        
        string assetdir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mopator");
        bundle = AssetBundle.LoadFromFile(assetdir);

        TerminalNode mopatornode = ScriptableObject.CreateInstance<TerminalNode>();
        mopatornode.creatureName = "Mopator";
        mopatornode.displayText = "Mopator\r\n\r\nScientific name: 57-MP\r\n\r\nThere are various hypotheses about the origin of this curious creature, ranging from fictional ones that say it was created by aliens to take over human brains to more plausible ones that it is simply a mutated mop. \r\nThe most likely theory is that it was created during an accident at the secret facility 57-Harbringer, but it was there that it was first observed by expeditions.\r\n\r\nMopator is not aggressive, driven by curiosity follows people, but what is fascinating is that it absorbs dirt from the floor, even though it is a food source that should not be nutritious for it at all.\r\nAs it moves, it leaves behind toxic, deadly feces.\r\nThe most likely and logical explanation for the formation of such large puddles is that Mopator collects moisture from the environment, mixes it with his feces, and defecates frequently, but according to research, only 1.57% of his feces are found in a single puddle, which mainly serves to deter hostile creatures.\n\n";
        mopatornode.clearPreviousText = true;
        
        MopatorEnemy = bundle.LoadAsset<EnemyType>("Assets/LethalCompany/Mods/Enemies/Mopator/MopatorEnemy.asset");
        MopatorEnemy.PowerLevel = Powerlevel.Value;
        MopatorEnemy.MaxCount = SpawnCap.Value;
        //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MopatorEnemy.enemyPrefab.GetComponent<MopatorAI>().Blob);
        //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MopatorEnemy.enemyPrefab);
        //LethalLib.Modules.Utilities.FixMixerGroups(MopatorEnemy.enemyPrefab.GetComponent<MopatorAI>().Blob);
        //LethalLib.Modules.Utilities.FixMixerGroups(MopatorEnemy.enemyPrefab);
        //LethalLib.Modules.Enemies.RegisterEnemy(MopatorEnemy, Rarity.Value, Levels.LevelTypes.All, mopatornode);
        EnemyTypeRef mopatorEnemyRef = ScriptableObject.CreateInstance<EnemyTypeRef>();

        mopatorEnemyRef.rarity = 70;
        mopatorEnemyRef.enemyType = MopatorEnemy;
        mopatorEnemyRef.infoNode = mopatornode;
        mopatorEnemyRef.spawnType = WaterGunLib.Modules.Enemies.SpawnType.Indoor;
        mopatorEnemyRef.PlanetNames = new List<string>{"All"};
        mopatorEnemyRef.networkPrefab = false;
        WaterGunLib.Modules.Prefabs.NetworkPrefabs.RegisterNetworkPrefab(mopatorEnemyRef.enemyType.enemyPrefab.GetComponent<MopatorAI>().Blob);
        WaterGunLib.Modules.Prefabs.NetworkPrefabs.RegisterNetworkPrefab(mopatorEnemyRef.enemyType.enemyPrefab);
        
        WaterGunLib.Modules.Enemies.RegisterEnemy(mopatorEnemyRef, GUID);

        if (MopatorPlushEnable.Value)
        {
            Item MopatorPlush = bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/Enemies/Mopator/MopatorPlushieItem.asset");
            ItemRef mopatorPlushRef = ScriptableObject.CreateInstance<ItemRef>();
            mopatorPlushRef.item = MopatorPlush;
            mopatorPlushRef.rarity = MopatorPlushRarity.Value;
            mopatorPlushRef.networkPrefab = false;
            mopatorEnemyRef.spawnType = WaterGunLib.Modules.Enemies.SpawnType.Indoor;
            mopatorEnemyRef.PlanetNames = new List<string> { "All" };

            WaterGunLib.Modules.Prefabs.NetworkPrefabs.RegisterNetworkPrefab(mopatorPlushRef.item.spawnPrefab);
            
            WaterGunLib.Modules.Items.RegisterItem(mopatorPlushRef, GUID);
            Debug.Log(mopatorPlushRef.PlanetNames);
            //LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MopatorPlush.spawnPrefab);
            //LethalLib.Modules.Utilities.FixMixerGroups(MopatorPlush.spawnPrefab);
            //LethalLib.Modules.Items.RegisterScrap(MopatorPlush, MopatorPlushRarity.Value, LethalLib.Modules.Levels.LevelTypes.All);
        }

        Logger.LogInfo($"Loaded Mopator v{VER}");

    }
}
#endif