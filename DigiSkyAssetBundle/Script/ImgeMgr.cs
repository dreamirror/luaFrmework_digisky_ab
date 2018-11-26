using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LuaFramework;
namespace DigiSky.AssetBundleKit
{
    public class ImgeMgr
    {
        public static Texture2D loadImge(string path) {
            Debug.Log("ImgeMgr loadImge!!!!!");
            Texture2D texture = AssetBundleManager.GetSingel().LoadSingleAsset<Texture2D>(path);
            if (texture != null) {
                return texture;
            }
            return null;
        }
    }
}
