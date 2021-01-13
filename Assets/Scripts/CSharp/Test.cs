using Shujun.LBSMap;
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
        mMapServices.SetZoomLevel(19);
        mMapServices.SetMapCenter(new Vector2D(-180, 74));
        mMapServices.DoRender();
    }
}
