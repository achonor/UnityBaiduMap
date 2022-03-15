using Achonor.LBSMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vector2D {
    public static Vector2D Infinity = new Vector2D(double.PositiveInfinity, double.PositiveInfinity);
    public double x;
    public double y;
    public double longitude {
        get {
            return x;
        }
        set {
            x = value;
        }
    }
    public double latitude {
        get {
            return y;
        }
        set {
            y = value;
        }
    }

    public Vector2D(double _x, double _y) {
        x = _x;
        y = _y;
    }

    public Vector2D(UnityEngine.LocationInfo locationInfo) {
        x = locationInfo.longitude;
        y = locationInfo.latitude;
    }

    public override string ToString() {
        return string.Format("{0},{1}", x, y);
    }
    public static bool operator ==(Vector2D self, Vector2D value) {
        return self.x.DEquals(value.x) && self.y.DEquals(value.y);
    }
    public static bool operator !=(Vector2D self, Vector2D value) {
        return (!self.x.DEquals(value.x)) || (!self.y.DEquals(value.y));
    }

    public static Vector2D operator *(Vector2D self, double value) {
        return new Vector2D(self.x * value, self.y * value);
    }

    public static Vector2D operator /(Vector2D self, double value) {
        return new Vector2D(self.x / value, self.y / value);
    }

    public static Vector2D operator +(Vector2D self, UnityEngine.Vector2 value) {
        return new Vector2D(self.x + value.x, self.y + value.y);
    }

    public double Distance(Vector2D vector2D) {
        return System.Math.Sqrt((x - vector2D.x) * (x - vector2D.x) + (y - vector2D.y) * (y - vector2D.y));
    }
}
