using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Shujun.LBSMap
{
    public class MapHttpTools {
        public IEnumerator Request<T>(string url, Action<T> callback, int retryCount) where T : DownloadHandler , new() {
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
    }
}