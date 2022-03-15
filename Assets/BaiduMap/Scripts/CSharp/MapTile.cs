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

        [SerializeField]
        private Sprite mDefaultSprite;

        [SerializeField]
        private TileData mTileData = null;

        private MapType mMapType = MapType.Street;

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
            mSpriteRender.sprite = mDefaultSprite;
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        public void Init(TileData tileData, MapType mapType) {
            gameObject.SetActive(true);
            if (mapType == mMapType && null != mTileData && tileData.Key == mTileData.Key) {
                if (mDefaultSprite != mSpriteRender.sprite) {
                    return;
                }
            }
            mSpriteRender.sprite = mDefaultSprite;
            mTileData = tileData;
            mMapType = mapType;
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
            MapType curMapType = mMapType;
            string fileName = string.Format("{0}_{1}.png", mTileData.Key, (int)curMapType);
            StartCoroutine(MapTexturePool.RequestSprite(mTileData.GetTileURL(curMapType), (sprite) => {
                mSpriteRender.sprite = sprite;
            }, fileName, 2));
        }
    }
}