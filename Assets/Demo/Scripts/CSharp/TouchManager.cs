using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShuJun.Event;
using Achonor;

namespace ShuJun.Touch {
    public class TouchManager : MonoBehaviour
    {
        private Vector2 mLastMousePos;

        private bool mTouchMoving = false;
        private bool mTouchZooming = false;
        private bool mTouchRotateing = false;


        private void Update() {
            if (Input.touchCount <= 0 && !CheckPointInScreen(Input.mousePosition)) {
                return;
            }
            if (!mTouchZooming && Input.GetMouseButtonUp(0)) {
                if (mTouchMoving) {
                    mTouchMoving = false;
                    EventManager.Dispatch(new TouchMovedEvent() {
                        TouchPoint = Input.mousePosition
                    });
                }
                if (mTouchRotateing) {
                    mTouchRotateing = false;
                    EventManager.Dispatch(new TouchRotatedEvent());
                }
            }
            if ((Input.touchSupported && Input.touchCount <= 0)) {
                mTouchZooming = false;
                return;
            }
            if (2 == Input.touchCount) {
                mTouchZooming = true;
                UnityEngine.Touch touch1 = Input.touches[0];
                UnityEngine.Touch touch2 = Input.touches[1];
                if ((touch1.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Stationary)
                    && (touch2.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Stationary)) {
                    //计算距离
                    float oldDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                    float newDistance = Vector2.Distance(touch1.position, touch2.position);
                    if (1e-8 < Mathf.Abs(oldDistance -newDistance)) {
                        EventManager.Dispatch(new TouchZoomEvent() {
                            ChangeZoom = (newDistance / oldDistance) - 1,
                            ZoomCenterPoint = (touch1.position + touch2.position) / 2
                        });
                    }
                }
            } else if (GetControl() && Input.GetAxis("Mouse ScrollWheel") < 0) {
                if (CheckPointInScreen(Input.mousePosition)) {
                    //滚轮缩小
                    EventManager.Dispatch(new TouchZoomEvent() {
                        ChangeZoom = -0.1f,
                        ZoomCenterPoint = Input.mousePosition
                    });
                }

            } else if (GetControl() && 0 < Input.GetAxis("Mouse ScrollWheel")) {
                if (CheckPointInScreen(Input.mousePosition)) {
                    //滚轮放大
                    EventManager.Dispatch(new TouchZoomEvent() {
                        ChangeZoom = 0.1f,
                        ZoomCenterPoint = Input.mousePosition
                    });
                }
            } else if (!mTouchZooming && GetAlt() && Input.GetMouseButton(0)
                && !mLastMousePos.Equals(Input.mousePosition)) {
                mTouchRotateing = true;
                //旋转地图
                Vector2 distance = ((Vector2)Input.mousePosition - mLastMousePos) * (540f / Screen.height) * 0.6f;
                EventManager.Dispatch(new TouchRotateEvent() {
                    ChangedEuler = new Vector2(-distance.y, distance.x)
                });
            } else if (Input.GetMouseButtonDown(0)) {
                //鼠标按下

            } else if (!mTouchZooming && Input.GetMouseButton(0)
                && !mLastMousePos.Equals(Input.mousePosition)) {
                mTouchMoving = true;
                // 鼠标长按，判断移动
                EventManager.Dispatch(new TouchMoveEvent() {
                    MoveOffset = ((Vector2)Input.mousePosition - mLastMousePos) * (540f / Screen.height)
                });
            }
        }

        private void LateUpdate() {
            mLastMousePos = Input.mousePosition;
        }

        private bool GetControl() {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private bool GetAlt() {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        public bool CheckPointInScreen(Vector2 point) {
            return (0 <= point.x && point.x <= Screen.width
                    && 0 <= point.y && point.y <= Screen.height);
        }
    }
}