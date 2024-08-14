using OverhaulMod.Combat;
using OverhaulMod.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModComponentCache
    {
        private static readonly CacheForGetComponent<CameraModeController> s_cacheForCameraModeControllerComponent = new CacheForGetComponent<CameraModeController>();

        private static readonly CacheForGetComponent<RobotInventory> s_cacheForRobotInventoryComponent = new CacheForGetComponent<RobotInventory>();

        public static void ClearCache()
        {
            s_cacheForCameraModeControllerComponent.Clear();
            s_cacheForRobotInventoryComponent.Clear();
        }

        public static CameraModeController GetCameraModeController(Transform transform)
        {
            return s_cacheForCameraModeControllerComponent.GetScript(transform);
        }

        public static RobotInventory GetRobotInventory(Transform transform)
        {
            return s_cacheForRobotInventoryComponent.GetScript(transform);
        }

        protected class CacheForGetComponent<T>
        {
            private readonly Dictionary<Transform, T> m_transformToObject;

            public CacheForGetComponent()
            {
                m_transformToObject = new Dictionary<Transform, T>();
            }

            public T GetScript(Transform transform)
            {
                if (m_transformToObject.ContainsKey(transform))
                {
                    T component1 = m_transformToObject[transform];
                    if (component1 == null)
                    {
                        component1 = transform.GetComponent<T>();
                        m_transformToObject[transform] = component1;
                    }
                    return component1;
                }

                T component = transform.GetComponent<T>();
                m_transformToObject.Add(transform, component);
                return component;
            }

            public void Clear()
            {
                m_transformToObject.Clear();
            }
        }
    }
}
