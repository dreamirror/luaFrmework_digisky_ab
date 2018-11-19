using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DigiSky.AssetBundleKit
{
    /// <summary>
    /// 已经加载的包
    /// </summary>
    public class tagLoadedAssetBundle
    {
        /// <summary>
        /// 已经加载的包
        /// </summary>
        public AssetBundle assetBundle;

        /// <summary>
        /// 包引用数
        /// </summary>
        public int nReferencedCount;

        /// <summary>
        /// 引用的AB包名
        /// </summary>
        public string[] arrDependencies = null;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ab"></param>
        public void Init(AssetBundle ab)
        {
            assetBundle = ab;
            nReferencedCount = 1;
        }
    }

    /// <summary>
    /// 资源包管理器
    /// </summary>
    public class AssetBundleManager : DigiSky.AssetBundleKit.SingelBehaviour<AssetBundleManager>
    {
        /// <summary>
        /// 主manifest文件
        /// </summary>
        AssetBundleManifest m_assetBundleManifest = null;

        /// <summary>
        /// 已经加载的包
        /// </summary>
        Dictionary<string, tagLoadedAssetBundle> m_dictLoadedAssetBundles = null;

        /// <summary>
        /// 所有协程句柄
        /// </summary>
        List<Coroutine> m_listCoroutine;

        /// <summary>
        /// 下载的包所在路径
        /// </summary>
        private string m_strDownloadPath = null;

        public string itfDownloadPath
        {
            get
            {
                return m_strDownloadPath;
            }
            set
            {
                string strTemp = value;
                if (strTemp != null)
                {
					if (strTemp.EndsWith("/") == false)
                        strTemp = strTemp + "/";
                }

                m_strDownloadPath = value;
            }
        }

        /// <summary>
        /// 安装包内的包所在路径
        /// </summary>
        string m_strStreamingPath = null;
        public string itfStreamingPath
        {
            get
            {
                return m_strStreamingPath;
            }
            set
            {
                string strTemp = value;
                if (strTemp != null)
                {
                    if (strTemp.EndsWith("/") == false)
                        strTemp = strTemp + "/";
                }

                m_strStreamingPath = strTemp;
            }
        }

        /// <summary>
        /// 停止所有协程
        /// </summary>
        private void _StopAllCoroutine()
        {
            if (m_listCoroutine != null
                && m_listCoroutine.Count > 0)
            {
                //if (m_gameobject == null)
                //{
                //    Debug.LogError("stop all coroutine failed, GameMainObj is not exist!");
                //    return;
                //}

                // 遍历停止协程
                for (int i = 0; i < m_listCoroutine.Count; i++)
                {
                    if (m_listCoroutine[i] != null)
                    {
                        Debug.Log("stop coroutine suc: " + i + " " + m_listCoroutine[i].ToString());
                        StopCoroutine(m_listCoroutine[i]);
                    }
                    else
                        Debug.Log("stop coroutine null: " + i);
                }

                m_listCoroutine.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Awake()
        {
			base.Awake();

            m_dictLoadedAssetBundles = new Dictionary<string, tagLoadedAssetBundle>();

            m_listCoroutine = new List<Coroutine>();
        }

        /// <summary>
        /// 初始化manifest文件,优先查找download path，没有则查找streaming path
        /// </summary>
        /// <param name="strManifestName">文件名称</param>
        public void InitManifest()
        {
            // 加载并初始化Manifest包
            Debug.Log("InitManifest AssetBundles");
            m_assetBundleManifest = LoadSingleAsset<AssetBundleManifest>("Assets/StreamingAssets/assetbundles/", "assetbundles", "AssetBundleManifest");
        }

        //
        // 摘要:
        //     释放
        public void OnDestroy()
        {
            _StopAllCoroutine();
        }

        public void OnReturnToLogin()
        {
            _StopAllCoroutine();
        }

        #region 异步加载
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strAssetName"></param>
        /// <param name="finishLoadAssetCallback"></param>
        public void LoadAssetAsyc(string strPath, string strAssetName, System.Action<Object> finishLoadAssetCallback)
        {
            if (strPath != null
                && strAssetName != null
                && strAssetName.Length != 0)
            {
                // 转为小写，包名只能是小写
                string strABName = strPath.ToLower() + strAssetName.ToLower();

                //if (m_gameobject == null)
                //{
                //    Debug.LogError("GameMainObj not exist!");
                //    return;
                //}

                // 异步加载资源
                Debug.Log("Begin LoadAssetAsyc");
                Coroutine routine = StartCoroutine(_LoadAssetAsyc(strABName, strAssetName, finishLoadAssetCallback));
                m_listCoroutine.Add(routine);
            }
        }

        /// <summary>
        /// 异步加载资源，从cache中
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="finishLoadAssetCallback"></param>
        /// <returns></returns>
        private IEnumerator _LoadAssetAsyc(string strABName, string strAssetName, System.Action<Object> finishLoadAssetCallback)
        {
            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    yield break;
            //}

            // 先加载自己包
            Debug.Log("Begin _LoadAssetAsyc");
            Coroutine routine1 = StartCoroutine(_LoadAssetBundleAsyc(strABName));
            m_listCoroutine.Add(routine1);
            yield return routine1;

            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    yield break;
            //}

            // 再加载依赖包
            Coroutine routine2 = StartCoroutine(_LoadDependenciesAsyc(strABName));
            m_listCoroutine.Add(routine2);
            yield return routine2;

            // 最后从包中加载出资源
            Object ob = _LoadAssetInternal(strABName, strAssetName);

            // 并把之前加载的assetbundle都卸载掉
            UnloadAssetBundleWithDependencies(strABName);

            _StopAllCoroutine();

            // 回调
            if (finishLoadAssetCallback != null)
            {
                finishLoadAssetCallback(ob);
            }
        }

        /// <summary>
        /// 加载依赖
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        private IEnumerator _LoadDependenciesAsyc(string strABName)
        {
            //从manifest获取依赖包名称
            if (m_assetBundleManifest == null)
            {
                Debug.LogError("Manifest file has not been loaded");
                yield break;
            }

            string[] dependencies = m_assetBundleManifest.GetDirectDependencies(strABName);
            if (dependencies.Length == 0)
                yield break;

            //// 记录依赖
            //m_dictDependencies.Add(assetBundleName, dependencies);

            // 异步加载依赖包
            for (int i = 0; i < dependencies.Length; i++)
            {
                //if (m_gameobject == null)
                //{
                //    Debug.LogError("GameMainObj not exist!");
                //    yield break;
                //}

                Coroutine routine = StartCoroutine(_LoadAssetBundleAsyc(dependencies[i]));
                m_listCoroutine.Add(routine);
                yield return routine;
            }
        }

        /// <summary>
        /// 加载AssetBundle，先查找cache里面，没有就查找streamingAssetPath
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private IEnumerator _LoadAssetBundleAsyc(string strABName)
        {
            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    yield break;
            //}

            // 检察是否已经加载
            tagLoadedAssetBundle bundle = null;
            m_dictLoadedAssetBundles.TryGetValue(strABName, out bundle);
            if (bundle != null)
            {
                //已经加载后又再次加载，引用+1
                bundle.nReferencedCount++;
                Debug.Log("download " + strABName + " success, and already downloaded just add reference count!");

                yield break;
            }

            if (AssetBundleInfoManager.IsExits()
                && AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath) != null
                && AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath).IsCached(strABName) == true)
            {
                Coroutine routine = StartCoroutine(_LoadAssetBundleAsycFromCache(strABName));
                m_listCoroutine.Add(routine);
                yield return routine;
            }
            else
            {
                Coroutine routine = StartCoroutine(_LoadAssetBundleAsycFromStreaming(strABName));
                m_listCoroutine.Add(routine);
                yield return routine;
            }
        }

        /// <summary>
        /// 从cache中加载包
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private IEnumerator _LoadAssetBundleAsycFromCache(string strABName)
        {
            //检查manifest
            if (m_assetBundleManifest == null
                || AssetBundleInfoManager.IsExits()
                || AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath) == null
                || AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath).IsCached(strABName) == false)
            {
                Debug.LogError("LoadAssetBundleFromCache error!");
                yield break;
            }

            //加载包
            string path = itfDownloadPath + strABName;
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            if (request.isDone
                && request.assetBundle != null)
            {
                //包加载成功
                tagLoadedAssetBundle loadedBundle = new tagLoadedAssetBundle();
                loadedBundle.Init(request.assetBundle);
                m_dictLoadedAssetBundles.Add(strABName, loadedBundle);

                Debug.Log("load " + strABName + " bundle from cache succeed!");
            }
            else
                //包加载失败
                Debug.Log("load assetBundle:" + strABName + "from cache is invalid!");
        }

        /// <summary>
        /// 从streamingAssetPath中加载包
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private IEnumerator _LoadAssetBundleAsycFromStreaming(string strABName)
        {
            string path = itfStreamingPath + strABName;
            Debug.Log("streamingAssetsPath: " + path);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;

            if (request != null
                && request.assetBundle != null)
            {
                //包加载成功
                tagLoadedAssetBundle loadedBundle = new tagLoadedAssetBundle();
                loadedBundle.Init(request.assetBundle);
                m_dictLoadedAssetBundles.Add(strABName, loadedBundle);

                Debug.Log("load " + strABName + " bundle from streming succeed!");
            }
            else
                //包加载失败
                Debug.Log("load assetBundle:" + strABName + "from streaming is invalid!");
        }

        #endregion

        #region 同步加载

        /// <summary>
        /// 加载场景，注意，场景AB包的释放需要放在SceneManager.sceneLoaded加载完成回调中或者异步加载时isDone显示为true后
        /// </summary>
        /// <param name="strSceneName"></param>
        public void LoadScene(string strSceneName)
        {
            if (//m_gameobject != null
                strSceneName != null)
            {
                string assetBundleName = strSceneName.ToLower();

                _LoadAssetBundle(assetBundleName);

                UnityEngine.SceneManagement.SceneManager.LoadScene(strSceneName);
            }
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(strSceneName);
        }

        /// <summary>
        /// 异步加载场景,注意，场景AB包的释放需要放在SceneManager.sceneLoaded加载完成回调中或者异步加载时isDone显示为true后
        /// </summary>
        /// <param name="strSceneName"></param>
        /// <returns></returns>
        public AsyncOperation LoadSceneAsync(string strSceneName)
        {
            if (//m_gameobject != null
                strSceneName != null)
            {
                string assetBundleName = strSceneName.ToLower();

                _LoadAssetBundle(assetBundleName.ToLower());

                AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(strSceneName);

                return op;
            }
            else
                return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(strSceneName);
        }

        /// <summary>
        /// 加载shader
        /// </summary>
        /// <param name="strShaderName"></param>
        /// <returns></returns>
        public Shader LoadShader(string strShaderName)
        {
            if (strShaderName != null)
            {
                Shader shader = Shader.Find(strShaderName);
                if (shader != null)
                    return shader;

                string assetBundleName = "shader/src/" + strShaderName.ToLower();

                _LoadAssetBundle(assetBundleName);

                shader = _LoadMainAsset(assetBundleName) as Shader;

                if (shader == null)
                    Debug.LogError("Shader Find failed, shader name: " + strShaderName);

                return shader;
            }
            
            return null;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="assetName"></param>
        /// <param name="finishLoadAssetCallback"></param>
        public T LoadAsset<T>(string strPath, string assetName) where T : UnityEngine.Object
        {
            Debug.Log("path: " + strPath + assetName);
            if (strPath != null
                && assetName != null
                && assetName.Length != 0)
            {
                // 转为小写，包名只能是小写
                string strABName = strPath.ToLower() + assetName.ToLower();

                _LoadAssetBundle(strABName);

                // 最后从包中加载出资源
                Object ob = _LoadAssetInternal(strABName, assetName);

                if (ob != null)
                {
                    return ob as T;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 使用一个完整的路径加载资源，最后一个/后面是asset名字,整体是包名
        /// </summary>
        /// <param name="strCompeletePath"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadSingleAsset(string strCompeletePath)
        {
            if (strCompeletePath != null
                && strCompeletePath.Length > 0)
            {
                strCompeletePath = strCompeletePath.ToLower();
                int index = strCompeletePath.LastIndexOf('/');

                string ABName = strCompeletePath.Substring(0, index + 1);
                string assetName = strCompeletePath.Substring(index + 1);

                return _LoadSingleAssetInternal(ABName, assetName);
            }

            return null;
        }

        /// <summary>
        /// 加载资源，如TextAsset等资源，加载后会立即释放AB包
        /// </summary>
        /// <typeparam name="T"></typeparam>
		/// <param name="strPath">AB包所在路径</param>
		/// <param name="strAssetName">AB包名称</param>
		/// <param name="strSpecialAssetName">一般及时其内部asset的名称，但某些特殊的不一样，目前只有Manifest文件和lua文件有特殊名字</param>
        /// <returns></returns>
        public T LoadSingleAsset<T>(string strPath, string strAssetName, string strSpecialAssetName = null) where T : UnityEngine.Object
        {
			UnityEngine.Object ob = _LoadSingleAssetInternal(strPath, strAssetName, strSpecialAssetName);

            if (ob != null)
            {
                return ob as T;
            }

            return default(T);
        }

        /// <summary>
        /// 加载资源逻辑
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="assetName"></param>
        /// <param name="specialAssetName"></param>
        /// <returns></returns>
        private UnityEngine.Object _LoadSingleAssetInternal(string strPath, string assetName, string specialAssetName = null)
        {
            // strPath不判断长度，允许为长度为0
            if (strPath != null
                && assetName != null
                && assetName.Length != 0)
            {
                // 转为小写，包名只能是小写
                string strABName = strPath.ToLower() + assetName.ToLower();

                // 某些包的assetname比较特殊，比如说Manifest文件
                string realAseetName = null;
                if (specialAssetName == null
                    || specialAssetName.Length == 0)
                    realAseetName = assetName;
                else
                    realAseetName = specialAssetName;

                AssetBundle assetBundle = null;
                if (AssetBundleInfoManager.IsExits() == true
                && AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath) != null
                && AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath).IsCached(strABName) == true)
                {
                    // 优先从cache加载
                    string cachePath = itfDownloadPath + strABName;
                    assetBundle = AssetBundle.LoadFromFile(cachePath);
                }
                else
                {
                    // cache没找到就从streaming加载
                    Debug.Log("streamingpath: " + itfStreamingPath + " " + strABName);
                    string streamingPath = itfStreamingPath + strABName;
                    assetBundle = AssetBundle.LoadFromFile(streamingPath);
                }

                if (assetBundle != null)
                {
                    // 最后从包中加载出资源
                    Object ob = assetBundle.LoadAsset(realAseetName);

                    //string[] names = assetBundle.GetAllAssetNames();

                    // 立即卸载
                    assetBundle.Unload(false);

                    return ob;
                }
            }

            return null;
        }

        /// <summary>
        /// 加载AB包中的主资源，即索引为0的资源
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private Object _LoadMainAsset(string strABName)
        {
            return _LoadAssetInternal(strABName, "", true);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="strABName"></param>
        /// <param name="strAssetName"></param>
        /// <param name="bMainAsset">主资源，包中的第一个资源</param>
        /// <returns></returns>
        private Object _LoadAssetInternal(string strABName, string strAssetName, bool bMainAsset = false)
        {
            if (strABName != null
                && strABName.Length != 0)
            {
                tagLoadedAssetBundle loadedBundle = null;
                if (m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedBundle))
                {
                    if (loadedBundle.assetBundle != null)
                    {
                        // 加载主资源
                        if (bMainAsset)
                        {
                            string[] names = loadedBundle.assetBundle.GetAllAssetNames();
                            if (names != null
                                && names[0] != null)
                            {
                                return loadedBundle.assetBundle.LoadAsset(names[0]);
                            }
                        }

                        return loadedBundle.assetBundle.LoadAsset(strAssetName);
                    }
                    else
                        Debug.LogError("the bundle of loaded asset bundle:" + strABName + " is null!");
                }
                else
                    Debug.LogError("the asset bundle:" + strABName + " has not been loaded!");
            }

            return null;
        }

        /// <summary>
        /// 加载依赖
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private void _LoadDependencies(string strABName)
        {
            //从manifest获取依赖包名称
            if (m_assetBundleManifest == null)
            {
                Debug.LogError("Manifest file has not been loaded");
				return;
            }

            string[] dependencies = m_assetBundleManifest.GetDirectDependencies(strABName);
            if (dependencies != null &&
                dependencies.Length == 0)
                return;

            // 记录依赖
            tagLoadedAssetBundle loadedAB = null;
            if (m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedAB))
            {
                loadedAB.arrDependencies = dependencies;
            }

            // 加载依赖包
            for (int i = 0; i < dependencies.Length; i++)
            {
                _LoadAssetBundle(dependencies[i]);
            }
        }

        /// <summary>
        /// 加载AssetBundle，先查找cache里面，没有就查找streamingAssetPath
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private void _LoadAssetBundle(string strABName)
        {
            // 检察是否已经加载
            tagLoadedAssetBundle loadedAB = null;
            m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedAB);
            if (loadedAB != null)
            {
                // 所有资源都要先加载依赖包
                _LoadDependencies(strABName);

                //已经加载后又再次加载，引用+1
                loadedAB.nReferencedCount++;
                Debug.Log("load " + strABName + " success, and already downloaded just add reference count!");
            }
            else
            {
                // 提前生成，后面再填充数据
                loadedAB = new tagLoadedAssetBundle();
                m_dictLoadedAssetBundles.Add(strABName, loadedAB);

                // 所有资源都要先加载依赖包
                _LoadDependencies(strABName);

                if (AssetBundleInfoManager.IsExits() == true
                && AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath) != null
                && AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath).IsCached(strABName) == true)
                {
                    _LoadAssetBundleFromCache(strABName);
                }
                else
                {
                    _LoadAssetBundleFromStreaming(strABName);
                }
            }
        }

        /// <summary>
        /// 从cache中加载包
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private void _LoadAssetBundleFromCache(string strABName)
        {
            //检查manifest
            if (m_assetBundleManifest == null
                || AssetBundleInfoManager.IsExits() == false
                || AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath) == null
                || AssetBundleInfoManager.GetSingel().GetBundleInfo(itfDownloadPath).IsCached(strABName) == false)
            {
                Debug.LogError("LoadAssetBundleFromCache error!");
                return;
            }

            //加载包
            string path = itfDownloadPath + strABName;
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);

            if (assetBundle != null)
            {
                //包加载成功
                tagLoadedAssetBundle loadedAB = null;
                if (m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedAB))
                {
                    loadedAB.assetBundle = assetBundle;
                    loadedAB.nReferencedCount = 1;
                }
                else
                    Debug.LogError("LoadAssetBundleFromCache error, tagLoadedAssetBundle is null!");

                Debug.Log("load " + strABName + " bundle from cache succeed!");
            }
            else
                //包加载失败
                Debug.Log("load assetBundle:" + strABName + " from cache fail!");
        }

        /// <summary>
        /// 从streamingAssetPath中加载包
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private void _LoadAssetBundleFromStreaming(string strABName)
        {
            string path = itfStreamingPath + strABName;

            Debug.Log("streamingAssetsPath: " + path);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);

            if (assetBundle != null)
            {
                //包加载成功
                tagLoadedAssetBundle loadedAB = null;
                if (m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedAB))
                {
                    loadedAB.assetBundle = assetBundle;
                    loadedAB.nReferencedCount = 1;
                }
                else
                    Debug.LogError("LoadAssetBundleFromStreaming error, tagLoadedAssetBundle is null!");

                Debug.Log("load " + strABName + " bundle from streming succeed!");
            }
            else
                //包加载失败
                Debug.Log("load assetBundle:" + strABName + "from streaming is invalid!");
        }

        #endregion

        /// <summary>
        /// 卸载包及其依赖包,只是操作引用计数
        /// </summary>
        /// <param name="strABName"></param>
        public void UnloadAssetBundleWithDependencies(string strABName, bool bUnloadAB = false)
        {
            _UnloadAssetBundleWithDependencies(strABName.ToLower(), bUnloadAB);
        }

        /// <summary>
        /// 卸载包及其依赖包
        /// </summary>
        /// <param name="strABName"></param>
        private void _UnloadAssetBundleWithDependencies(string strABName, bool bUnloadAB)
        {
            //卸载包
            _UnloadAssetBundleInternal(strABName, bUnloadAB);
        }

        /// <summary>
        /// 卸载依赖包
        /// </summary>
        /// <param name="strABName"></param>
        private void _UnloadDependencies(string strABName, bool bUnloadAB)
        {
            if (strABName == null
                || strABName.Length == 0)
            {
                Debug.LogError("assetBundleName cannot be null!");
                return;
            }

            // 获取依赖包名称
            tagLoadedAssetBundle loadedAB = null;
            if (m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedAB))
            {
                if (loadedAB.arrDependencies != null
                    && loadedAB.arrDependencies.Length > 0)
                {
                    for (int i = 0; i < loadedAB.arrDependencies.Length; i++)
                    {
                        _UnloadAssetBundleInternal(loadedAB.arrDependencies[i], bUnloadAB);
                    }
                }
            }
        }

        /// <summary>
        /// 卸载AB包，不会实际释放，只是操作引用计数
        /// </summary>
        /// <param name="strABName"></param>
        private void _UnloadAssetBundleInternal(string strABName, bool bUnloadAB)
        {
            if (strABName == null
                || strABName.Length == 0)
            {
                Debug.LogError("assetBundleName cannot be null!");
                return;
            }

            //卸载其依赖包
            _UnloadDependencies(strABName, bUnloadAB);

            //获得加载的包
            tagLoadedAssetBundle loadedBundle = null;
            if (m_dictLoadedAssetBundles.TryGetValue(strABName, out loadedBundle))
            {
                //引用计数减一
                loadedBundle.nReferencedCount--;

                if (bUnloadAB == true
                    && loadedBundle.nReferencedCount <= 0)
                {
                    if (loadedBundle.nReferencedCount < 0)
                        Debug.LogError("UnloadUnusedAssetBundles error, assetBundle name: " + loadedBundle.assetBundle.name + ", referenced count: " + loadedBundle.nReferencedCount);

                    _RealUnloadAB(loadedBundle);
                }
            }
            else
                Debug.LogError("the bundle " + strABName + " to unload is not exist!");
        }

        /// <summary>
        /// 移除已经不再使用的AB包
        /// </summary>
        public void UnloadUnusedAssetBundles()
        {
            if (m_dictLoadedAssetBundles != null
                && m_dictLoadedAssetBundles.Count > 0)
            {
                List<tagLoadedAssetBundle> temp = new List<tagLoadedAssetBundle>();
                Dictionary<string, tagLoadedAssetBundle>.Enumerator itor = m_dictLoadedAssetBundles.GetEnumerator();
                while (itor.MoveNext())
                {
                    tagLoadedAssetBundle loadedAB = itor.Current.Value;
                    if (loadedAB != null)
                    {
                        if (loadedAB.nReferencedCount < 0)
                            Debug.LogError("UnloadUnusedAssetBundles error, assetBundle name: " + loadedAB.assetBundle.name + ", referenced count: " + loadedAB.nReferencedCount);

                        if (loadedAB.nReferencedCount <= 0)
                        {
                            temp.Add(loadedAB);
                        }
                    }
                }

                // 卸载
                for (int i = 0; i < temp.Count; i++)
                {
                    _RealUnloadAB(temp[i]);
                }
            }
        }

        /// <summary>
        /// 卸载AB
        /// </summary>
        /// <param name="loadedAB"></param>
        private void _RealUnloadAB(tagLoadedAssetBundle loadedAB)
        {
            if (m_dictLoadedAssetBundles != null
                && m_dictLoadedAssetBundles.ContainsKey(loadedAB.assetBundle.name)
                && loadedAB.assetBundle != null)
            {
                string abName = loadedAB.assetBundle.name;
                loadedAB.assetBundle.Unload(false);
                m_dictLoadedAssetBundles.Remove(abName);
                Debug.Log("Unload " + abName + " success!");
            }
        }
    }
}
