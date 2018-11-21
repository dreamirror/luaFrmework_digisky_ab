using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LuaFramework;
namespace DigiSky.AssetBundleKit
{
    class AbMgr:Manager
    {
        /// <summary>
        /// 加载ab
        /// </summary>
        /// <param path="assetbudlePath">ab的路径</param>
        public void LoadAssetBundle(string path,string luaCall, params object[] args) {
            AssetBundleManager.GetSingel().LoadAssetAsyc("Assets/StreamingAssets/assetbundles/pic", "1", (UnityEngine.Object obj) => {
                LuaManager.CallFunction(luaCall, args);
            });
        }

        /// <summary>
        /// 加载asset
        /// </summary>
        /// <param assetName="assetName"></param>
        public void LoadAsset(string assetName)
        {

        }
        /// <summary>
        /// 加载asset
        /// </summary>
        /// <param path="assetbudlePath">ab的路径</param>
        /// <param assetName="assetName"></param>
        public void LoadAsset(string path,string assetName)
        {

        }

        /// <summary>
        /// 卸载AB
        /// </summary>
        /// <param path="assetbudlePath">ab的路径</param>
        /// <param assetName="assetName"></param>
        public void UnLoadAssetBundle(string path)
        {

        }

        /// <summary>
        /// 卸载AB
        /// </summary>
        /// <param ab="assetbudle">ab的路径</param>
        /// <param assetName="assetName"></param>
        public void UnLoadAssetBundle(AssetBundle ab)
        {
            ab.Unload(false); //非暴力卸载
        }
    }
}
