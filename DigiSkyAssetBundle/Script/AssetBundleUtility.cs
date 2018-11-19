
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DigiSky.AssetBundleKit
{
    /// <summary>
    /// 单件MonoBehaviour模板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingelBehaviour<T> : MonoBehaviour where T : class, new()
    {
        /// <summary>
        /// 单件对象
        /// </summary>
        protected static T Instance = null;

        /// <summary>
        /// 这里直接处理了Awake
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance == null)
                Instance = this as T;
        }

        /// <summary>
        /// 外部生成GameObject并AddComponent组件 //持久化这个对象
        /// </summary>
        public static void Initialization()
        {
            if (Instance == null)
            {
                GameObject objTemp = new GameObject();
                if (objTemp != null)
                {
                    Type type = typeof(T);
                    objTemp.name = type.ToString();
                    objTemp.AddComponent(type);
                    DontDestroyOnLoad(objTemp);
                }
            }
        }

        /// <summary>
        /// 取得单件实例
        /// </summary>
        /// <returns></returns>
        public static T GetSingel()
        {
            return Instance;
        }

        /// <summary>
        /// 单件是否存在
        /// </summary>
        /// <returns></returns>
        public static bool IsExits()
        {
            return Instance != null;
        }

        /// <summary>
        /// 静态释放
        /// </summary>
        public static void Destroy()
        {
            if (Instance != null)
            {
                MonoBehaviour objBehaviour = Instance as MonoBehaviour;
                if (objBehaviour != null
                    && objBehaviour.gameObject != null)
                    GameObject.Destroy(objBehaviour.gameObject);

                Instance = null;
            }
        }
    }
}