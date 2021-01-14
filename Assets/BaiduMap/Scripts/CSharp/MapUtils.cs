using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MapUtils
{
    /// <summary>
    /// Texture转Sprite
    /// 10级地图上1像素是1个Unit单位
    /// </summary>
    /// <param name="texture2D"></param>
    /// <returns></returns>
    public static Sprite Texture2D2Sprite(Texture2D texture2D) {
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1f);
    }

    /// <summary>
    ///  保存图片
    /// </summary>
    /// <param name="outPath"></param>
    /// <param name="texture2D"></param>
    public static void SaveTexture(string outPath, Texture2D texture2D) {
        byte[] bytes = texture2D.EncodeToPNG();
        string outDirectory = Directory.GetParent(outPath).FullName;
        if (!File.Exists(outDirectory)) {
            Directory.CreateDirectory(outDirectory);
        }
        File.WriteAllBytes(outPath, bytes);
    }
}
