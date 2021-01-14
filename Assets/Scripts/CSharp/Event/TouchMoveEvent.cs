using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShuJun.Event {
    public class TouchMoveEvent : IBaseEvent
    {
        public Vector2 MoveOffset;
    }
}
