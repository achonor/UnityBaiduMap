
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Achonor;

namespace ShuJun.Event {
    public class EventManager : MonoBehaviour
    {
        private class EventContainer {
            public readonly Component mBindGo;
            public readonly Action<IBaseEvent> mCallback;

            public EventContainer(Component _bindGo, Action<IBaseEvent> _callback) {
                mBindGo = _bindGo;
                mCallback = _callback;
            }

            public bool Dispatch(IBaseEvent param) {
                if (null == mBindGo) {
                    return false;
                }
                mCallback.Invoke(param);
                return true;
            }

        }

        private static Dictionary<Type, List<EventContainer>> mCallbackDict = new Dictionary<Type, List<EventContainer>>();

        public static void Register<T>(Action<T> callback, Component _bindGo) where T : IBaseEvent {
            EventContainer eventContainer = new EventContainer(_bindGo, (IBaseEvent param) => {
                callback.Invoke((T)param);
            });

            System.Type eventType = typeof(T);
            if (!mCallbackDict.ContainsKey(eventType)) {
                mCallbackDict.Add(eventType, new List<EventContainer>());
            }
            mCallbackDict[eventType].Add(eventContainer);
        }

        public static void Remove<T>(Component _bindGo) {
            System.Type eventType = typeof(T);
            if (null == _bindGo || !mCallbackDict.ContainsKey(eventType)) {
                return;
            }
            List<EventContainer> eventContainers = mCallbackDict[eventType];
            for (int i = eventContainers.Count - 1; 0 <= i; i--) {
                if (eventContainers[i].mBindGo == _bindGo) {
                    eventContainers.RemoveAt(i);
                }
            }
        }

        public static void Dispatch<T>(T _event) where T : IBaseEvent {
            print("激活事件 : " + typeof(T).Name);
            System.Type eventType = typeof(T);
            if (!mCallbackDict.ContainsKey(eventType)) {
                return;
            }
            List<EventContainer> eventContainers = mCallbackDict[eventType];
            for (int i = eventContainers.Count - 1; 0 <= i; i--) {
                bool result = eventContainers[i].Dispatch(_event);
                if (!result) {
                    eventContainers.RemoveAt(i);
                }
            }
        }
    }
}

