using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LuaFramework;
using UnityEngine.SceneManagement;
using System.Collections;
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

        /// <summary>
        /// 加载场景，注意，场景AB包的释放需要放在SceneManager.sceneLoaded加载完成回调中或者异步加载时isDone显示为true后
        /// </summary>
        /// <param name="strSceneName"></param>
        public static void LoadScene(string strSceneName)
        {
            AssetBundleManager.GetSingel().LoadScene (strSceneName);
        }


        /// <summary>
        /// 异步加载场景回调lua函数
        /// </summary>
        /// <param name="strSceneName"></param>
        /// <param call_back="callBack"></param>
        public static void LoadSceneAsyncCall(string sceneName,string callBack) {
            Debug.Log("LoadSceneAsyncCall !!");
            AssetBundleManager.GetSingel().LoadSceneAsyncCall(sceneName, callBack);
        }





    }
}
