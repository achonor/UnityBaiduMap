using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Achonor.LBSMap {
    public static class MathCommon
    {
        public static bool FloatIsEqual(this float value1, float value2) {
            if (Math.Abs(value1 - value2) <= 1e-8) {
                return true;
            }
            return false;
        }

        public static bool DoubleIsEqual(this double value1, double value2) {
            if (Math.Abs(value1 - value2) <= 1e-8) {
                return true;
            }
            return false;
        }

        public static double GetRange(this double value, double min, double max) {
            if (0 != min) {
                value = value > min ? value : min;
            }
            if (0 != max) {
                value = value > max ? max : value;
            }
            return value;
        }

        public static double GetLoop(this double value, double min, double max) {
            while (value > max) {
                value -= max - min;
            }
            while (value < min) {
                value += max - min;
            }
            return value;
        }
    }
}
