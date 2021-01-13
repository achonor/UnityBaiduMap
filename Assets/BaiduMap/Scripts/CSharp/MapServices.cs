using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shujun.LBSMap {
    public class MapServices : MonoBehaviour {
        [SerializeField]
        private Camera mMapCamera;

        /// <summary>
        /// 最小缩放等级
        /// </summary>
        private static int MIN_ZOOM_LEVEL = 4;
        /// <summary>
        /// 最大缩放等级
        /// </summary>
        private static int MAX_ZOOM_LEVEL = 19;

        /// <summary>
        /// 当前地图缩放级别
        /// </summary>
        private int mMapZoomLevel = 16;
        /// <summary>
        /// 地图中心经纬度
        /// </summary>
        private Vector2D mCenterLngLat;

        //设置地图缩放级别
        public void SetZoomLevel(int zoom) {
            mMapZoomLevel = Mathf.Clamp(zoom, MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL);
        }

        public void SetMapCenter(Vector2D lngLat) {
            mCenterLngLat = lngLat;
        }

        /// <summary>
        /// 渲染地图
        /// </summary>
        public void DoRender() {
            Vector3 centerPos = MapFunction.LngLat2WorldPos(mCenterLngLat, mMapZoomLevel);
            centerPos.z = 0;
            print(centerPos);
            mMapCamera.transform.position = centerPos;
        }
    }
}