using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace DigiSky.AssetBundleKit
{

    public class BuildAssetBundle
    {
        /// <summary>
        /// 打出的包输出路径
        /// </summary>
        public static string AssetBundlesOutputPath = "Assets/StreamingAssets/AssetBundles";

        /// <summary>
        /// 要打包的文件目录,在Assets文件夹下
        /// </summary>
        public static string resToBundleDir = "/Resources/";

        /// <summary>
        /// 要打包的配置文件目录,
        /// </summary>
        public static string DataToBundleDir = "/Resources/Data/";

        /// <summary>
        /// scence文件目录
        /// </summary>
        public static string ScenceFileDir = "/LuaFramework/Scenes/";

        /// <summary>
        /// 版本信息文件名称
        /// </summary>
        public static string bundleInfoFileName = "BundleInfo.txt";


        ///<summary>
        ///ab路径以及对应包名，如果没有包名就是最小粒度
        ///</summary>>
        public static Dictionary<string, string> abInfo = new Dictionary<string, string> {
            { Application.dataPath + "/ResourcesCopy/"+"logic","logic"},
            { Application.dataPath + "/ResourcesCopy/"+"pic","pic"},
            { Application.dataPath + "/ResourcesCopy/"+"bundleInfo",""},

        };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string _GetAssetBundlesDir()
        {
            //去掉Application.dataPath最后的“Assets”
            string temp = Application.dataPath;
            temp = temp.Substring(0, temp.Length - 6);
			return temp + AssetBundlesOutputPath + "/";
        }

        /// <summary>
        /// 所有资源打包
        /// </summary>
        /// <param name="bForceRebuild">强制全部重新打包</param>
        public static void BuildAssetBundles(bool bForceRebuild = false)
        {
            // 清除之前生成的ab包
            if (bForceRebuild == true)
                _DeleteAssetBundles();

            // shader打包相关操作
            // 所有自建shader的包名使用shader名称，不使用shader文件名称
            // 所有使用到的内置shader包含进alwaysIncludedShaders里面
            SetShaderSettings();

            // 重新设置lightmap mode为manual
            _SetLightMapModeManual();

            //设置所有资源包名
            Debug.Log("111111111111111");
            Debug.Log(Application.dataPath + resToBundleDir);
            _SetBundleNameAndVariantByDir(Application.dataPath + ScenceFileDir);
            foreach (KeyValuePair<string, string> kvp in abInfo) {
                Debug.Log(kvp.Key);
                Debug.Log(kvp.Value);
                _SetConfigBundleNameAndVariantByDir(kvp.Key,kvp.Value);
            }

            // 单独设置scence文件包名
            string scencePath = Application.dataPath + ScenceFileDir;
            _SetBundleNameAndVariantByDir(scencePath, false, true);

            //打包
            _BuildAssetBundles();

            // 先生成包然偶后才能生成BundleInfo文件
            GenerateBundleInfoFile("", false);
            _SetAssetBundleNameAndVariant(Application.dataPath + resToBundleDir + bundleInfoFileName);

            //再次打包
            _BuildAssetBundles();

            //_DeleteBundleInfoFile();

            AssetDatabase.Refresh();

            Debug.Log("Build AssetBundle success!");
        }

        /// <summary>
        /// 只给配置档Data文件夹打包
        /// </summary>
        public static void BuildDataAssetBundles()
        {
            //清空所有资源包名
            Debug.Log("2222222222222222222");
            _SetBundleNameAndVariantByDir(Application.dataPath + resToBundleDir, true);

            //重新设置data数据包名
            string dataPath = Application.dataPath + DataToBundleDir;
            _SetCurDirAssetBundleNameAndVariant(dataPath);

            // 生成BundleInfo文件
            GenerateBundleInfoFile("data");
            _SetAssetBundleNameAndVariant(Application.dataPath + resToBundleDir + bundleInfoFileName);

            //创建包
            _BuildAssetBundles();

            //_DeleteBundleInfoFile();

            AssetDatabase.Refresh();

            Debug.Log("Build data AssetBundle success!");
        }

        /// <summary>
        /// 设置所有Resources文件夹下文件的包名
        /// </summary>
        public static void SetAllAssetBundleNameAndVariant()
        {
            Debug.Log("3333333333");
            _SetBundleNameAndVariantByDir(Application.dataPath + resToBundleDir);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 清空所有包名
        /// </summary>
        public static void ClearAllAssetBundleNameAndVariant()
        {
            Debug.Log("44444444444444444444");
            _SetBundleNameAndVariantByDir(Application.dataPath + resToBundleDir, true);

            AssetDatabase.Refresh();

            Debug.Log("Clear AssetBundle name success!");
        }

        /// <summary>
        /// 打包
        /// </summary>
        private static void _BuildAssetBundles()
        {
            //根据平台设置输出路径
			string outputPath = AssetBundlesOutputPath;
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// 设置所有ResourcesCopy文件夹下文件(不包括shader文件，shader文件有特殊的包名)的包名,setEmpty为true，设为空
        /// </summary>
        /// <param name="setEmpty"></param>
        private static void _SetBundleNameAndVariantByDir(string strDir, bool setEmpty = false, bool onlyAssetName = false)
        {
            if (Directory.Exists(strDir) == false)
            {
                Debug.LogError("strDir: " + strDir + "is not exsit!");
                return;
            }
            Debug.Log(strDir);
            Debug.Log("----------------");
            //显示进度条
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);

            //遍历所有子文件夹
            string[] dirPaths = Directory.GetDirectories(strDir, "*", SearchOption.AllDirectories);
            if (dirPaths != null
                && dirPaths.Length > 0)
            {
                for (int i = 0; i < dirPaths.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / dirPaths.Length);

                    _SetCurDirAssetBundleNameAndVariant(dirPaths[i], setEmpty, onlyAssetName);
                }
            }

            //自身文件夹
            _SetCurDirAssetBundleNameAndVariant(strDir, setEmpty, onlyAssetName);

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 设置单个文件夹下文件的assetBundleName和Variant，不包括其子文件夹
        /// //重写能够传入包名的版本
        /// </summary>

        private static void _SetConfigBundleNameAndVariantByDir(string strDir,string abName, bool setEmpty = false, bool onlyAssetName = false)
        {
            if (Directory.Exists(strDir) == false)
            {
                Debug.LogError("strDir: " + strDir + "is not exsit!");
                return;
            }

            //显示进度条
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);

            //遍历所有子文件夹
            string[] dirPaths = Directory.GetDirectories(strDir, "*", SearchOption.AllDirectories);
            Debug.Log("***************************");
            Debug.Log(strDir);
            Debug.Log(dirPaths.Length);
            Debug.Log(dirPaths.Length);

            if (dirPaths != null
                && dirPaths.Length > 0)
            {
                for (int i = 0; i < dirPaths.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / dirPaths.Length);

                    _SetCurDirConfigAssetBundleNameAndVariant(dirPaths[i],abName, setEmpty, onlyAssetName);
                }
            }

            //自身文件夹
            _SetCurDirConfigAssetBundleNameAndVariant(strDir,abName, setEmpty, onlyAssetName);

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 设置单个文件夹下文件的assetBundleName和Variant，不包括其子文件夹
        /// </summary>
        /// <param name="curDirPath"></param>
        /// /// <param name="onlyAssetName">scence文件特殊处理，包名不带路径，直接用scence名字</param>
        private static void _SetCurDirAssetBundleNameAndVariant(string curDirPath, bool setEmpty = false, bool onlyAssetName = false)
        {
            if (curDirPath == null
                || curDirPath.Length == 0)
            {
                Debug.LogError("path cannot be null or empty!");
                return;
            }

            //遍历所有文件

            string[] filePaths = Directory.GetFiles(curDirPath, "*", SearchOption.TopDirectoryOnly);
            if (filePaths != null
                && filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    _SetAssetBundleNameAndVariant(filePaths[i], setEmpty, onlyAssetName);
                }
            }
        }

        /// <summary>
        /// 设置单个文件夹下文件的assetBundleName和Variant，名字为传入的名字
        /// </summary>
        /// <param name="curDirPath"></param>
        /// /// <param name="onlyAssetName">scence文件特殊处理，包名不带路径，直接用scence名字</param>
        private static void _SetCurDirConfigAssetBundleNameAndVariant(string curDirPath,string abName, bool setEmpty = false, bool onlyAssetName = false)
        {
            if (curDirPath == null
                || curDirPath.Length == 0)
            {
                Debug.LogError("path cannot be null or empty!");
                return;
            }

            //遍历所有文件

            string[] filePaths = Directory.GetFiles(curDirPath, "*", SearchOption.TopDirectoryOnly);
            if (filePaths != null
                && filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    _SetAssetConfigBundleNameAndVariant(filePaths[i],abName, setEmpty, onlyAssetName);
                }
            }
        }

        /// <summary>
        /// 设置单个文件的assetBundleName和Variant
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onlyAssetName">设置scence文件包名，为true的时候有特殊逻辑</param>
        /// <param name="specialABName">用户shader文件设置包名，不为空的时候表示设置shader文件包名，有特殊逻辑</param>
        private static void _SetAssetBundleNameAndVariant(string path, bool setEmpty = false, bool onlyAssetName = false, string specialABName = null)
        {
            if (path == null
                || path.Length == 0)
            {
                Debug.LogError("path cannot be null or empty!");
                return;
            }

            //忽略掉.meta和.cfg文件文件
            if (Path.GetExtension(path) == ".meta"
                || Path.GetExtension(path) == ".cfg")
            {
                return;
            }

            //不处理没有后缀名的文件
            if (Path.GetExtension(path).Equals("")
                && setEmpty == false)
            {
                // Debug.LogError("unity unreconize format: " + path);
                return;
            }

            // 暂时忽略掉lua文件
            //if (Path.GetExtension(path).Equals(".lua")
            //    && setEmpty == false)
            //{
            //    return;
            //}

            // 忽略掉cs文件
            if (Path.GetExtension(path).Equals(".cs")
                && setEmpty == false)
                return;

            // 忽略EditorOnly文件
            if (path.Contains("LightingData")
                && setEmpty == false)
            {
                return;
            }

            //GetAtPath需要的目录：Assets/xx/xxx.xx
            string tempPath = path.Substring(Application.dataPath.Length - 6).Replace("\\", "/");

            AssetImporter importer = AssetImporter.GetAtPath(tempPath);
            if (importer != null)
            {
                if (File.Exists(tempPath))
                {
                    if (setEmpty)
                    {
                        if (importer.assetBundleName != null
                            && importer.assetBundleName.Length != 0)
                        {
                            importer.assetBundleName = "";
                        }
                        if (importer.assetBundleVariant != null
                            && importer.assetBundleVariant.Length != 0)
                        {
                            importer.assetBundleVariant = "";
                        }
                    }
                    else
                    {
                        string assetBundleName = null;
                        if (specialABName != null)
                        {
                            // 设置shader文件包名
                            assetBundleName = specialABName;
                        }
                        else
                        {
                            // 忽略掉shader文件的包名设置
                            if (tempPath.EndsWith(".shader"))
                            {
                                return;
                            }

                            //去掉后缀名
                            string extName = Path.GetExtension(tempPath);
                            tempPath = tempPath.Substring(0, tempPath.Length - extName.Length);

                            //去掉Assets和来源目录
                            assetBundleName = tempPath.Replace("Assets" + resToBundleDir, "");
                            if (onlyAssetName)
                            {
                                int pos = assetBundleName.LastIndexOf('/');
                                if (pos != -1)
                                    assetBundleName = assetBundleName.Substring(assetBundleName.LastIndexOf('/') + 1);
                            }
                        }

                        if (assetBundleName != null
                            && assetBundleName.Length != 0)
                        {
                            importer.assetBundleName = assetBundleName;
                            importer.assetBundleVariant = "";
                            Debug.Log("set bundle name success: " + assetBundleName);
                        }
                        else
                            Debug.LogError("set bundle name failed: " + path);
                    }
                }
                else
                    Debug.LogError("file: " + tempPath + " not exist!");
            }
            else
                Debug.LogError("file: " + tempPath + " cannot get AssetImporter!");
        }

        /// <summary>
        /// 设置单个文件的assetBundleName和Variant,名字为传入的名字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onlyAssetName">设置scence文件包名，为true的时候有特殊逻辑</param>
        /// <param name="specialABName">用户shader文件设置包名，不为空的时候表示设置shader文件包名，有特殊逻辑</param>
        private static void _SetAssetConfigBundleNameAndVariant(string path,string abName, bool setEmpty = false, bool onlyAssetName = false, string specialABName = null)
        {
            if (path == null
                || path.Length == 0)
            {
                Debug.LogError("path cannot be null or empty!");
                return;
            }

            //忽略掉.meta和.cfg文件文件
            if (Path.GetExtension(path) == ".meta"
                || Path.GetExtension(path) == ".cfg")
            {
                return;
            }

            //不处理没有后缀名的文件
            if (Path.GetExtension(path).Equals("")
                && setEmpty == false)
            {
                // Debug.LogError("unity unreconize format: " + path);
                return;
            }

            // 暂时忽略掉lua文件
            //if (Path.GetExtension(path).Equals(".lua")
            //    && setEmpty == false)
            //{
            //    return;
            //}

            // 忽略掉cs文件
            if (Path.GetExtension(path).Equals(".cs")
                && setEmpty == false)
                return;

            // 忽略EditorOnly文件
            if (path.Contains("LightingData")
                && setEmpty == false)
            {
                return;
            }

            //GetAtPath需要的目录：Assets/xx/xxx.xx
            string tempPath = path.Substring(Application.dataPath.Length - 6).Replace("\\", "/");

            AssetImporter importer = AssetImporter.GetAtPath(tempPath);
            if (importer != null)
            {
                if (File.Exists(tempPath))
                {
                    if (setEmpty)
                    {
                        if (importer.assetBundleName != null
                            && importer.assetBundleName.Length != 0)
                        {
                            importer.assetBundleName = "";
                        }
                        if (importer.assetBundleVariant != null
                            && importer.assetBundleVariant.Length != 0)
                        {
                            importer.assetBundleVariant = "";
                        }
                    }
                    else
                    {
                        string assetBundleName = null;
                        if (specialABName != null)
                        {
                            // 设置shader文件包名
                            assetBundleName = specialABName;
                        }
                        else
                        {
                            // 忽略掉shader文件的包名设置
                            if (tempPath.EndsWith(".shader"))
                            {
                                return;
                            }

                            //去掉后缀名
                            string extName = Path.GetExtension(tempPath);
                            tempPath = tempPath.Substring(0, tempPath.Length - extName.Length);

                            //去掉Assets和来源目录
                            assetBundleName = tempPath.Replace("Assets" + resToBundleDir, "");
                            if (onlyAssetName)
                            {
                                int pos = assetBundleName.LastIndexOf('/');
                                if (pos != -1)
                                    assetBundleName = assetBundleName.Substring(assetBundleName.LastIndexOf('/') + 1);
                            }
                        }
                        if (abName != "") {
                            assetBundleName = abName;
                        }
                        if (assetBundleName != null
                            && assetBundleName.Length != 0)
                        {
                            importer.assetBundleName = assetBundleName;
                            importer.assetBundleVariant = "";
                            Debug.Log("set bundle name success: " + assetBundleName);
                        }
                        else
                            Debug.LogError("set bundle name failed: " + path);
                    }
                }
                else
                    Debug.LogError("file: " + tempPath + " not exist!");
            }
            else
                Debug.LogError("file: " + tempPath + " cannot get AssetImporter!");
        }

        /// <summary>
        /// 生成BundleInfo文件
        /// </summary>
        public static void GenerateBundleInfoFile(string rootFolder = "", bool bEmpty = false)
        {
            AssetDatabase.Refresh();

            _GenerateBundleInfoFile(rootFolder, bEmpty);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成BundleInfo文件
        /// </summary>
        /// <param name="path"></param>
        private static void _GenerateBundleInfoFile(string rootFolder, bool bEmpty = false)
        {
            StringBuilder sb = new StringBuilder();
            AssetDatabase.RemoveUnusedAssetBundleNames();
            if (bEmpty == false)
            {
                string[] names = AssetDatabase.GetAllAssetBundleNames();
                
                for (int i = 0; i < names.Length; i++)
                {
                    Debug.Log("a bundleinfo==" + names[i]);
                    if (names[i].StartsWith(rootFolder))
                    {
                        // 忽略掉bundleinfo文件
                        if (names[i].EndsWith("bundleinfo"))
                        {
                            continue;
                        }

                        string oneline = _WriteOneLine(names[i]);
                        if (oneline != null
                            && oneline.Length != 0)
                        {
                            sb.Append(oneline);
                        }
                    }
                }
            }

            // 保存文件
            _SaveFile(Application.dataPath + resToBundleDir, bundleInfoFileName, sb);

            Debug.Log("Generate BundleInfo File success!");
        }

        /// <summary>
        /// 写一行
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        private static string _WriteOneLine(string assetbundleName)
        {
            if (assetbundleName != null
                && assetbundleName.Length != 0)
            {
                string filePath = _GetAssetBundlesDir() + assetbundleName;
                if (File.Exists(filePath) == false)
                {
                    Debug.Log(filePath + " doesn't exists!");
                    return null;
                }

                // 获取crc和hash
                uint crc;
                bool getCrc = BuildPipeline.GetCRCForAssetBundle(filePath, out crc);
                Hash128 hashcode;
                bool getHash = BuildPipeline.GetHashForAssetBundle(filePath, out hashcode);
                if (getCrc == false
                    || getHash == false)
                {
                    Debug.LogWarning("get crc or hash code failed! path: " + _GetAssetBundlesDir() + assetbundleName);
                    return null;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}\t{1}\t{2}\n", assetbundleName, hashcode.ToString(), crc);
                return sb.ToString();
            }
            else
                Debug.LogError("parameter invalid, assetbundle name: " + assetbundleName);

            return null;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="sb"></param>
        private static void _SaveFile(string path, string fileName, StringBuilder sb)
        {
            if (path == null
                || path.Length == 0)
            {
                Debug.LogError("path cannot be null!");
                return;
            }

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            if (fileName == null
                || fileName.Length == 0
                || sb == null)
            {
                Debug.LogError("parameter invalid!");
                return;
            }

            var utf8WithoutBom = new System.Text.UTF8Encoding(false);
            string filePath = path + fileName;
            using (StreamWriter textWriter = new StreamWriter(filePath, false, utf8WithoutBom))
            {
                textWriter.Write(sb.ToString());
                textWriter.Flush();
                textWriter.Close();

                Debug.Log("Save file: " + filePath + " success!");
            }
        }

        private static void _DeleteBundleInfoFile()
        {
            File.Delete(Application.dataPath + "/Resources/BundleInfo.bytes");
        }

        private static void _DeleteAssetBundles()
        {
			if (Directory.Exists(AssetBundlesOutputPath) == true)
				Directory.Delete(AssetBundlesOutputPath, true);

			Directory.CreateDirectory(AssetBundlesOutputPath);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 设置shader相关，shader特殊打包设置
        /// </summary>
        public static void SetShaderSettings()
        {
            // 手动添加的一些要使用到的shader，将会被设置到alwaysIncludedShaders里面
            string[] originShaders = new string[0]{
            //"Nature/Terrain/Diffuse",
        };

            // 不会被设置到alwaysIncludedShaders里面得shader
            System.Collections.Generic.List<string> ignoreShaderList = new System.Collections.Generic.List<string>();
            ignoreShaderList.Add("Standard");
            ignoreShaderList.Add("Hidden/InternalErrorShader");

            // 遍历Application.dataPath/ResourcesCopy文件夹，查找所有的自建shader
            System.Collections.Generic.List<string> customShaderNameList = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<string> customShaderFilePathList = new System.Collections.Generic.List<string>();

            string[] filePaths = Directory.GetFiles(Application.dataPath + resToBundleDir, "*.shader", SearchOption.AllDirectories);
            if (filePaths != null
                && filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    string str = File.ReadAllText(filePaths[i]);

                    // 查找shader名字的字符串
                    System.Text.RegularExpressions.Regex rx = new System.Text.RegularExpressions.Regex("^( *)Shader( *)\"(.*?)\"",
                                    System.Text.RegularExpressions.RegexOptions.Compiled
                                    | System.Text.RegularExpressions.RegexOptions.Multiline);
                    System.Text.RegularExpressions.MatchCollection matches = rx.Matches(str);

                    if (matches.Count == 1)
                    {
                        string title = matches[0].Value;
                        // 查找shader名字的字符串
                        System.Text.RegularExpressions.Regex realNameRx = new System.Text.RegularExpressions.Regex("\"(.*?)\"",
                                        System.Text.RegularExpressions.RegexOptions.Compiled);
                        System.Text.RegularExpressions.MatchCollection realNamematches = realNameRx.Matches(title);
                        if (realNamematches.Count == 1)
                        {
                            // 去掉前后引号
                            string shaderName = realNamematches[0].Value;
                            shaderName = shaderName.Substring(1, shaderName.Length - 2);

                            // 检查重复shader
                            if (customShaderNameList.Contains(shaderName))
                            {
                                int index = customShaderNameList.FindIndex(s => s.Equals(shaderName));
                                string logStr = string.Format("有重复的shader name:{0} \n, path: {1}\npath: {2}", customShaderNameList[i], filePaths[i], customShaderFilePathList[index]);
                                Debug.LogError(logStr);
                                return;
                            }

                            customShaderNameList.Add(shaderName);
                            customShaderFilePathList.Add(filePaths[i]);
                            Debug.Log("shader name:" + shaderName + " path: " + filePaths[i]);
                        }
                        else
                            Debug.LogError("shader name analysis error! " + realNamematches.Count + filePaths[i]);
                    }
                    else
                    {
                        Debug.LogError("shader name analysis error! " + matches.Count + filePaths[i]);
                    }
                }
            }

            // 查找在工程中已经被material引用到的shader
            System.Collections.Generic.List<string> usedShaderNameList = new System.Collections.Generic.List<string>();
            string objPath = Application.dataPath + resToBundleDir;
            string[] directoryEntries;
            try
            {
                directoryEntries = System.IO.Directory.GetFiles(objPath, "*", SearchOption.AllDirectories);

                for (int i = 0; i < directoryEntries.Length; i++)
                {
                    string p = directoryEntries[i];
                    string[] tempPaths = p.Split(new string[] { "/Assets/" }, System.StringSplitOptions.None);
                    int length = tempPaths.Length;
                    if (length >= 1
                        && tempPaths[length - 1].EndsWith(".mat"))
                    {
                        Material mat = AssetDatabase.LoadAssetAtPath("Assets/" + tempPaths[length - 1], typeof(Material)) as Material;
                        // 检查是否使用了Standard shader,抛出错误
                        if (mat != null
                            && ignoreShaderList.Contains(mat.shader.name))
                        {
                            Debug.LogError("禁止使用" + mat.shader.name + " shader, Material path: " + "Assets/" + tempPaths[length - 1]);
                        }

                        if (mat != null
                            && usedShaderNameList.Contains(mat.shader.name) == false)
                            usedShaderNameList.Add(mat.shader.name);
                    }
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                Debug.Log("The path encapsulated in the " + objPath + "Directory object does not exist.");
            }

            // 找出使用到的内置shader，加入到AlwaysIncludedShaders
            System.Collections.Generic.List<string> buildInToIncludeShaders = new System.Collections.Generic.List<string>();
            for (int i = 0; i < usedShaderNameList.Count; i++)
            {
                if (customShaderNameList.Contains(usedShaderNameList[i]) == false
                    && ignoreShaderList.Contains(usedShaderNameList[i]) == false)
                {
                    buildInToIncludeShaders.Add(usedShaderNameList[i]);
                    Debug.Log("build in shader: " + usedShaderNameList[i]);
                }
            }

            // 将在工程中未被引用到的自建shader加入到AlwaysIncludedShaders
            //for (int i = 0; i < customShaderNameList.Count; i++)
            //{
            //    if (usedShaderNameList.Contains(customShaderNameList[i]) == false)
            //    {
            //        buildInToIncludeShaders.Add(customShaderNameList[i]);
            //    }
            //}

            // 手动添加的shader加入到AlwaysIncludedShaders
            for (int i = 0; i < originShaders.Length; i++)
            {
                if (buildInToIncludeShaders.Contains(originShaders[i]) == false
                    && ignoreShaderList.Contains(originShaders[i]) == false)
                {
                    buildInToIncludeShaders.Add(originShaders[i]);
                    Debug.Log("orgin shader: " + originShaders[i]);
                }
            }

            // 过滤掉重复的shader
            for (int i = 0; i < buildInToIncludeShaders.Count; i++)
            {
                for (int j = i + 1; j < buildInToIncludeShaders.Count; )
                {
                    if (buildInToIncludeShaders[i].Equals(buildInToIncludeShaders[j]))
                    {
                        buildInToIncludeShaders.RemoveAt(j);
                    }
                    else
                        j++;
                }
            }

            // 设置AlwaysIncludedShaders
            SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            SerializedProperty it = graphicsSettings.GetIterator();
            SerializedProperty dataPoint;
            while (it.NextVisible(true))
            {
                if (it.name == "m_AlwaysIncludedShaders")
                {
                    it.ClearArray();

                    for (int i = 0; i < buildInToIncludeShaders.Count; i++)
                    {
                        if (ignoreShaderList.Contains(buildInToIncludeShaders[i]) == false)
                        {
                            it.InsertArrayElementAtIndex(i);
                            dataPoint = it.GetArrayElementAtIndex(i);
                            dataPoint.objectReferenceValue = Shader.Find(buildInToIncludeShaders[i]);
                        }
                    }

                    graphicsSettings.ApplyModifiedProperties();
                }
            }

            // 设置自建shader文件包名
            for (int i = 0; i < customShaderNameList.Count; i++)
            {
                if (customShaderFilePathList.Count > i)
                {
                    _SetAssetBundleNameAndVariant(customShaderFilePathList[i], false, false, "shader/src/" + customShaderNameList[i]);
                }
            }

            Debug.Log("set shader settings success!");
        }

        /// <summary>
        /// 设置工程graphics setting lightmap mode 为 manual
        /// </summary>
        private static void _SetLightMapModeManual()
        {
            // 设置AlwaysIncludedShaders
            SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            SerializedProperty it = graphicsSettings.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name == "m_LightmapStripping")
                {
                    it.enumValueIndex = 1;  // enum{ Automic = 0, Manual = 1 };

                }
                else if (it.name == "m_LightmapKeepPlain"
                        || it.name == "m_LightmapKeepDirCombined"
                        || it.name == "m_LightmapKeepDirSeparate")
                {
                    it.boolValue = true;
                }
                else if (it.name == "m_LightmapKeepDynamicPlain"
                        || it.name == "m_LightmapKeepDynamicDirCombined"
                        || it.name == "m_LightmapKeepDynamicDirSeparate")
                {
                    it.boolValue = false;
                }
            }

            graphicsSettings.ApplyModifiedProperties();
        }
    }
}
