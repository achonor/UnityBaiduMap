using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Achonor.LBSMap {

    public static class MapFunction {

        //public const string MapUrlSrc = "http://online2.map.bdimg.com/onlinelabel/?qt=tile&x={0}&y={1}&z={2}";
        public const string MapUrlSrc = "http://online5.map.bdimg.com/tile/?qt=vtile&x={0}&y={1}&z={2}&styles=pl&udt=20210114";


        /// <summary>
        /// 定义10级Tile比例为1
        /// 18级地图 瓦片图上1个像素等于1个MC单位（详见LngLat2Tile） 1个瓦片图是256x256
        /// 256个MC单位等于1个Unit单位（详见LngLat2Position），18级地图1张瓦片图是1个Unit单位
        /// 即18级地图的瓦片图上1像素是1/256个Unit单位  10级地图上1像素是1个Unit单位
        /// </summary>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static float GetTileScale(int zoom) {
            return Mathf.Pow(2, 10 - zoom);
        }

        /// <summary>
        /// 获取相机高度，定义10级地图相机高度为10
        /// </summary>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static float GetCameraHeight(float zoom) {
            return 300 * Mathf.Pow(2, 10 - zoom);
        }

        /// <summary>
        /// 获取相机移动距离相对屏幕的比例
        /// </summary>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public static float GetCameraMoveScale(float zoom) {
            return Mathf.Pow(2, 10 - zoom);
        }


        /// <summary>
        /// 将LngLat地理坐标系转换为tile瓦片坐标系
        /// </summary>
        /// <param name="lngLat">经纬度信息</param>
        /// <param name="zoom">缩放比例</param>
        /// <param name="pixelOffset">像素偏移</param>
        /// <returns></returns>
        public static TileData LngLat2Tile(Vector2D lngLat, int zoom, Vector2 pixelOffset) {
            TileData tileData = new TileData(zoom);
            //转换MC坐标
            Vector2D vector2D = MCTransform.ConvertLL2MC(lngLat);
            //转换到缩放级别
            vector2D *= Math.Pow(2, zoom - 18);
            //偏移像素
            vector2D += pixelOffset;
            tileData.tile.x = (int)(vector2D.x / 256.0);
            tileData.tile.y = (int)(vector2D.y / 256.0);
            tileData.pixel.x = ((int)vector2D.x % 256);
            tileData.pixel.y = ((int)vector2D.y % 256);
            return tileData;
        }

        /// <summary>
        /// 将LngLat地理坐标系转换为tile瓦片坐标系
        /// </summary>
        /// <param name="lngLat">经纬度信息</param>
        /// <param name="zoom">缩放比例</param>
        public static TileData LngLat2Tile(Vector2D lngLat, int zoom) {
            return LngLat2Tile(lngLat, zoom, Vector2.zero);
        }


        /// <summary>
        /// 将tile(瓦片)坐标系转换为LngLat(地理)坐标系
        /// 18级地图 瓦片图上1个像素等于1个MC单位
        /// </summary>
        /// <param name="tile">瓦片坐标</param>
        /// <param name="pixel">瓦片上的像素坐标</param>
        /// <param name="zoom">缩放比例</param>
        /// <returns></returns>
        public static Vector2D Tile2LngLat(TileData tileData) {
            Vector2D vector2D = new Vector2D();
            vector2D.x = tileData.tile.x * 256.0 + tileData.pixel.x;
            vector2D.y = tileData.tile.y * 256.0 + tileData.pixel.y;
            vector2D /= Math.Pow(2, tileData.zoom - 18);
            return MCTransform.ConvertMC2LL(vector2D);
        }

        /// <summary>
        /// Tile转像素
        /// </summary>
        /// <param name="tileData"></param>
        /// <returns></returns>
        public static Vector2D Tile2Pixel(this TileData tileData) {
            Vector2D vector2D = new Vector2D();
            vector2D.x = tileData.tile.x * 256.0 + tileData.pixel.x;
            vector2D.y = tileData.tile.y * 256.0 + tileData.pixel.y;
            return vector2D;
        }

        /// <summary>
        /// 经纬度转坐标（x，y）
        /// 256个MC单位对应1个Unit单位
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public static Vector3 LngLat2Position(Vector2D lngLat) {
            //转换MC坐标
            Vector2D vector2D = MCTransform.ConvertLL2MC(lngLat);
            //缩小到Unit单位范围内（-100000, 100000） 
            return new Vector2((float)(vector2D.x / 256), (float)(vector2D.y / 256));
        }

        /// <summary>
        /// 坐标转经纬度
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static Vector2D Position2LngLat(Vector3 worldPos) {
            Vector2D vector2D = new Vector2D(worldPos.x * 256, worldPos.y * 256);


            return MCTransform.ConvertMC2LL(vector2D);
        }

        /// <summary>
        /// 返回瓦片的下载URL
        /// </summary>
        /// <param name="lngLat">经纬度</param>
        /// <param name="zoom">缩放比例</param>
        /// <returns></returns>
        public static string GetTileURL(this Vector2D lngLat, int zoom) {
            TileData tileData = LngLat2Tile(lngLat, zoom);
            return GetTileURL(tileData);
        }

        /// <summary>
        /// 返回瓦片的下载URL
        /// </summary>
        /// <param name="tile">瓦片坐标</param>
        /// <param name="zoom">缩放比例</param>
        /// <returns></returns>
        public static string GetTileURL(this TileData tileData) {
            return string.Format(MapUrlSrc, tileData.tile.x, tileData.tile.y, tileData.zoom);
        }

        /// <summary>
        /// 腾讯地图坐标转百度地图
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public static Vector2D LngLatTx2Bd(Vector2D lngLat) {
            Vector2D result = new Vector2D();
            double z = Math.Sqrt(lngLat.x * lngLat.x + lngLat.y * lngLat.y) + 0.00002 * Math.Sin(lngLat.y * Math.PI);
            double theta = Math.Atan2(lngLat.y, lngLat.x) + 0.000003 * Math.Cos(lngLat.x * Math.PI);
            result.longitude = z * Math.Cos(theta) + 0.0065;
            result.latitude = z * Math.Sin(theta) + 0.006;
            return result;
        }

        /// <summary>
        /// 百度地图坐标转腾讯地图
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public static Vector2D LngLatBd2Tx(Vector2D lngLat) {
            Vector2D result = new Vector2D();
            double x = lngLat.x - 0.0065, y = lngLat.y - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Math.PI);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Math.PI);
            result.longitude = z * Math.Cos(theta);
            result.latitude = z * Math.Sin(theta);
            return result;
        }
    }
}