using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiSky.AssetBundleKit;
using UnityEngine.UI;
public class Main : MonoBehaviour {
    public GameObject myGgameObj;     //先创建个Sprite  设置上图片 
    public Image image;            // Use this for initialization

    void Start () {


	}
    public System.Action<Object> call;

    public void Call_back(Object obj) {
        Debug.Log("call back");
        //Object pic = obj.LoadAsset("1.jpg");
        /* var texture = obj as Texture2D;
         Sprite sp = Sprite.Create(texture, new Rect(0, 0, 2048, 1024), Vector2.zero);
         if (sp != null) {
             Debug.Log("******************");
         }
         image.sprite = sp;*/
        
        TextAsset text = obj as TextAsset;

        string content = text.text;
        Debug.Log(content);
    }

    public void update_call(bool success) {
        if (success)
        {
            Debug.Log("update success!!!");
        }
        else {
            Debug.LogWarning("update failed!!!");
        }
    }

    private void Awake()
    {
        // 创建GameObject对象                 
      
        GameObject gameObj = GameObject.Find("Image");
        image = gameObj.GetComponent<Image>();
        if (image != null) {
            Debug.Log("@@@@@@@@@@@@@");
        }
        System.Action<Object> calls = Call_back;
        AssetBundleManager.Initialization();
        AssetBundleManager mgr = AssetBundleManager.GetSingel();
        mgr.itfStreamingPath = Application.streamingAssetsPath + "/AssetBundles/";
        mgr.itfDownloadPath = Application.persistentDataPath + "/";
        mgr.InitManifest();
        /*  
        mgr.LoadAssetAsyc("Assets/StreamingAssets/assetbundles/", "bundleinfo", calls);*/
        //UnityEngine.Object pic = mgr._LoadSingleAssetInternal("Assets/StreamingAssets/assetbundles/pic/","2.jpg");


        //AB下载测试

        AssetBundleInfoManager.Initialization();
        AssetBundleInfoManager info = AssetBundleInfoManager.GetSingel();
        AssetBundleDownloader.Initialization();
        AssetBundleDownloader downloader = AssetBundleDownloader.GetSingel();
        downloader.SetSourceAssetBundleURL("http://192.168.101.57/F%3A/filesserver/assetbundles/");
        System.Action<bool> call = update_call;
        downloader.CheckUpdate(call);
    }



    // Update is called once per frame
    void Update () {
		
	}
}
