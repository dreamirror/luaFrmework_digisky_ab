using UnityEngine;
using UnityEditor;

namespace DigiSky.AssetBundleKit
{
    public static class EditorMenu
    {
        [MenuItem("DigiSkyTool/BuildAssetBundle/Config Dialog")]
        static public void ConfigDialog()
        {
            EditorWindow.GetWindow(typeof(BuildAssetBundleWindow));
        }
    }
}
