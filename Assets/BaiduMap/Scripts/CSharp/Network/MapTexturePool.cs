using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Achonor.LBSMap
{
    /// <summary>
    /// Texture缓存池
    /// </summary>
    public static class MapTexturePool {

        public class MapTexture : IComparable<MapTexture> {
            public string name;
            public Sprite sprite;
            public Texture2D texture;
            /// <summary>
            /// 最后访问时间
            /// </summary>
            public long LastAccessTime;

            /// <summary>
            /// 访问时间排序
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(MapTexture other) {
                if (LastAccessTime != other.LastAccessTime) {
                    return LastAccessTime < other.LastAccessTime ? 1 : -1;
                }
                return 0;
            }
        }

        public static string MapTileCachePath {
            get {
                return Path.Combine(Application.persistentDataPath, "MapTileCachePath");
            }
        }
        /// <summary>
        /// 最大缓存数量
        /// </summary>
        private const int MaxCachePool = 300;
        private static Dictionary<string, MapTexture> mMapTextureDict = new Dictionary<string, MapTexture>();
        public static  IEnumerator StartPool() {
            yield return null;
            while (true) {
                //每五秒检测一次
                yield return new WaitForSeconds(5f);
                try {
                    //开始检测Texture缓存池
                    if (MaxCachePool < mMapTextureDict.Count) {
                        //需要释放
                        List<MapTexture> mapTextures =new List<MapTexture>(mMapTextureDict.Values);
                        mapTextures.Sort();
                        for (int i = mapTextures.Count - 1; MaxCachePool < i; i--) {
                            Texture2D.Destroy(mapTextures[i].texture);
                            mMapTextureDict.Remove(mapTextures[i].name);
                        }
                    }
                } catch (Exception ex) {
                    Debug.LogError("缓存池异常：" +　ex.ToString());
                }
            }
        }

        public static MapTexture GetTexture(string name) {
            if (!mMapTextureDict.ContainsKey(name)) {
                return null;
            }
            MapTexture mapTexture = mMapTextureDict[name];
            mapTexture.LastAccessTime = DateTime.UtcNow.Ticks;
            return mapTexture;
        }

        public static MapTexture AddTexture(string name, byte[] textureBytes) {
            MapTexture mapTexture = new MapTexture() {
                name = name,
                LastAccessTime = DateTime.UtcNow.Ticks
            };
            mapTexture.texture = new Texture2D(256, 256);
            mapTexture.texture.LoadImage(textureBytes);
            mapTexture.sprite = Texture2D2Sprite(mapTexture.texture);
            if (!mMapTextureDict.ContainsKey(name)) {
                mMapTextureDict.Add(name, mapTexture);
            } else {
                mMapTextureDict[name] = mapTexture;
                Debug.LogError("重复AddTexture：" + name);
            }
            return mapTexture;
        }


        public static IEnumerator Request<T>(string url, Action<T> callback, int retryCount) where T : DownloadHandler , new() {
            using (UnityWebRequest www = UnityWebRequest.Get(url)) {
                www.downloadHandler = new T();
                WWWForm wWWForm = new WWWForm();
                yield return www.SendWebRequest();
                if (www.isHttpError || www.isNetworkError) {
                    if (0 < retryCount--) {
                        yield return Request(url, callback, retryCount);
                    } else{
                        callback?.Invoke(null);
                    }
                }else {
                    //成功
                    callback?.Invoke((T)www.downloadHandler);
                }
            }
        }

        public static IEnumerator RequestSprite(string url, Action<Sprite> callback, string fileName, int retryCount) {
            MapTexture mapTexture = GetTexture(fileName);
            if (null != mapTexture) {
                callback?.Invoke(mapTexture.sprite);
                yield return null;
                yield break;
            }
            //从文件或者网络获取
            string filePath = Path.Combine(MapTileCachePath, fileName);
            if (File.Exists(filePath)) {
                url = "file://" + filePath;
            }
            yield return Request<DownloadHandlerBuffer>(url, (down) => {
                if (null == down) {
                    //地图下载失败
                    Debug.LogError("地图下载失败：" + fileName);
                    return;
                }
                if (!File.Exists(filePath)) {
                    SaveTexture(filePath, down.data);
                } else {
                    //修改文件访问时间
                    FileInfo fileInfo = new FileInfo(filePath);
                    fileInfo.LastAccessTime = DateTime.UtcNow;
                }
                mapTexture = AddTexture(fileName, down.data);
                callback.Invoke(mapTexture.sprite);
            }, retryCount);
        }

        /// <summary>
        /// Texture转Sprite
        /// 10级地图上1像素是1个Unit单位
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static Sprite Texture2D2Sprite(Texture2D texture2D) {
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.filterMode = FilterMode.Bilinear;
            texture2D.Apply();
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1f);
        }

        /// <summary>
        ///  保存图片
        /// </summary>
        /// <param name="outPath"></param>
        /// <param name="texture2D"></param>
        public static void SaveTexture(string outPath, byte[] textureBytes) {
            string outDirectory = Directory.GetParent(outPath).FullName;
            if (!File.Exists(outDirectory)) {
                Directory.CreateDirectory(outDirectory);
            }
            File.WriteAllBytes(outPath, textureBytes);
        }
    }
}