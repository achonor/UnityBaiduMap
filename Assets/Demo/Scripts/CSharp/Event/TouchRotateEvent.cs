using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShuJun.Event {
    public class TouchRotateEvent : IBaseEvent
    {
        public Vector2 ChangedEuler = Vector2.zero;
    }
}
