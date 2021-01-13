using Achonor.LBSMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private MapServices mMapServices;

    private void Awake() {
        if (null == mMapServices) {
            mMapServices = GetComponent<MapServices>();
        }
    }

    private void Start() {
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 21));
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 20));
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 19));
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 3));

        mMapServices.SetZoomLevel(16);
        mMapServices.SetMapCenter(new Vector2D(112.966696, 28.171502));
        mMapServices.DoRender();
    }
}
