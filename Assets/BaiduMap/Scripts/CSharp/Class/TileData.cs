using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Achonor.LBSMap {
    [Serializable]
    public class TileData {
        /// <summary>
        /// 缩放级别
        /// </summary>
        public int zoom;
        /// <summary>
        /// 墨卡托Tile坐标
        /// </summary>
        public Vector2Int tile = new Vector2Int();
        /// <summary>
        /// 坐标对应在Tile上的像素点
        /// </summary>
        public Vector2 pixel = new Vector2();

        public string Key {
            get {
                return string.Format("{0}x{1}x{2}", zoom, tile.x, tile.y);
            }
        }

        public TileData(int _zoom, int tileX = 0, int tileY = 0, int pixelX = 0, int pixelY = 0) {
            zoom = _zoom;
            tile.x = tileX;
            tile.y = tileY;
            pixel.x = pixelX;
            pixel.y = pixelY;
        }

        /// <summary>
        /// 获取Tile在场景中图片的缩放比例
        /// </summary>
        /// <returns></returns>
        public float GetTileScale() {
            return MapFunction.GetTileScale(zoom);
        }

        public override string ToString() {
            return string.Format("({0}, {1}, {2})", zoom, tile.ToString(), pixel.ToString());
        }

        public double Distance(TileData tileData) {
            Vector2D vector1 = MapFunction.Tile2Pixel(this);
            Vector2D vector2 = MapFunction.Tile2Pixel(tileData);
            return vector1.Distance(vector2);
        }
    }
}