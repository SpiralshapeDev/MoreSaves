using BepInEx;
using HarmonyLib;
using System.IO;
using Thor;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace MoreSaves.HUD
{
    public class Loader
    {
        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Failed to load texture, attempting backup");
                MoreSavesBase.textures_failed = true;
                return null;
            }
            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            return tex;
        }

        public static Sprite TextureToSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Font LoadFont(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Failed to find font file, using backup font");
                return MoreSavesBase.backupFont;
            }

            byte[] fontData = File.ReadAllBytes(path);
            Font customFont = new Font();
            customFont = new Font(path);

            if (customFont == null)
            {
                Debug.LogError("Failed to load font, using backup font");
                return MoreSavesBase.backupFont;
            }

            return customFont;
        }
    }
}
