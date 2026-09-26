using System.IO;
using UnityEngine;

namespace MoreSaves.Helpers
{
    public static class TextureManager
    {
        public static Texture2D LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Failed to find texture `{path}`");
                MoreSavesBase.textures_failed = true;
                return null;
            }
            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            return tex;
        }

        public static Sprite TextureToSprite(Texture2D texture2D)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
