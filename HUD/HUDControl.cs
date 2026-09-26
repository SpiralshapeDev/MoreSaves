using BepInEx;
using HarmonyLib;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using Rewired;
using static System.Net.Mime.MediaTypeNames;
using TMPro;
using Thor;

namespace MoreSaves.HUD
{
    public class HUDControl
    {
        public static GameObject button_up;
        public static GameObject button_down;
        public static GameObject page_display;
        public static TextMeshProUGUI button_up_text;
        public static TextMeshProUGUI button_down_text;
        public static TextMeshProUGUI page_display_text;
        public static Vector3 button_up_text_position;
        public static Vector3 button_down_text_position;
        public static Vector3 page_display_text_position;

        static GameObject page_element(bool is_button, string name, string texture, string text, float font_size, Vector2 position, Vector2 textOffset, Vector2 size)
        {
            Thor.HUD hud = GameObject.FindObjectOfType<Thor.HUD>();
            GameObject GO;
            if (is_button)
            {
                GO = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.Image), typeof(Button));
            } else
            {
                GO = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.Image));
            }
            name = name.Replace(" ", "");
            GO.name = $"{MoreSavesBase.modGUID}.{name}";
            GO.transform.SetParent(hud.transform, false);

            RectTransform RT = GO.GetComponent<RectTransform>();
            RT.anchoredPosition = position;
            RT.sizeDelta = size;

            string imgPath = Path.Combine(Paths.PluginPath, MoreSavesBase.modName, "assets", texture);
            Texture2D tex = Loader.LoadTexture(imgPath);

            if (tex != null)
            {
                UnityEngine.UI.Image img = GO.GetComponent<UnityEngine.UI.Image>();
                img.sprite = Loader.TextureToSprite(tex);
            }


            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(GO.transform, false);

            string font_path = Path.Combine(Paths.PluginPath, MoreSavesBase.modName, "assets", "font.ttf");

            TextMeshProUGUI txt = textGO.GetComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.font = TMP_FontAsset.CreateFontAsset(Loader.LoadFont(font_path));
            txt.fontSize = font_size;
            txt.fontSizeMax = font_size;
            txt.color = Color.white;
            txt.gameObject.transform.position += new Vector3(textOffset.x, textOffset.y, 0);

            if (name.Contains("Up"))
            {
                button_up_text = txt;
                button_up_text_position = txt.gameObject.transform.position;
            }
            else if (name.Contains("Down"))
            {
                button_down_text = txt;
                button_down_text_position = txt.gameObject.transform.position;
            }
            else if (name.Contains("Display"))
            {
                page_display_text = txt;
                page_display_text_position = txt.gameObject.transform.position;
            }

            return GO;
        }

        public static void Awake()
        {
            Button btn;

            Debug.Log($"{MoreSavesBase.modGUID}: Creating UI");
            Vector2 button_size = new Vector2(178, 46);

            button_up = page_element(true, "Page Button Up", "button.png", "Next Page", 28f, new Vector2(850, -255), new Vector2(-55, 12.5f), button_size);
            btn = button_up.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                bool subtract = false;
                MoreSavesBase.Next(subtract);
            });

            button_down = page_element(true, "Page Button Down", "button.png", "Previous Page", 28f, new Vector2(850, -425), new Vector2(-75f, 12.5f), button_size);
            btn = button_down.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                bool subtract = true;
                MoreSavesBase.Next(subtract);
            });

            page_display = page_element(false, "Page Display", "box.png", "", 48f, new Vector2(850, -340), new Vector2(-18.5f, 20f), new Vector2(92, 92));

            Debug.Log($"{MoreSavesBase.modGUID}: Done Creating UI");
        }

        public static void Update()
        {
            if (!MoreSavesBase.created_hud) { return; }

            if (MoreSavesBase.in_menu)
            {
                button_up.gameObject.SetActive(true);
                button_down.gameObject.SetActive(true);
                page_display.gameObject.SetActive(true);
                if (MoreSavesBase.textures_failed)
                {
                    button_up_text.color = Color.black;
                    button_down_text.color = Color.black;
                    page_display_text.color = Color.black;
                } else
                {
                    button_up_text.color = Color.white;
                    button_down_text.color = Color.white;
                    page_display_text.color = Color.white;
                }
            } else
            {
                button_up.gameObject.SetActive(false);
                button_down.gameObject.SetActive(false);
                page_display.gameObject.SetActive(false);
            }

            page_display_text.text = $"P{MoreSavesBase.current_page}";

            int numOffset = Mathf.Clamp(page_display_text.text.Length - 2, 0, 256);

            page_display_text.fontSize = page_display_text.fontSizeMax * (1 - (0.25f * numOffset));

            page_display_text.transform.position = page_display_text_position + new Vector3(-3.5f * numOffset, -5f * numOffset, 0);

            if (MoreSavesBase.saveOffset == 0)
            {
                button_down_text.color = Color.grey;
            }
            if (MoreSavesBase.saveOffset == MoreSavesBase.MaxFileCount.Value - 3)
            {
                button_up_text.color = Color.grey;
            }
        }
    }
}
