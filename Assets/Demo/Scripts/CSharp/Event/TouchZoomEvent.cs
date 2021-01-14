using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShuJun.Event {
    public class TouchZoomEvent : IBaseEvent
    {
        public float ChangeZoom = 0;
        public Vector2 ZoomCenterPoint = Vector2.zero;
    }
}
