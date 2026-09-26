using System.Reflection;
using HarmonyLib;
using Thor;
using static Thor.GameData;
using UnityEngine;

namespace MoreSaves.Helpers
{
    internal class SaveManager
    {
        public static int currentPage = 1;
        public static float cooldownTimer = 0;
        private static float originalSlotDelay = 0;
        private static int indexOffset => (currentPage - 1) * 3;
        private static int maxPageCount => MoreSavesBase.MaxPageCount.Value;

        [HarmonyPatch(typeof(GameData), nameof(GameData.Setup))]
        [HarmonyPrefix]
        static void SetupPrefix(ref SaveData[] ___mSaveDatas)
        {
            MoreSavesBase.Instance.Config.Reload();
            ___mSaveDatas = new SaveData[maxPageCount*3];
            Debug.Log($"{MoreSavesBase.modGUID}: Successfully initiated {maxPageCount * 3} save files.");
        }

        [HarmonyPatch(typeof(SaveSlotListItem), nameof(SaveSlotListItem.Initialize))]
        [HarmonyPrefix]
        static void InitPrefix(ref float ___m_delay, ref int slotIndex, ref SaveSlotListItem __instance)
        {
            slotIndex += indexOffset;
            originalSlotDelay = ___m_delay;
            ___m_delay = 0;

            if (HudManager.active) return;
            if (slotIndex - indexOffset != 1) return;
            HudManager.Awake();
        }

        [HarmonyPatch(typeof(SaveSlotListItem), nameof(SaveSlotListItem.Initialize))]
        [HarmonyPostfix]
        static void InitPostfix(ref float ___m_delay)
        {
            ___m_delay = originalSlotDelay;
        }

        [HarmonyPatch(typeof(SaveSlotListItem), nameof(SaveSlotListItem.Shutdown))]
        [HarmonyPrefix]
        static void ShutdownPrefix(SaveSlotListItem __instance)
        {
            int slotIndex = (int)AccessTools.Property(typeof(SaveSlotListItem), "SlotIndex").GetValue(__instance);
            if (slotIndex - indexOffset != 1) return;
            HudManager.Shutdown();
        }

        static void OnSpawnsAvatar(PlayerEvent playerEvent)
        {
            HudManager.Shutdown();
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

        private static void RefreshSlots()
        {
            SaveSlotListItem[] saveSlotListItems = Object.FindObjectsOfType<SaveSlotListItem>();

            var slotIndex = saveSlotListItems.Length  - 1;
            foreach (var slotItem in saveSlotListItems)
            {
                UpdateSlot(slotItem, slotIndex);
                slotIndex--;
            }
            cooldownTimer = originalSlotDelay * 3;
            HudManager.Refresh();
        }

        private static void UpdateSlot(SaveSlotListItem item, int slotIndex)
        {
            if (!item) return;
            item.Initialize(slotIndex);

            FieldInfo tokensField = AccessTools.Field(typeof(SaveSlotListItem), "m_tokens");
            DataObjectCollection tokens = (DataObjectCollection)tokensField?.GetValue(item);
            if (tokens == null) return;

            FieldInfo tokenLayoutField = AccessTools.Field(typeof(SaveSlotListItem), "m_tokenLayout");
            RectTransform tokenLayout = (RectTransform)tokenLayoutField?.GetValue(item);
            if (tokenLayout == null) return;

            int tokenIndex = 0;
            foreach (Transform child in tokenLayout)
            {
                if (tokenIndex >= tokens.Count) break;

                TokenListItem tokenListItem = child.GetComponent<TokenListItem>();
                if (tokenListItem)
                {
                    tokenListItem.Initialize(tokens[tokenIndex], item.SaveData.IsDiscovered(tokens[tokenIndex]));
                }
                tokenIndex++;
            }
        }

        public static void ChangePage(int variation)
        {
            MoreSavesBase.Instance.Config.Reload();
            if (!HudManager.active) return;
            if (cooldownTimer != 0) return;

            int previousPage = currentPage;
            currentPage += variation;
            currentPage = Mathf.Clamp(currentPage,1,MoreSavesBase.MaxPageCount.Value);
            if (previousPage == currentPage) return;

            RefreshSlots();
        }
    }
}
