using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Achonor.LBSMap {
    public class MapServices : MonoBehaviour {
        [SerializeField]
        private Camera mMapCamera;

        [SerializeField]
        private Transform mTileParent;

        [SerializeField]
        private Transform mTilePool;

        private MapType mMapType;

        /// <summary>
        /// 显示范围
        /// </summary>
        private Rect mLngLatRange = new Rect(-180f, -74f, 180f, 74f);
        private Rect mWorldPosRange = new Rect();

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
        private float mMapZoomLevel = 16;
        /// <summary>
        /// 地图中心经纬度
        /// </summary>
        private Vector2D mCenterLngLat;

        /// <summary>
        /// 最大缓存容量
        /// </summary>
        private long mMaxChacheSize = 500 * 1024 * 1024;

        private void Start() {
            SetLngLatRange(mLngLatRange);
            //检测缓存容量是否超出
            CheckCahcheSize();
        }

        /// <summary>
        ///  设置缓存容量
        /// </summary>
        /// <param name="mbSize">单位（MB）</param>
        public void SetCacheSize(long mbSize) {
            mMaxChacheSize = mbSize * 1024 * 1024;
        }

        public void SetLngLatRange(Rect rect) {
            mLngLatRange = rect;
            //初始化世界坐标范围
            Vector3 minVector = LngLat2WorldPos(new Vector2D(rect.x, rect.y));
            Vector3 maxVector = LngLat2WorldPos(new Vector2D(rect.width, rect.height));
            mWorldPosRange = new Rect(minVector.x, minVector.z, maxVector.x, maxVector.z);
        }

        /// <summary>
        /// 设置地图类型
        /// </summary>
        /// <param name="mapType"></param>
        public void SetMapType(MapType mapType) {
            if (mMapType == mapType) {
                return;
            }
            mMapType = mapType;
            ClearAllMapTile();
        }

        /// <summary>
        /// 设置地图缩放级别
        /// </summary>
        /// <param name="zoom"></param>
        public void SetZoomLevel(int zoom) {
            mMapZoomLevel = Mathf.Clamp(zoom, MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL);
        }

        /// <summary>
        /// 设置地图中心经纬度
        /// </summary>
        /// <param name="lngLat"></param>
        public void SetMapCenter(Vector2D lngLat) {
            mCenterLngLat = lngLat;
        }

        /// <summary>
        /// 地图缩放
        /// </summary>
        /// <param name="zoom">变化量（0， 1）</param>
        public void ZoomMap(float zoom) {
            mMapZoomLevel += zoom;
            mMapZoomLevel = Mathf.Clamp(mMapZoomLevel, MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL);
        }

        /// <summary>
        /// 移动地图
        /// </summary>
        /// <param name="offset">屏幕像素单位</param>
        public void MoveMap(Vector2 offset) {
            //相机位置
            Vector3 vector = new Vector3(offset.x, 0, offset.y) ;
            Vector3 worldOffset = transform.TransformPoint(vector * MapFunction.GetCameraMoveScale(mMapZoomLevel));
            mMapCamera.transform.position -= worldOffset;
            //判断移动是否超出范围
            Vector3 nextCameraPos = mMapCamera.transform.position;
            float cameraHeight = MapFunction.GetCameraHeight(mMapZoomLevel);
            Vector3 leftDownWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(0, 0, cameraHeight));
            Vector3 rightUpWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cameraHeight));
            if (leftDownWorldPos.x < mWorldPosRange.x) {
                nextCameraPos.x += mWorldPosRange.x - leftDownWorldPos.x;
            } else if (mWorldPosRange.width < rightUpWorldPos.x) {
                nextCameraPos.x += mWorldPosRange.width - rightUpWorldPos.x;
            }
            if (leftDownWorldPos.y < mWorldPosRange.y) {
                nextCameraPos.y += mWorldPosRange.y - leftDownWorldPos.y;
            } else if (mWorldPosRange.height < rightUpWorldPos.y) {
                nextCameraPos.y += mWorldPosRange.height - rightUpWorldPos.y;
            }
            mMapCamera.transform.position = nextCameraPos;
            //计算中心经纬度
            mCenterLngLat = WorldPos2LngLat(mMapCamera.transform.position);
        }

        /// <summary>
        /// 渲染地图
        /// </summary>
        public void DoRender() {
            //相机坐标
            Vector3 centerPos = LngLat2WorldPos(mCenterLngLat);
            //相机高度
            Vector3 heightVec = new Vector3(0, MapFunction.GetCameraHeight(mMapZoomLevel), 0);
            Vector3 cameraPos = centerPos + heightVec;
            mMapCamera.transform.position = cameraPos;
            //计算相机范围
            Vector3 leftDownWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(-0.5f * Screen.width, -0.5f * Screen.height, heightVec.y));
            Vector3 rightUpWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(1.5f * Screen.width, 1.5f * Screen.height, heightVec.y));
            TileData leftDownTile = WorldPos2TileData(leftDownWorldPos);
            TileData rightUpTile = WorldPos2TileData(rightUpWorldPos);
            List<TileData> allTileDatas = new List<TileData>();
            for (int i = leftDownTile.tile.x; i <= rightUpTile.tile.x; i++) {
                for (int k = leftDownTile.tile.y; k <= rightUpTile.tile.y; k++) {
                    allTileDatas.Add(new TileData((int)mMapZoomLevel, i, k));
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
            //清理距离远的Tile
            if (0 < allTileDatas.Count) {
                TileData maxDisTile = allTileDatas[allTileDatas.Count - 1];
                double maxDistance = maxDisTile.Distance(centerTileData);
                ClearMapTile(centerTileData, maxDistance);
            }
        }

        /// <summary>
        /// 经纬度转世界坐标
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public Vector3 LngLat2WorldPos(Vector2D lngLat) {
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
                zoom = (int)mMapZoomLevel;   
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
            mapTile.Init(tileData, mMapType);
            return mapTile;
        }

        private void RemoveTile(MapTile mapTile) {
            mapTile.gameObject.SetActive(false);
            mapTile.transform.SetParent(mTilePool);
            mMapTileDict.Remove(mapTile.Key);
        }

        private void ClearMapTile(TileData centerTileData, double maxDistance) {
            List<MapTile> needClears = new List<MapTile>();
            foreach (MapTile mapTile in mMapTileDict.Values) {
                if (2 <= Mathf.Abs((int)mMapZoomLevel - mapTile.Data.zoom)) {
                    needClears.Add(mapTile);
                }
                if (2 * maxDistance < mapTile.Data.Distance(centerTileData)) {
                    needClears.Add(mapTile);
                }
            }
            for (int i = 0; i < needClears.Count; i++) {
                RemoveTile(needClears[i]);
            }
        }

        /// <summary>
        /// 清理所有Tile
        /// </summary>
        private void ClearAllMapTile() {
            List<MapTile> needClears = new List<MapTile>();
            foreach (MapTile mapTile in mMapTileDict.Values) {
                needClears.Add(mapTile);
            }
            for (int i = 0; i < needClears.Count; i++) {
                RemoveTile(needClears[i]);
            }
        }

        private void CheckCahcheSize() {
            long curChacheSize = 0;
            print(MapHttpTools.MapTileCachePath);
            string cachePath = MapHttpTools.MapTileCachePath;
            if (!Directory.Exists(cachePath)) {
                return;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(cachePath);
            FileInfo[] allFiles = directoryInfo.GetFiles(); 
            print("缓存文件数量 ：" + allFiles.Length);
            for (int i = 0; i < allFiles.Length; i++) {
                curChacheSize += allFiles[i].Length;
            }
            print("缓存大小 ：" + curChacheSize);
            print("缓存上限 ：" + mMaxChacheSize);
            if (curChacheSize < mMaxChacheSize) {
                return;
            }
            //按照访问时间排序
            List<FileInfo> allFileList = new List<FileInfo>(allFiles);
            allFileList.Sort((file1, file2) => {
                if (file1.LastAccessTime != file2.LastAccessTime) {
                    return file1.LastAccessTime < file2.LastAccessTime ? 1 : -1;
                }
                return 0;
            });
            for (int i = allFileList.Count - 1; 0 <= i; i--) {
                if (curChacheSize < mMaxChacheSize) {
                    break;
                }
                print("删除文件 :" + allFileList[i].Name);
                curChacheSize -= allFileList[i].Length;
                allFileList[i].Delete();
            }
        }
    }
}