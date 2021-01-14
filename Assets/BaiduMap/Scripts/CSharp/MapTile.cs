using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Achonor.LBSMap {
    /// <summary>
    /// 地图瓦片
    /// </summary>
    public class MapTile : MonoBehaviour {

        [SerializeField]
        private SpriteRenderer mSpriteRender;

        private TileData mTileData = null;

        public string Key {
            get {
                return mTileData.Key;
            }
        }

        public TileData Data {
            get {
                return mTileData;
            }
        }

        private void Awake() {
            if (null == mSpriteRender) {
                mSpriteRender = GetComponent<SpriteRenderer>();
            }
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        public void Init(TileData tileData) {
            gameObject.SetActive(true);
            if (null != mTileData && tileData.Key == mTileData.Key) {
                if (null != mSpriteRender.sprite) {
                    return;
                }
            }
            mSpriteRender.sprite = null;
            mTileData = tileData;
            StopAllCoroutines();
            //缩放比例
            transform.localScale = mTileData.GetTileScale() * Vector3.one;
            //开始下载
            DownloadSprite();
        }
        private void DownloadSprite() {
            if (null == mTileData) {
                print("TileData没有赋值");
                return;
            }
            //下载瓦片地图
            StartCoroutine(MapHttpTools.RequestSprite(mTileData.GetTileURL(), (sprite) => {
                mSpriteRender.sprite = sprite;
            }, mTileData.Key, 2));
        }
    }
}