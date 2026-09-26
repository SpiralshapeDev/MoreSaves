using BepInEx;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Thor;

namespace MoreSaves.Helpers
{
    public class HudManager
    {
        public static bool active = false;
        private static GameObject button_up;
        private static GameObject button_down;
        private static GameObject page_display;
        private static readonly float pageDisplayFontSize = 48f;
        private static readonly Vector2 pageDisplayOffset = new Vector2(3.5f,0);
        private static TextMeshProUGUI button_up_text => button_up.transform.Find("TextObject").GetComponent<TextMeshProUGUI>();
        private static TextMeshProUGUI button_down_text => button_down.transform.Find("TextObject").GetComponent<TextMeshProUGUI>();
        private static TextMeshProUGUI page_display_text => page_display.transform.Find("TextObject").GetComponent<TextMeshProUGUI>();

        static GameObject page_element(bool is_button, string name, string texture, string text, float font_size, Vector2 position, Vector2 size, Vector2 textOffset)
        {
            HUD hud = Object.FindObjectOfType<HUD>();
            GameObject GO = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
            if (is_button) GO.AddComponent(typeof(Button));

            GO.name = $"{MoreSavesBase.modGUID}.{name.Replace(" ", "_")}";
            GO.transform.SetParent(hud.transform, false);

            RectTransform RT = GO.GetComponent<RectTransform>();
            RT.anchoredPosition = position;
            RT.sizeDelta = size;

            string runningDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Path.Combine(Paths.PluginPath, MoreSavesBase.modName);
            string texturePath = Path.Combine(runningDirectory, "assets", texture);
            Texture2D texture2D = TextureManager.LoadTexture(texturePath);
            if (texture2D != null) GO.GetComponent<UnityEngine.UI.Image>().sprite = TextureManager.TextureToSprite(texture2D);

            GameObject textGO = new GameObject("TextObject", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(GO.transform, false);

            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.5f);
            textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.anchoredPosition = textOffset;

            TextMeshProUGUI txt = textGO.GetComponent<TextMeshProUGUI>();
            txt.alignment = TextAlignmentOptions.Center;
            txt.text = text;
            txt.fontSize = font_size;
            txt.color = MoreSavesBase.textures_failed ? Color.black : Color.white;

            return GO;
        }

        public static void Awake()
        {
            Debug.Log($"{MoreSavesBase.modGUID}: Creating UI");
            Vector2 button_size = new Vector2(178, 46);

            button_up = page_element(true, "Page Up", "button.png", "Next Page", 28f, new Vector2(850, -255), button_size, Vector2.zero);
            button_up.GetComponent<Button>().onClick.AddListener(() =>
            {
                SaveManager.ChangePage(1);
            });

            button_down = page_element(true, "Page Down", "button.png", "Previous Page", 28f, new Vector2(850, -425), button_size, Vector2.zero);
            button_down.GetComponent<Button>().onClick.AddListener(() =>
            {
                SaveManager.ChangePage(-1);
            });

            page_display = page_element(false, "Page Display", "box.png", "", pageDisplayFontSize, new Vector2(850, -340), new Vector2(92, 92), pageDisplayOffset);

            Debug.Log($"{MoreSavesBase.modGUID}: Finished Creating UI");
            active = true;
            Refresh();
        }

        public static void Shutdown()
        {
            active = false;
            MoreSavesBase.textures_failed = false;
            Object.Destroy(button_down);
            Object.Destroy(button_up);
            Object.Destroy(page_display);
            (button_down, button_up, page_display) = (null, null, null);
            Debug.Log($"{MoreSavesBase.modGUID}: Deleted UI");
        }

        public static void Refresh()
        {
            if (!active) { return; }
            page_display_text.text = $"P{SaveManager.currentPage}";

            page_display_text.fontSize = pageDisplayFontSize - (page_display_text.text.Length - 2) * 10f;
            page_display_text.GetComponent<RectTransform>().anchoredPosition = pageDisplayOffset - (page_display_text.text.Length - 2) * new Vector2(2,0);
            if (MoreSavesBase.textures_failed) return;
            button_down_text.color = SaveManager.currentPage == 1 ? Color.grey : Color.white;
            button_up_text.color = SaveManager.currentPage == MoreSavesBase.MaxPageCount.Value ? Color.grey : Color.white;

            page_display_text.fontSize = pageDisplayFontSize - (page_display_text.text.Length - 2) * 10f;
        }
    }
}
