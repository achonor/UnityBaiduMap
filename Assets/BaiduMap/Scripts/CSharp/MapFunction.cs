using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Shujun.LBSMap {

    public static class MapFunction {

        public const string MapUrlSrc = "http://online1.map.bdimg.com/onlinelabel/?qt=tile&x={0}&y={1}&z={2}";


        private static List<List<double>> MCRANGE = new List<List<double>>() {
            new List<double>(){ 0, 0, 0, 0},
            new List<double>(){ 0, 0, 0, 0},
            new List<double>(){ 0, 0, 0, 0},
            new List<double>(){ -656596217767.76416, -383649109051.37415, 656596217665.03418, 408751445578.28906 },
            new List<double>(){ -328298108883.88208, -191824554525.68707, 328298108832.51709, 204375722789.14453 },
            new List<double>(){ -164149054441.94104, -95912277262.843536, 164149054416.25854, 102187861394.57227 },
            new List<double>(){ -82074527220.97052, -47956138631.421768, 82074527208.129272, 51093930697.286133 },
            new List<double>(){ -41037263610.48526, -23978069315.710884, 41037263604.064636, 25546965348.643066 },
            new List<double>(){ -20518631805.24263, -11989034657.855442, 20518631802.032318, 12773482674.321533 },
            new List<double>(){ -10259315902.621315, -5994517328.927721, 10259315901.016159, 6386741337.1607666 },
            new List<double>(){ -5129657951.3106575, -2997258664.4638605, 5129657950.50808, 3193370668.5803833 },
            new List<double>(){ -2564828975.6553288, -1498629332.2319303, 2564828975.25404, 1596685334.2901917 },
            new List<double>(){ -1282414487.8276644, -749314666.11596513, 1282414487.62702, 798342667.14509583 },
            new List<double>(){ -641207243.91383219, -374657333.05798256, 641207243.81351, 399171333.57254791 },
            new List<double>(){ -320603621.95691609, -187328666.52899128, 320603621.906755, 199585666.78627396 },
            new List<double>(){ -160301810.97845805, -93664333.264495641, 160301810.95337749, 99792833.393136978 },
            new List<double>(){ -80150905.489229023, -46832166.63224782, 80150905.476688743, 49896416.696568489 },
            new List<double>(){ -40075452.744614512, -23416083.31612391, 40075452.738344371, 24948208.348284245 },
            new List<double>(){ -20037726.372307256, -11708041.658061955, 20037726.369172186, 12474104.174142122 },
            new List<double>(){ -10018863.186153628, -5854020.8290309776, 10018863.184586093, 6237052.0870710611 }
        };

        /// <summary>
        /// summary>
        /// 将LngLat地理坐标系转换为tile瓦片坐标系
        /// </summary>
        /// <param name="lngLat">经纬度信息</param>
        /// <param name="zoom">缩放比例</param>
        /// <param name="pixelOffset">像素偏移</param>
        /// <returns></returns>
        public static TileInfo LngLat2Tile(Vector2D lngLat, int zoom, Vector2 pixelOffset) {
            TileInfo tileInfo = new TileInfo(zoom);
            //转换MC坐标
            Vector2D vector2D = MCTransform.ConvertLL2MC(lngLat);
            //转换到缩放级别
            vector2D *= Math.Pow(2, zoom - 18);
            //偏移像素
            vector2D += pixelOffset;
            tileInfo.tile.x = (int)(vector2D.x / 256.0);
            tileInfo.tile.y = (int)(vector2D.y / 256.0);
            tileInfo.pixel.x = ((int)vector2D.x % 256);
            tileInfo.pixel.y = ((int)vector2D.y % 256);
            return tileInfo;
        }

        /// <summary>
        /// 将LngLat地理坐标系转换为tile瓦片坐标系
        /// </summary>
        /// <param name="lngLat">经纬度信息</param>
        /// <param name="zoom">缩放比例</param>
        public static TileInfo LngLat2Tile(Vector2D lngLat, int zoom) {
            return LngLat2Tile(lngLat, zoom, Vector2.zero);
        }


        /// <summary>
        /// 将tile(瓦片)坐标系转换为LngLat(地理)坐标系
        /// </summary>
        /// <param name="tile">瓦片坐标</param>
        /// <param name="pixel">瓦片上的像素坐标</param>
        /// <param name="zoom">缩放比例</param>
        /// <returns></returns>
        public static Vector2D Tile2LngLat(TileInfo tileInfo, int zoom) {
            Vector2D vector2D = new Vector2D();
            vector2D.x = tileInfo.tile.x * 256.0 + tileInfo.pixel.x;
            vector2D.y = tileInfo.tile.y * 256.0 + tileInfo.pixel.y;
            vector2D /= Math.Pow(2, zoom - 18);
            vector2D.x = MathCommon.GetLoop(vector2D.x, MCRANGE[zoom][0], MCRANGE[zoom][2]);
            vector2D.y = MathCommon.GetLoop(vector2D.y, MCRANGE[zoom][1], MCRANGE[zoom][3]);
            return MCTransform.ConvertMC2LL(vector2D);
        }

        /// <summary>
        /// 经纬度转世界坐标（x，y）
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public static Vector3 LngLat2WorldPos(Vector2D lngLat, int zoom) {
            //转换MC坐标
            Vector2D vector2D = MCTransform.ConvertLL2MC(lngLat);
            //缩小到Unity范围内（-100000, 100000）
            return new Vector2((float)(vector2D.x / 256), (float)(vector2D.y / 256));
        }


        /// <summary>
        /// 返回瓦片的下载URL
        /// </summary>
        /// <param name="lngLat">经纬度</param>
        /// <param name="zoom">缩放比例</param>
        /// <returns></returns>
        public static string GetTileURL(Vector2D lngLat, int zoom) {
            TileInfo tileInfo = LngLat2Tile(lngLat, zoom);
            return GetTileURL(tileInfo, zoom);
        }

        /// <summary>
        /// 返回瓦片的下载URL
        /// </summary>
        /// <param name="tile">瓦片坐标</param>
        /// <param name="zoom">缩放比例</param>
        /// <returns></returns>
        public static string GetTileURL(TileInfo tileInfo, int zoom) {
            return string.Format(MapUrlSrc, tileInfo.tile.x, tileInfo.tile.y, zoom);
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