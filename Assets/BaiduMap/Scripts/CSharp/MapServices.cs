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
        /// 相机的旋转值
        /// </summary>
        private Vector2 mCameraRotate = new Vector2(0, 0);

        /// <summary>
        /// 显示范围
        /// </summary>
        private Rect mLngLatRange = new Rect(70f, 0f, 150f, 55f);
        private Rect mWorldPosRange = new Rect();

        /// <summary>
        /// 相机旋转X轴的范围Min
        /// </summary>
        private float mCameraRotateMinX = -60;
        /// <summary>
        /// 相机旋转X轴的范围Max
        /// </summary>
        private float mCameraRotateMaxX = 60;


        private Dictionary<string, MapTile> mMapTileDict = new Dictionary<string, MapTile>();

        /// <summary>
        /// 最小缩放等级
        /// </summary>
        private static int MIN_ZOOM_LEVEL = 6;

        /// <summary>
        /// 最大的缩放等级，超过后仍然可以缩放，是指不替换图片了
        /// </summary>
        private static int MAX_TILE_ZOOM_LEVEL = 19;
        /// <summary>
        /// 最大缩放等级
        /// </summary>
        private static int MAX_ZOOM_LEVEL = 22;


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
        /// <summary>
        /// 同屏最多显示的数量
        /// </summary>
        private int mMaxTileCount = 30;

        public Camera MapCamera {
            get {
                return mMapCamera;
            }
        }

        public float MapZoomLevel {
            get {
                return mMapZoomLevel;
            }
        }

        public int MapTileZoomLevel {
            get {
                return (int)Mathf.Clamp(mMapZoomLevel, MIN_ZOOM_LEVEL, MAX_TILE_ZOOM_LEVEL);
            }
        }

        private void Start() {
            SetLngLatRange(mLngLatRange);
            //检测缓存容量是否超出
            CheckCahcheSize();
            //启动缓存池
            StartCoroutine(MapTexturePool.StartPool());
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
        /// <param name="lngLat">BD09ll坐标系</param>
        public void SetMapCenter(Vector2D lngLat) {
            mCenterLngLat = lngLat;
            //相机坐标
            Vector3 centerPos = LngLat2WorldPos(mCenterLngLat);
            //相机高度
            Vector3 heightVec = new Vector3(0, (float)MapFunction.GetCameraHeight(mMapZoomLevel), 0);
            Vector3 cameraPos = centerPos + heightVec;
            mMapCamera.transform.position = cameraPos;
            UpdateCameraTransform(Vector3.zero);
            DoRender();
        }

        /// <summary>
        /// 地图缩放
        /// </summary>
        /// <param name="zoom">变化量（0， 1）</param>
        public void ZoomMap(float zoom) {
            mMapZoomLevel += zoom;
            mMapZoomLevel = Mathf.Clamp(mMapZoomLevel, MIN_ZOOM_LEVEL, MAX_ZOOM_LEVEL);
            UpdateCameraTransform(Vector3.zero);
        }

        /// <summary>
        /// 移动地图
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void MoveMap(float x, float y) {
            MoveMap(new Vector2(x, y));
        }

        /// <summary>
        /// 移动地图
        /// </summary>
        /// <param name="offset">屏幕像素单位</param>
        public void MoveMap(Vector2 offset) {
            UpdateCameraTransform(offset);
        }

        public void RotateMap(Vector2 offset) {
            mCameraRotate += offset;
            mCameraRotate.x = Mathf.Clamp(mCameraRotate.x, mCameraRotateMinX, mCameraRotateMaxX);
            UpdateCameraTransform(Vector3.zero);
        }

        public void UpdateCameraTransform(Vector3 posOffset) {
            //-----------------------计算位置-----------------------//
            if (0 != posOffset.x || 0 != posOffset.y) {
                //相机位置
                Vector3 vector = new Vector3(posOffset.x, posOffset.y, 0);
                Vector3 worldOffset = mMapCamera.transform.TransformDirection(vector) * MapFunction.GetMapScale(mMapZoomLevel);
                worldOffset.y = 0;
                mMapCamera.transform.position -= worldOffset;
                //判断移动是否超出范围
                Vector3 nextCameraPos = mMapCamera.transform.position;
                Vector3 leftDownWorldPos = ScreenToWorldPoint(new Vector3(0, 0));
                Vector3 rightUpWorldPos = ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
                if (leftDownWorldPos.x < mWorldPosRange.x) {
                    nextCameraPos.x += mWorldPosRange.x - leftDownWorldPos.x;
                } else if (mWorldPosRange.width < rightUpWorldPos.x) {
                    nextCameraPos.x += mWorldPosRange.width - rightUpWorldPos.x;
                }
                if (leftDownWorldPos.z < mWorldPosRange.y) {
                    nextCameraPos.z += mWorldPosRange.y - leftDownWorldPos.z;
                } else if (mWorldPosRange.height < rightUpWorldPos.z) {
                    nextCameraPos.z += mWorldPosRange.height - rightUpWorldPos.z;
                }
                mMapCamera.transform.position = nextCameraPos;
                float camera2CenterDistance = (float)MapFunction.GetCameraHeight(mMapZoomLevel);
                Vector3 centerWorldPos = mMapCamera.ScreenToWorldPoint(new Vector3(0.5f * Screen.width, 0.5f * Screen.height, camera2CenterDistance));
                //计算中心经纬度
                mCenterLngLat = WorldPos2LngLat(centerWorldPos);
            }
            //-----------------------计算旋转-----------------------//
            //中心坐标
            Vector3 centerPos = LngLat2WorldPos(mCenterLngLat);
            //相机未旋转时在中心点空间下的坐标
            Vector3 heightVec = new Vector3(0, (float)MapFunction.GetCameraHeight(mMapZoomLevel), 0);
            //旋转矩阵
            Matrix4x4 rotateMatrix = Matrix4x4.Rotate(Quaternion.Euler(mCameraRotate.x, mCameraRotate.y, 0));
            //相机在中心的空间下的坐标
            Vector3 cameraPos = rotateMatrix.MultiplyVector(heightVec);
            //转到世界坐标
            cameraPos += centerPos;
            mMapCamera.transform.position = cameraPos;
            //朝向中心点，先计算up向量
            float angle = Vector3.Angle(-heightVec, centerPos - cameraPos);
            if (Mathf.Abs(angle) <= 0.001f) {
                Vector3 lastUp = mMapCamera.transform.TransformDirection(Vector3.up);
                mMapCamera.transform.LookAt(centerPos, lastUp);
            } else if (0 < mCameraRotate.x) {
                mMapCamera.transform.LookAt(centerPos, -Vector3.up);
            } else {
                mMapCamera.transform.LookAt(centerPos, Vector3.up);
            }
        }

        /// <summary>
        /// 渲染地图
        /// </summary>
        public void DoRender() {
            Vector3 centerPos = LngLat2WorldPos(mCenterLngLat);
            float camera2CenterDistance = (float)MapFunction.GetCameraHeight(mMapZoomLevel);
            //计算相机范围
            Vector3 cameraPos = mMapCamera.transform.position;
            //计算相机从屏幕左下角发出的射线和地图层的交点（计算直线和面的交点）
            Vector3 leftDownWorldPos = ScreenToWorldPoint(new Vector3(-0.1f * Screen.width, -0.1f * Screen.height, 1));
            Vector3 leftDownDir = (leftDownWorldPos - cameraPos);
            leftDownWorldPos = MapFunction.CalcLineAndPlanePoint(cameraPos, leftDownDir, centerPos, Vector3.up);
            //有哪些Tile要加入
            TileData leftDownTile = WorldPos2TileData(leftDownWorldPos);
            TileData centerTile = WorldPos2TileData(centerPos);
            List<TileData> allTileDatas = new List<TileData>();
            int minX = Mathf.Min(leftDownTile.tile.x, centerTile.tile.x);
            int maxX = Mathf.Max(leftDownTile.tile.x, centerTile.tile.x);
            int minY = Mathf.Min(leftDownTile.tile.y, centerTile.tile.y);
            int maxY = Mathf.Max(leftDownTile.tile.y, centerTile.tile.y);

            for (int i = 0; i <= (maxX - minX) * 2.5f; i++) {
                for (int k = 0; k <= (maxY - minY) * 2.5f; k++) {
                    allTileDatas.Add(new TileData(MapTileZoomLevel, minX + i, minY + k));
                }
            }
            //按照距离排序，距离近的优先加载
            allTileDatas.Sort((tileData1, tileData2)=> {
                double distance1 = centerTile.Distance(tileData1);
                double distance2 = centerTile.Distance(tileData2);
                if (distance1.DoubleIsEqual(distance2)) {
                    return 0;
                }
                return distance1 < distance2 ? -1 : 1;
            });
            //最大数量
            while (mMaxTileCount < allTileDatas.Count) {
                allTileDatas.RemoveAt(allTileDatas.Count - 1);
            }
            for (int i = 0; i < allTileDatas.Count; i++) {
                MapTile mapTile = GetMapTile(allTileDatas[i]);
                mapTile.gameObject.name = mapTile.Data.Key;
                mapTile.transform.position = TileData2WorldPos(allTileDatas[i]);
            }
            //清理距离远的Tile
            if (0 < allTileDatas.Count) {
                TileData maxDisTile = allTileDatas[allTileDatas.Count - 1];
                double maxDistance = maxDisTile.Distance(centerTile);
                ClearMapTile(centerTile, maxDistance);
            }
        }

        /// <summary>
        /// 经纬度转世界坐标
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public Vector3 LngLat2WorldPos(Vector2D lngLat) {
            Vector3 position = MapFunction.LngLat2Position(lngLat);
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

        /// <summary>
        /// 屏幕坐标转经纬度
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public Vector2D ScreenPos2LngLat(Vector2 screenPos) {
            Vector3 worldPos = ScreenToWorldPoint(screenPos);
            return WorldPos2LngLat(worldPos);
        }


        /// <summary>
        /// 经纬度转屏幕
        /// </summary>
        /// <param name="lngLat"></param>
        /// <returns></returns>
        public Vector2 LngLat2ScreenPos(Vector2D lngLat) {
            Vector3 worldPos = LngLat2WorldPos(lngLat);
            return WorldToScreenPoint(worldPos);
        }

        public Vector3 ScreenToWorldPoint(Vector2 screenPos) {
            double screenX = screenPos.x;
            double screenY = screenPos.y;
            double screenZ = MapFunction.GetCameraHeight(mMapZoomLevel);
            Matrix4x4 pMatrix = mMapCamera.projectionMatrix;

            //反齐次除法，求出裁剪空间坐标
            double px = screenX / Screen.width;
            px = (px - 0.5f) * 2f;
            double py = screenY / Screen.height;
            py = (py - 0.5f) * 2f;
            double pz = (-screenZ - pMatrix.m23) / pMatrix.m22;
            double pw = screenZ;
            px *= pw;
            py *= pw;
            //裁剪空间到相机空间
            Matrix4x4 pInverseMatrix = mMapCamera.projectionMatrix.inverse;
            double vx = (pInverseMatrix.m00 * px + pInverseMatrix.m01 * py + pInverseMatrix.m02 * pz + pInverseMatrix.m03 * pw);
            double vy = (pInverseMatrix.m10 * px + pInverseMatrix.m11 * py + pInverseMatrix.m12 * pz + pInverseMatrix.m13 * pw);
            double vz = (pInverseMatrix.m20 * px + pInverseMatrix.m21 * py + pInverseMatrix.m22 * pz + pInverseMatrix.m23 * pw);
            //观察空间到世界空间
            Matrix4x4 vInverseMatrix = mMapCamera.worldToCameraMatrix.inverse;
            double x = (vInverseMatrix.m00 * vx + vInverseMatrix.m01 * vy + vInverseMatrix.m02 * vz + vInverseMatrix.m03 * 1);
            double y = (vInverseMatrix.m10 * vx + vInverseMatrix.m11 * vy + vInverseMatrix.m12 * vz + vInverseMatrix.m13 * 1);
            double z = (vInverseMatrix.m20 * vx + vInverseMatrix.m21 * vy + vInverseMatrix.m22 * vz + vInverseMatrix.m23 * 1);
            return new Vector3((float)(x), (float)(y), (float)(z));
        }

        public Vector3 WorldToScreenPoint(Vector3 worldPos) {
            double worldX = worldPos.x;
            double worldY = worldPos.y;
            double worldZ = worldPos.z;

            //世界空间到观察空间
            Matrix4x4 vMatrix = mMapCamera.worldToCameraMatrix;
            double vx = (vMatrix.m00 * worldX + vMatrix.m01 * worldY + vMatrix.m02 * worldZ + vMatrix.m03 * 1);
            double vy = (vMatrix.m10 * worldX + vMatrix.m11 * worldY + vMatrix.m12 * worldZ + vMatrix.m13 * 1);
            double vz = (vMatrix.m20 * worldX + vMatrix.m21 * worldY + vMatrix.m22 * worldZ + vMatrix.m23 * 1);
            //相机空间到裁剪空间
            Matrix4x4 pMatrix = mMapCamera.projectionMatrix;
            double px = (pMatrix.m00 * vx + pMatrix.m01 * vy + pMatrix.m02 * vz + pMatrix.m03 * 1);
            double py = (pMatrix.m10 * vx + pMatrix.m11 * vy + pMatrix.m12 * vz + pMatrix.m13 * 1);
            double pz = (pMatrix.m20 * vx + pMatrix.m21 * vy + pMatrix.m22 * vz + pMatrix.m23 * 1);
            double pw = (pMatrix.m30 * vx + pMatrix.m31 * vy + pMatrix.m32 * vz + pMatrix.m33 * 1);
            //齐次除法
            double x = px / pw;
            double y = py / pw;

            //转到0-1的范围
            x = (x * 0.5) + 0.5;
            y = (y * 0.5) + 0.5;
            return new Vector3((float)(x * Screen.width), (float)(y * Screen.height), (float)(-vz));
        }


        public Vector3 TileData2WorldPos(TileData tileData) {
            Vector2D lngLat = MapFunction.Tile2LngLat(tileData);
            return LngLat2WorldPos(lngLat);
        }

        public TileData WorldPos2TileData(Vector3 worldPos, int zoom = -1) {
            if (-1 == zoom){
                zoom = MapTileZoomLevel;   
            }
            Vector2D lngLat = WorldPos2LngLat(worldPos);
            return MapFunction.LngLat2Tile(lngLat, zoom);
        }

        /// <summary>
        /// 当前地图的缩放比例
        /// </summary>
        /// <returns></returns>
        public float GetMapScale() {
            return MapFunction.GetMapScale(mMapZoomLevel);
        }

        public float GetTileScale() {
            return MapFunction.GetTileScale((int)mMapZoomLevel);
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
                if (1 <= Mathf.Abs(MapTileZoomLevel - mapTile.Data.zoom)) {
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
            print(MapTexturePool.MapTileCachePath);
            string cachePath = MapTexturePool.MapTileCachePath;
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