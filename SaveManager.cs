using HarmonyLib;
using Thor;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using static Thor.GameData;
using static Thor.PlayerEvent;
using MoreSaves.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace MoreSaves
{
    internal class SaveManager
    {
        [HarmonyPatch(typeof(GameData), nameof(GameData.Setup))]
        [HarmonyPrefix]
        static void SetupPrefix(ref SaveData[] ___mSaveDatas)
        {
            int max_count = MoreSavesBase.MaxFileCount.Value;
            ___mSaveDatas = new SaveData[max_count];
            Debug.Log($"{MoreSavesBase.modGUID}: Succefully initiated {max_count} save files.");
        }

        [HarmonyPatch(typeof(SaveSlotListItem), nameof(SaveSlotListItem.Initialize))]
        [HarmonyPrefix]
        static void InitPrefix(ref float ___m_delay, ref int slotIndex)
        {
            slotIndex += MoreSavesBase.saveOffset;
            MoreSavesBase.delay = ___m_delay;
            ___m_delay = 0;
            MoreSavesBase.in_menu = true;
            
            if (MoreSavesBase.created_hud) return;
            
            MoreSavesBase.created_hud = true;
            HUDControl.Awake();
        }

        [HarmonyPatch(typeof(SaveSlotListItem), nameof(SaveSlotListItem.Initialize))]
        [HarmonyPostfix]
        static void InitPostfix(ref float ___m_delay)
        {
            ___m_delay = MoreSavesBase.delay;
        }

        [HarmonyPatch(typeof(SaveSlotListItem), nameof(SaveSlotListItem.Shutdown))]
        [HarmonyPrefix]
        static void ShutdownPrefix()
        {
            MoreSavesBase.in_menu = false;
        }

        static void OnSpawnsAvatar(PlayerEvent playerEvent)
        {
            MoreSavesBase.in_menu = false;
            MoreSavesBase.created_hud = false;
            UnityEngine.Object.Destroy(HUDControl.button_down);
            UnityEngine.Object.Destroy(HUDControl.button_up);
            UnityEngine.Object.Destroy(HUDControl.page_display);
            MoreSavesBase.textures_failed = false;
            Debug.Log($"{MoreSavesBase.modGUID}: Deleted UI");
        }

        [HarmonyPatch(typeof(Thor.HUD))]
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void Awake()
        {
            foreach (SimulationPlayer player in Game.Instance.Simulation.Players)
            {
                Debug.Log($"{MoreSavesBase.modGUID}: Creating UI Events");
                player.RegisterEvent(PlayerEvent.EventType.SpawnsAvatar, OnSpawnsAvatar);
                Debug.Log($"{MoreSavesBase.modGUID}: Done Creating UI Events");
            }
        }
    }
}
