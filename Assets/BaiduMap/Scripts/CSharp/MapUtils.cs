using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapUtils
{
    /// <summary>
    /// Texture转Sprite
    /// </summary>
    /// <param name="texture2D"></param>
    /// <returns></returns>
    public static Sprite Texture2D2Sprite(Texture2D texture2D) {
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1f);
    }
}
