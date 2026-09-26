using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MoreSaves.Helpers;

namespace MoreSaves
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MoreSavesBase : BaseUnityPlugin
    {
        public const string modGUID = "SpiralMods." + modName;
        public const string modName = "MoreSaves";
        private const string modVersion = "1.1.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static MoreSavesBase Instance;

        private static ManualLogSource mls;

        public static bool textures_failed = false;

        public static BepInEx.Configuration.ConfigEntry<int> MaxPageCount;

        void Awake()
        {
            Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo($"{modName} has loaded (ModVersion: {modVersion}, ModGUID: {modGUID})!");

            harmony.PatchAll(typeof(SaveManager));
            harmony.PatchAll(typeof(HudManager));
            ConfigCreate();
            Config.Reload();
        }

        void ConfigCreate()
        {
            MaxPageCount = Config.Bind<int>("Settings", "Max Page Count", 99, "");
        }

        void Update()
        {
            if (SaveManager.cooldownTimer == 0) return;

            SaveManager.cooldownTimer -= Time.deltaTime;
            SaveManager.cooldownTimer = Mathf.Max(0, SaveManager.cooldownTimer);
        }
    }
}