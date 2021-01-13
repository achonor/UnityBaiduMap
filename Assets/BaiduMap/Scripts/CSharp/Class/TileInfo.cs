using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo {
    public int zoom;
    public Vector2Int tile = new Vector2Int();
    public Vector2 pixel = new Vector2Int();

    public string Key {
        get {
            return string.Format("{0}|{1}|{2}", zoom, tile.x, tile.y);
        }
    }

    public TileInfo(TileInfo tileInfo) {
        zoom = tileInfo.zoom;
        tile.x = tileInfo.tile.x;
        tile.y = tileInfo.tile.y;
        pixel.x = tileInfo.pixel.x;
        pixel.y = tileInfo.pixel.y;
    }

    public TileInfo(int _zoom, int tileX = 0, int tileY = 0, int pixelX = 0, int pixelY = 0) {
        zoom = _zoom;
        tile.x = tileX;
        tile.y = tileY;
        pixel.x = pixelX;
        pixel.y = pixelY;
    }

    public TileInfo OffsetTile(Vector2Int offset) {
        tile += offset;
        return this;
    }

    public override string ToString() {
        return string.Format("({0}, {1}, {2})", zoom, tile.ToString(), pixel.ToString());
    }
}
