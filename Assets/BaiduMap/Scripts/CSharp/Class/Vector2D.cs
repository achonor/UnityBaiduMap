using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2D {
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

    public Vector2D() {
        x = 0;
        y = 0;
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

    public static Vector2D operator *(Vector2D self, double value) {
        return new Vector2D(self.x * value, self.y * value);
    }

    public static Vector2D operator /(Vector2D self, double value) {
        return new Vector2D(self.x / value, self.y / value);
    }

    public static Vector2D operator +(Vector2D self, UnityEngine.Vector2 value) {
        return new Vector2D(self.x + value.x, self.y + value.y);
    }
}
