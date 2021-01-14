using Achonor.LBSMap;
using ShuJun.Event;
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

        EventManager.Register<TouchMoveEvent>((param) => {
            mMapServices.MoveMap(param.MoveOffset);
        }, this);

        EventManager.Register<TouchMovedEvent>((param)=>{
            mMapServices.DoRender();
        }, this);

        EventManager.Register<TouchZoomEvent>((param) => {
            mMapServices.ZoomMap(param.ChangeZoom);
            mMapServices.DoRender();
        }, this);

        print(MCTransform.ConvertLL2MC(new Vector2D(180, 74)));
    }

    private void Start() {
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 21));
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 20));
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 19));
        print(MapFunction.GetTileURL(new Vector2D(112.966696, 28.171502), 3));

        mMapServices.SetZoomLevel(19);
        //mMapServices.SetMapCenter(new Vector2D(0, 0));
        mMapServices.SetMapCenter(new Vector2D(112.888678, 28.213555));
        mMapServices.DoRender();
    }
}
