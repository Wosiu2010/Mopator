#if BEPINEX
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;
[BepInPlugin(GUID,NAME,VER)]
public class Plugin : BaseUnityPlugin
{
    const string GUID = "WaterGun.Mopator";
    const string NAME = "MopatorEnemy";
    const string VER = "1.0.0";

    static AssetBundle bundle;

    void Awake()
    {

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
        mopatornode.displayText = "Mopator\r\n\r\nScientific name: 57-MP\r\n\r\nThere are various hypotheses about the origin of this curious creature, ranging from fictional ones that say it was created by aliens to take over human brains to more plausible ones that it is simply a mutated mop. \r\nThe most likely theory is that it was created during an accident at the secret facility 57-Harbringer, but it was there that it was first observed by expeditions.\r\n\r\nMopator is not aggressive, driven by curiosity follows people, but what is fascinating is that it absorbs dirt from the floor, even though it is a food source that should not be nutritious for it at all.\r\nAs it moves, it leaves behind toxic, deadly feces.\r\nThe most likely and logical explanation for the formation of such large puddles is that Mopator collects moisture from the environment, mixes it with his feces, and defecates frequently, but according to research, only 1.67% of his feces are found in a single puddle, which mainly serves to deter hostile creatures.";
        mopatornode.clearPreviousText = true;
        
        EnemyType MopatorEnemy = bundle.LoadAsset<EnemyType>("Assets/LethalCompany/Mods/Mopator/MopatorEnemy.asset");
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MopatorEnemy.enemyPrefab.GetComponent<MopatorAI>().Blob);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MopatorEnemy.enemyPrefab);
        LethalLib.Modules.Utilities.FixMixerGroups(MopatorEnemy.enemyPrefab.GetComponent<MopatorAI>().Blob);
        LethalLib.Modules.Utilities.FixMixerGroups(MopatorEnemy.enemyPrefab);
        LethalLib.Modules.Enemies.RegisterEnemy(MopatorEnemy, 90, LethalLib.Modules.Levels.LevelTypes.All, LethalLib.Modules.Enemies.SpawnType.Default, mopatornode);

        Logger.LogInfo("Loaded Mopator");

    }
}
#endif