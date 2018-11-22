using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LuaFramework;
namespace DigiSky.AssetBundleKit
{
    public class AbMgr
    {
        public string b = "";

        /// <summary>
        /// 加载ab
        /// </summary>
        /// <param bundlePath="assetbudlePath">ab的路径</param>
        /// <param assetName="assetName">加载的asset的路径</param>
        /// <param luaCall="lua call back">加载成功的回调</param>
        public static void LoadAssetBundle(string bundlePath,string assetName,string luaCall = "load_default_call", params object[] args) {
            Debug.Log("AbMgr LoadAssetBundle !!!!!");
            if (bundlePath == null) {
                Debug.LogError("ABmgr LoadAssetBundle bundlePath is null");
                return;
            }
            if (assetName == null) {
                Debug.LogError("ABmgr LoadAsssetBundle assetName is null");
                return;
            }
            if (luaCall == null) {
                Debug.LogWarning("ABmgr LoadAssetBundle luaCall is null");
            }
            AssetBundleManager.GetSingel().LoadAssetAsyc(bundlePath, assetName, (UnityEngine.Object obj) => {
                Debug.Log("AbMgr load call back !!!!!");
                Util.CallMethod("Network", luaCall, obj, args);
            });
        }

        /// <summary>
        /// 加载asset
        /// </summary>
        /// <param assetName="assetName"></param>
        public static void LoadAsset(string assetName,string luaCall = "load_asset_default_call", params object[] args)
        {
            if (assetName == null) {
                Debug.LogError("ABmgr loadAsset assetName is null");
                return;
            }

            AssetBundleManager.GetSingel().LoadAssetAsyc(bundlePath, assetName, (UnityEngine.Object obj) => {
                Debug.Log("AbMgr load call back !!!!!");
                Util.CallMethod("Network", luaCall, obj, args);
            });
        }
        /// <summary>
        /// 加载asset
        /// </summary>
        /// <param path="assetbudlePath">ab的路径</param>
        /// <param assetName="assetName"></param>
        public static void LoadAsset(string path,string assetName)
        {

        }

        /// <summary>
        /// 卸载AB
        /// </summary>
        /// <param path="assetbudlePath">ab的路径</param>
        /// <param assetName="assetName"></param>
        public static void UnLoadAssetBundle(string path)
        {

        }

        /// <summary>
        /// 卸载AB
        /// </summary>
        /// <param ab="assetbudle">ab的路径</param>
        /// <param assetName="assetName"></param>
        public static void UnLoadAssetBundle(AssetBundle ab)
        {
            ab.Unload(false); //非暴力卸载
        }
    }
}
