using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Achonor.LBSMap {
    public class MapServices : MonoBehaviour {
        [SerializeField]
        private Camera mMapCamera;

        [SerializeField]
        private Transform mTileParent;

        [SerializeField]
        private Transform mTilePool;

        private Dictionary<string, MapTile> mMapTileDict = new Dictionary<string, MapTile>();

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
            //相机坐标
            Vector3 centerPos = LngLat2WorldPos(mCenterLngLat);
            //相机高度
            Vector3 heightVec = new Vector3(0, MapFunction.GetCameraHeight(mMapZoomLevel), 0);
            Vector3 cameraPos = centerPos + transform.TransformPoint(heightVec);
            mMapCamera.transform.position = cameraPos;
            //计算相机范围
            Vector3 leftDownWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(0, 0, heightVec.y));
            Vector3 rightUpWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, heightVec.y));
            TileData leftDownTile = WorldPos2TileData(leftDownWorldPos);
            TileData rightUpTile = WorldPos2TileData(rightUpWorldPos);
            List<TileData> allTileDatas = new List<TileData>();
            for (int i = leftDownTile.tile.x; i <= rightUpTile.tile.x; i++) {
                for (int k = leftDownTile.tile.y; k <= rightUpTile.tile.y; k++) {
                    allTileDatas.Add(new TileData(mMapZoomLevel, i, k));
                }
            }
            //中心Tile
            TileData centerTileData = WorldPos2TileData(centerPos);
            //按照距离排序，距离近的优先加载
            allTileDatas.Sort((tileData1, tileData2)=> {
                double distance1 = centerTileData.Distance(tileData1);
                double distance2 = centerTileData.Distance(tileData2);
                if (distance1.DoubleIsEqual(distance2)) {
                    return 0;
                }
                return distance1 < distance2 ? -1 : 1;
            });
            for (int i = 0; i < allTileDatas.Count; i++) {
                MapTile mapTile = GetMapTile(allTileDatas[i]);
                mapTile.transform.position = TileData2WorldPos(allTileDatas[i]);
            }
        }

        /// <summary>
        /// 经纬度转世界坐标
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public Vector3 LngLat2WorldPos(Vector2D lngLat, int zoom = -1) {
            if (-1 == zoom) {
                zoom = mMapZoomLevel;
            }
            Vector2 position = MapFunction.LngLat2Position(lngLat);
            return mTileParent.TransformPoint(position);
        }

        /// <summary>
        /// 世界坐标转经纬度
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public Vector2D WorldPos2LngLat(Vector3 worldPos) {
            Vector3 position = mTileParent.InverseTransformPoint(worldPos);
            return MapFunction.Position2LngLat(position);
        }

        
        public Vector3 TileData2WorldPos(TileData tileData) {
            Vector2D lngLat = MapFunction.Tile2LngLat(tileData);
            return LngLat2WorldPos(lngLat);
        }

        public TileData WorldPos2TileData(Vector3 worldPos, int zoom = -1) {
            if (-1 == zoom){
                zoom = mMapZoomLevel;   
            }
            Vector2D lngLat = WorldPos2LngLat(worldPos);
            return MapFunction.LngLat2Tile(lngLat, zoom);
        }

        private MapTile GetMapTile(TileData tileData) {
            if (mMapTileDict.ContainsKey(tileData.Key)) {
                return mMapTileDict[tileData.Key];
            }
            Transform newTile = null;
            if (1 < mTilePool.childCount) {
                newTile = mTilePool.GetChild(1);
            } else {
                Transform sources = mTilePool.GetChild(0);
                newTile = Instantiate(sources);
            }
            newTile.SetParent(mTileParent);
            MapTile mapTile = newTile.GetComponent<MapTile>();
            mMapTileDict.Add(tileData.Key, mapTile);
            mapTile.Init(tileData);
            return mapTile;
        }

        private void RemoveTile(MapTile mapTile) {
            mapTile.gameObject.SetActive(false);
            mapTile.transform.SetParent(mTilePool);
            mMapTileDict.Remove(mapTile.Key);
        }
    }
}