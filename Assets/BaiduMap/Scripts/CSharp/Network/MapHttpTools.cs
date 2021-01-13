using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Achonor.LBSMap
{
    public static class MapHttpTools {
        public static IEnumerator Request<T>(string url, Action<T> callback, int retryCount) where T : DownloadHandler , new() {
            using (UnityWebRequest www = UnityWebRequest.Get(url)) {
                www.downloadHandler = new T();
                WWWForm wWWForm = new WWWForm();
                yield return www.SendWebRequest();
                if (www.isHttpError || www.isNetworkError) {
                    //没有提示
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

        public static IEnumerator RequestSprite(string url, Action<Sprite> callback, int retryCount) {
            yield return Request<DownloadHandlerTexture>(url, (down) => {
                callback.Invoke(MapUtils.Texture2D2Sprite(down.texture));
            }, retryCount);
        }
    }
}