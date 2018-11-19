using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace DigiSky.AssetBundleKit
{

    public class BuildAssetBundleWindow : EditorWindow
    {
        /// <summary>
        /// 打出的包输出路径
        /// </summary>
        private string m_strAssetBundlesOutputPath = "Assets/StreamingAssets/AssetBundles";

        /// <summary>
        /// 要打包的文件目录,在Assets文件夹下
        /// </summary>
        private string m_strResToBundlePath = "/Resources/";

        /// <summary>
        /// 要打包的配置文件目录,
        /// </summary>
        private string m_strDataToBundleDir = "/Resources/Data/";

        /// <summary>
        /// scence文件目录
        /// </summary>
        private string m_strScenceFileDir = "/Scence/";

        BuildAssetBundleWindow()
        {
            this.titleContent = new GUIContent("Build AssetBundle Dialog");
            this.minSize = new UnityEngine.Vector2(400, 400);
            this.maxSize = new UnityEngine.Vector2(500, 500);
        }

        void OnEnable()
        {
            ReadConfigFile(GetConfigFilePath());
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();

            //绘制标题
            GUILayout.Space(10);
            GUI.skin.label.fontSize = 24;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Build AssetBundle Dialog");

            GUI.skin.label.fontSize = 10;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            //绘制文本
            GUILayout.Space(10);
            GUILayout.Label("输出路径，必填（工程文件夹下）", GUILayout.MaxWidth(800));
            m_strAssetBundlesOutputPath = EditorGUILayout.TextField("", m_strAssetBundlesOutputPath);

            //
            GUILayout.Space(10);
            GUILayout.Label("打包资源文件路径，必填（Assets文件夹下）", GUILayout.MaxWidth(800));
            m_strResToBundlePath = EditorGUILayout.TextField("", m_strResToBundlePath);

            GUILayout.Space(10);
            GUILayout.Label("场景文件所在路径，必填（Assets文件夹下）", GUILayout.MaxWidth(800));
            m_strScenceFileDir = EditorGUILayout.TextField("", m_strScenceFileDir);

            GUILayout.Space(10);
            GUILayout.Label("配置档文件所在路径，可选，单独给配置档打包使用（Assets文件夹下）", GUILayout.MaxWidth(800));
            m_strDataToBundleDir = EditorGUILayout.TextField("", m_strDataToBundleDir);


            GUILayout.Space(40);
            //添加按钮
            if (GUILayout.Button("Build (只打上次改变)"))
            {
                BuildAssetBundles();
            }
            if (GUILayout.Button("Rebuild (全部重新)"))
            {
                ReuildAssetBundles();
            }
            if (GUILayout.Button("Build Data (单独配置档)"))
            {
                BuildDataAssetBundles();
            }
            if (GUILayout.Button("Clear AssetBundle Name"))
            {
                ClearAssetBundleName();
            }
            if (GUILayout.Button("Include Custome Shaders"))
            {
                IncludeCustomeShaders();
            }


            GUILayout.EndVertical();
        }

        /// <summary>
        /// 应用当前数据
        /// </summary>
        private void Apply()
        {
            DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath = m_strAssetBundlesOutputPath;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath.StartsWith("/"))
                DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath = DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath.Remove(0, 1);
            if (DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath.EndsWith("/"))
                DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath = DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath.Remove(DigiSky.AssetBundleKit.BuildAssetBundle.AssetBundlesOutputPath.Length - 1, 1);

            DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir = m_strResToBundlePath;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir.StartsWith("/") == false)
                DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir = "/" + DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir.EndsWith("/") == false)
                DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir = DigiSky.AssetBundleKit.BuildAssetBundle.resToBundleDir + "/";

            DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir = m_strScenceFileDir;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir.StartsWith("/") == false)
                DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir = "/" + DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir.EndsWith("/") == false)
                DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir = DigiSky.AssetBundleKit.BuildAssetBundle.ScenceFileDir + "/";

            DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir = m_strDataToBundleDir;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir.StartsWith("/") == false)
                DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir = "/" + DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir;
            if (DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir.EndsWith("/") == false)
                DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir = DigiSky.AssetBundleKit.BuildAssetBundle.DataToBundleDir + "/";

            SaveConfigFile(GetConfigFilePath());
        }

        private bool CheckFileExsits()
        {
			// 检查输出目录最后一级是不是“AssetBundles”
			if (m_strAssetBundlesOutputPath != null
				&& (m_strAssetBundlesOutputPath.EndsWith("assetbundles") == false
                    && m_strAssetBundlesOutputPath.EndsWith("assetbundles/") == false)) 
			{
				if (m_strAssetBundlesOutputPath.EndsWith ("/"))
                    m_strAssetBundlesOutputPath = m_strAssetBundlesOutputPath + "assetbundles";
				else
                    m_strAssetBundlesOutputPath = m_strAssetBundlesOutputPath + "/assetbundles";

                EditorUtility.DisplayDialog("打包出错", "输出目录最后一级必须是“assetbundles”", "确定");
				return false;
			}
			
            if (m_strAssetBundlesOutputPath == null
                || Directory.Exists(m_strAssetBundlesOutputPath) == false)
            {
                Directory.CreateDirectory(m_strAssetBundlesOutputPath);
            }

            if (m_strResToBundlePath == null
                || Directory.Exists(Application.dataPath + m_strResToBundlePath) == false)
            {
                EditorUtility.DisplayDialog("打包出错", "打包资源文件路径不存在", "确定");
                return false;
            }
            if (m_strScenceFileDir == null
                || Directory.Exists(Application.dataPath + m_strScenceFileDir) == false)
            {
                EditorUtility.DisplayDialog("打包出错", "场景文件所在路径不存在", "确定");
                return false;
            }
            return true;
        }

        private void ReuildAssetBundles()
        {
			if (CheckFileExsits() == false)
                return;

			Apply();

            DigiSky.AssetBundleKit.BuildAssetBundle.BuildAssetBundles(true);
        }

        private void BuildAssetBundles()
        {
			if (CheckFileExsits() == false)
                return;

			Apply();

            DigiSky.AssetBundleKit.BuildAssetBundle.BuildAssetBundles();
        }

        private void BuildDataAssetBundles()
        {
			if (m_strDataToBundleDir == null
				|| Directory.Exists(Application.dataPath + m_strDataToBundleDir) == false)
			{
				EditorUtility.DisplayDialog("打包出错", "配置档文件所在路径不存在", "确定");
				return;
			}

			Apply();

            DigiSky.AssetBundleKit.BuildAssetBundle.BuildDataAssetBundles();
        }

        private void ClearAssetBundleName()
        {
			Apply();

            DigiSky.AssetBundleKit.BuildAssetBundle.ClearAllAssetBundleNameAndVariant();
        }

        private void IncludeCustomeShaders()
        {
			if (m_strResToBundlePath == null
				|| Directory.Exists(Application.dataPath + m_strResToBundlePath) == false)
			{
				EditorUtility.DisplayDialog("打包出错", "打包资源文件路径不存在", "确定");
				return;
			}

			Apply();

            DigiSky.AssetBundleKit.BuildAssetBundle.SetShaderSettings();
        }

        private string GetConfigFilePath()
        {
            string filePath = Application.dataPath + "/DigiSkyAssetBundle/Editor";
            string fileName = "AssetBundleEditorConfig.cfg";
            // src环境下，config文件在/DigiSkyAssetBundle/Editor目录下
            if (Directory.Exists(filePath))
            {
                return filePath + "/" + fileName;
            }
            else
            {
                // 查找Digisky.dll路径，config文件默认在同一目录下
				string[] filePaths = Directory.GetFiles(Application.dataPath, "DigiSkyAssetBundle-Editor.dll", SearchOption.AllDirectories);
                if (filePaths != null
                    && filePaths.Length == 1)
                {
					string dirPath = Path.GetDirectoryName (filePaths [0]);
					return dirPath + "/" + fileName;
                }
                else
                    Debug.LogError("can not find DigiSkyAssetBundle.dll");
            }

            return null;
        }

        private void ReadConfigFile(string path)
        {
            if (path == null
                || path.Length == 0)
            {
                Debug.LogError("invalid path!");
                return;
            }

            if (File.Exists(path) == false)
                return;

            StreamReader reader = new StreamReader(path);
            if (reader != null)
            {
                // 读一行
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    // 字段间用制表符分隔
                    string[] items = line.Split('\t');
                    if (items != null
                        && items.Length == 2)
                    {
                        if (items[0] == null
                            || items[1] == null)
                        {
                            Debug.LogError("Read " + path + " file error, format invalid!");
                            continue;
                        }

                        if (items[0].Equals("m_strAssetBundlesOutputPath"))
                        {
                            m_strAssetBundlesOutputPath = items[1];
                        }
                        else if (items[0].Equals("m_strDataToBundleDir"))
                        {
                            m_strDataToBundleDir = items[1];
                        }
                        else if (items[0].Equals("m_strResToBundlePath"))
                        {
                            m_strResToBundlePath = items[1];
                        }
                        else if (items[0].Equals("m_strScenceFileDir"))
                        {
                            m_strScenceFileDir = items[1];
                        }
                        else
                        {
                            Debug.LogError("Read " + path + " file error, format invalid!");
                        }
                    }
                    else
                        Debug.LogError("Read " + path + " file error, format invalid!");
                }

                reader.Close();
            }
        }

        private void SaveConfigFile(string path)
        {
            StreamWriter sw = new StreamWriter(path);
            if (sw != null)
            {
                StringBuilder sb = new StringBuilder();
                if (m_strAssetBundlesOutputPath != null)
                    sb.AppendFormat("{0}\t{1}\n", "m_strAssetBundlesOutputPath", m_strAssetBundlesOutputPath);
                if (m_strDataToBundleDir != null)
                    sb.AppendFormat("{0}\t{1}\n", "m_strDataToBundleDir", m_strDataToBundleDir);
                if (m_strResToBundlePath != null)
                    sb.AppendFormat("{0}\t{1}\n", "m_strResToBundlePath", m_strResToBundlePath);
                if (m_strScenceFileDir != null)
                    sb.AppendFormat("{0}\t{1}\n", "m_strScenceFileDir", m_strScenceFileDir);

                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();

				AssetDatabase.Refresh();
            }
        }
    }
}
