using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using MoreSaves;
using System.Linq;
using Thor;
using Rewired;
using System.Reflection;
using System.Collections;
using TMPro;
using MoreSaves.HUD;

namespace MoreSaves
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MoreSavesBase : BaseUnityPlugin
    {
        public const string modGUID = "SpiralMods." + modName;
        public const string modName = "MoreSaves";
        private const string modVersion = "1.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static MoreSavesBase Instance;

        public static ManualLogSource mls;

        public static int saveOffset = 0;

        public static float delay = 0, cooldownTimer = 0;

        public static bool in_menu = false, created_hud = false, textures_failed = false;

        public static int current_page
        {
            get
            {
                return saveOffset / 3 + 1;
            }
        }

        public static BepInEx.Configuration.ConfigEntry<int> MaxFileCount;

        public static Font backupFont;

        void Awake()
        {
            string[] availableFonts = Font.GetOSInstalledFontNames();
            if (availableFonts.Contains("Arial"))
            {
                backupFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
                Logger.LogInfo("Loaded Arial font.");
            }
            else
            {
                backupFont = Font.CreateDynamicFontFromOSFont("Liberation Sans", 16);
                Logger.LogInfo("Loaded Liberation Sans font.");
            }

            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo($"{modName} has loaded (ModVersion: {modVersion}, ModGUID: {modGUID})!");

            harmony.PatchAll(typeof(SaveManager));
            harmony.PatchAll(typeof(HUDControl));
            ConfigCreate();
        }

        void ConfigCreate()
        {
            MoreSavesBase.MaxFileCount = this.Config.Bind<int>("Settings", "Max Save File Count", 297, "Only accepts numbers divisible by 3");
            ConfigFix();
        }

        void ConfigFix()
        {
            int max_file = MoreSavesBase.MaxFileCount.Value;
            if (max_file % 3 != 0)
            {
                int fixedValue = (int)(Mathf.Round((float)max_file / 3) * 3);
                MoreSavesBase.MaxFileCount.Value = fixedValue;
                Debug.Log($"Adjusted Max Save File Count from {max_file} to {fixedValue}");
            }
            Config.Reload();
        }

        void Update()
        {
            HUDControl.Update();
            cooldownTimer -= Time.deltaTime;
            cooldownTimer = Mathf.Clamp(cooldownTimer, 0, 256);
        }

        public static void Next(bool subtract)
        {
            if (cooldownTimer == 0 && in_menu)
            {
                int original = saveOffset;
                if (subtract) { saveOffset -= 3; } else { saveOffset += 3; }
                saveOffset = Mathf.Clamp(saveOffset, 0, MoreSavesBase.MaxFileCount.Value - 3);
                if (original != saveOffset)
                {
                    ReloadSlots();
                }
            }
        }

        public static void ReloadSlots()
        {
            SaveSlotListItem[] slots = GameObject.FindObjectsOfType<SaveSlotListItem>();

            int i = 2;
            foreach (var slot in slots)
            {
                MoreSavesBase.RestartSlot(slot, i);
                i--;
            }
            cooldownTimer = delay * 3;
        }

        public static void RestartSlot(SaveSlotListItem item, int slot)
        {
            if (item == null) return;

            MethodInfo initMethod = AccessTools.Method(typeof(SaveSlotListItem), "Initialize");

            initMethod?.Invoke(item, new object[] { slot });
        }
    }
}