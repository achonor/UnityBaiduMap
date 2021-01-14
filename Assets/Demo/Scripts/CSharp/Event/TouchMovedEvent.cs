using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShuJun.Event {
    public class TouchMovedEvent : IBaseEvent
    {
        public Vector2 TouchPoint = Vector2.zero;
    }
}
