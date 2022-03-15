using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Achonor.LBSMap {
    /// <summary>
    /// 经纬度墨卡托转换
    /// </summary>
    public static class MCTransform {
        static double DEF_PI = 3.14159265359; // PI
        static double DEF_2PI = 6.28318530712; // 2 * PI
        static double DEF_PI180 = 0.01745329252; // PI / 180.0
        static double DEF_R = 6370693.5; // radius of earth

        private static List<List<double>> LL2MC = new List<List<double>>(){
            new List<double>() {-0.0015702102444, 111320.7020616939, 1704480524535203, -10338987376042340, 26112667856603880, -35149669176653700, 26595700718403920, -10725012454188240, 1800819912950474, 82.5},
            new List<double>() {0.0008277824516172526, 111320.7020463578, 647795574.6671607, -4082003173.641316, 10774905663.51142, -15171875531.51559, 12053065338.62167,  -5124939663.577472, 913311935.9512032, 67.5},
            new List<double>() {0.00337398766765, 111320.7020202162, 4481351.045890365, -23393751.19931662, 79682215.47186455, -115964993.2797253, 97236711.15602145, -43661946.33752821, 8477230.501135234, 52.5},
            new List<double>() {0.00220636496208, 111320.7020209128, 51751.86112841131, 3796837.749470245, 992013.7397791013, -1221952.21711287, 1340652.697009075, -620943.6990984312, 144416.9293806241, 37.5},
            new List<double>() {-0.0003441963504368392, 111320.7020576856, 278.2353980772752, 2485758.690035394, 6070.750963243378, 54821.18345352118, 9540.606633304236, -2710.55326746645, 1405.483844121726, 22.5},
            new List<double>() {-0.0003218135878613132, 111320.7020701615, 0.00369383431289, 823725.6402795718, 0.46104986909093, 2351.343141331292, 1.58060784298199, 8.77738589078284, 0.37238884252424, 7.45}
        };
        private static List<List<double>> MC2LL = new List<List<double>>() {
            new List<double>() { 1.410526172116255e-8, 0.00000898305509648872, -1.9939833816331, 200.9824383106796, -187.2403703815547, 91.6087516669843, -23.38765649603339, 2.57121317296198, -0.03801003308653, 17337981.2 },
            new List<double>() { -7.435856389565537e-9, 0.000008983055097726239, -0.78625201886289, 96.32687599759846, -1.85204757529826, -59.36935905485877, 47.40033549296737, -16.50741931063887, 2.28786674699375, 10260144.86 },
            new List<double>() { -3.030883460898826e-8, 0.00000898305509983578, 0.30071316287616, 59.74293618442277, 7.357984074871, -25.38371002664745, 13.45380521110908, -3.29883767235584, 0.32710905363475, 6856817.37 },
            new List<double>() { -1.981981304930552e-8, 0.000008983055099779535, 0.03278182852591, 40.31678527705744, 0.65659298677277, -4.44255534477492, 0.85341911805263, 0.12923347998204, -0.04625736007561, 4482777.06 },
            new List<double>() { 3.09191371068437e-9, 0.000008983055096812155, 0.00006995724062, 23.10934304144901, -0.00023663490511, -0.6321817810242, -0.00663494467273, 0.03430082397953, -0.00466043876332, 2555164.4 },
            new List<double>() { 2.890871144776878e-9, 0.000008983055095805407, -3.068298e-8, 7.47137025468032, -0.00000353937994, -0.02145144861037, -0.00001234426596, 0.00010322952773, -0.00000323890364, 826088.5 }
        };

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

        private static double[] MCBAND = { 12890594.86, 8362377.87, 5591021d, 3481989.83, 1678043.12, 0d };
        private static int[] LLBAND = { 75, 60, 45, 30, 15, 0 };

        private static Vector2D Convertor(double longitude, double latitude, double[] cg) {
            if (cg == null) {
                return Vector2D.Infinity;
            }
            double t = cg[0] + cg[1] * Math.Abs(longitude);
            double ce = Math.Abs(latitude) / cg[9];
            double ch = cg[2] + cg[3] * ce + cg[4] * Math.Pow(ce, 2) + cg[5] * Math.Pow(ce, 3) + cg[6] * Math.Pow(ce, 4) + cg[7] * Math.Pow(ce, 5) + cg[8] * Math.Pow(ce, 6);
            t = t * (longitude < 0 ? -1 : 1);
            ch = ch * (latitude < 0 ? -1 : 1);
            return new Vector2D(t, ch);
        }

        /// <summary>
        /// 经纬度转墨卡托坐标
        /// </summary>
        /// <param name="lngLat">经纬度</param>
        /// <returns></returns>
        public static Vector2D ConvertLL2MC(Vector2D lngLat) {
            return ConvertLL2MC(lngLat.longitude, lngLat.latitude);
        }

        /// <summary>
        /// 经纬度转墨卡托坐标
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns></returns>
        public static Vector2D ConvertLL2MC(double longitude, double latitude) {
            longitude = MathCommon.GetLoop(longitude, -180, 180);
            latitude = MathCommon.GetRange(latitude, -74, 74);
            double[] cg = null;
            for (int cf = 0; cf < LLBAND.Length; cf++) {
                if (latitude >= LLBAND[cf]) {
                    cg = LL2MC[cf].ToArray();
                    break;
                }
            }
            if (cg == null) {
                for (int cf = LLBAND.Length - 1; cf >= 0; cf--) {
                    if (latitude <= -LLBAND[cf]) {
                        cg = LL2MC[cf].ToArray();
                        break;
                    }
                }
            }
            return Convertor(longitude, latitude, cg);
        }

        /// <summary>
        /// 墨卡托坐标转经纬度坐标
        /// </summary>
        /// <param name="vector2D">MC坐标</param>
        /// <returns></returns>
        public static Vector2D ConvertMC2LL(Vector2D vector2D) {
            vector2D.x = MathCommon.GetRange(vector2D.x, -20037726.3691722, 20037726.3691722);
            vector2D.y = MathCommon.GetRange(vector2D.y, -12474104.1741421, 12474104.1741421);
            return ConvertMC2LL(vector2D.x, vector2D.y);
        }

        /// <summary>
        /// 墨卡托坐标转经纬度坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector2D ConvertMC2LL(double x, double y) {
            double[] cF = null;
            double newX = Math.Abs(x);
            double newY = Math.Abs(y);

            for (int cE = 0; cE < MCBAND.Length; cE++) {
                if (newY >= MCBAND[cE]) {
                    cF = MC2LL[cE].ToArray();
                    break;
                }
            }
            newX *= (x < 0 ? -1 : 1);
            newY *= (y < 0 ? -1 : 1);

            Vector2D result = Convertor(newX, newY, cF);
            return result;
        }
    }
}