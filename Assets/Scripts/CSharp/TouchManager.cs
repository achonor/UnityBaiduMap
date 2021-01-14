using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Achonor;
using Achonor.LBSMap;
using ShuJun.Event;

namespace ShuJun.Touch {
    public class TouchManager : MonoBehaviour
    {
        private Vector2 mLastMousePos;

        private bool mTouchMoving = false;

        private void Awake() {
            print(Input.touchSupported);
        }

        private void Update() {
            if (Input.touchCount <= 0 && !CheckPointInScreen(Input.mousePosition)) {
                return;
            }

            if (2 == Input.touchCount) {
                UnityEngine.Touch touch1 = Input.touches[0];
                UnityEngine.Touch touch2 = Input.touches[1];
                if ((touch1.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Stationary)
                    && (touch2.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Stationary)) {
                    //计算距离
                    float oldDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                    float newDistance = Vector2.Distance(touch1.position, touch2.position);
                    if (!oldDistance.FloatIsEqual(newDistance)) {
                        EventManager.Dispatch(new TouchZoomEvent() {
                            ChangeZoom = (newDistance / oldDistance) - 1,
                            ZoomCenterPoint = (touch1.position + touch2.position) / 2
                        });
                    }
                }
            }
            else if (GetControl() && Input.GetAxis("Mouse ScrollWheel") < 0) {
                if (CheckPointInScreen(Input.mousePosition)) {
                    //滚轮缩小
                    EventManager.Dispatch(new TouchZoomEvent() {
                        ChangeZoom = -0.1f,
                        ZoomCenterPoint = Input.mousePosition
                    });
                }

            }
            else if (GetControl() && 0 < Input.GetAxis("Mouse ScrollWheel")) {
                if (CheckPointInScreen(Input.mousePosition)) {
                    //滚轮放大
                    EventManager.Dispatch(new TouchZoomEvent() {
                        ChangeZoom = 0.1f,
                        ZoomCenterPoint = Input.mousePosition
                    });
                }
            }
            else if (Input.GetMouseButtonDown(0)) {
                //鼠标按下
                mLastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && !Input.mousePosition.Equals(mLastMousePos)) {
                // 鼠标长按，判断移动
                EventManager.Dispatch(new TouchMoveEvent() {
                    MoveOffset = (Vector2)Input.mousePosition - mLastMousePos
                });
                mTouchMoving = true;
                mLastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0)) {
                if (mTouchMoving) {
                    EventManager.Dispatch(new TouchMovedEvent() {
                        TouchPoint = Input.mousePosition
                    });
                    mTouchMoving = false;
                }
            }
        }

        private bool GetControl() {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        public bool CheckPointInScreen(Vector2 point) {
            return (0 <= point.x && point.x <= Screen.width
                    && 0 <= point.y && point.y <= Screen.height);
        }
    }
}