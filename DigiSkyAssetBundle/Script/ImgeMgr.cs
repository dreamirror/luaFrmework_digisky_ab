using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LuaFramework;
namespace DigiSky.AssetBundleKit
{
    class ImgeMgr
    {
        public static Texture2D loadImge(string path) {
            AssetBundleManager.GetSingel().LoadAssetAsyc(path, (UnityEngine.Object obj) => {
                Debug.Log("AbMgr load call back !!!!!");
            });
            return null;
        }
    }
}
