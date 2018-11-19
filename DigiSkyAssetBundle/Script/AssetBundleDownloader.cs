using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DigiSky.AssetBundleKit
{
    public class AssetBundleDownloader : DigiSky.AssetBundleKit.SingelBehaviour<AssetBundleDownloader>
    {
        protected override void Awake()
        {
			base.Awake();

            m_listCoroutine = new List<Coroutine>();
        }

        /// <summary>
        /// 资源服务器URL地址
        /// </summary>
        string m_strBaseDownloadingURL = "";

        /// <summary>
        /// 资源服务器URL接口
        /// </summary>
        public string itfBaseDownloadingURL
        {
            get
            {
                return m_strBaseDownloadingURL;
            }
            set
            {
                m_strBaseDownloadingURL = value;
            }
        }

        /// <summary>
        /// 记录版本信息的文件名
        /// </summary>
        static readonly string m_BundleInfoFileName = "BundleInfo";

        // 资源服务器上的bundleinfo文件
        private tagBundleInfo m_serverBundleInfo = null;

        /// <summary>
        /// 更新完毕回调,更新完成后通知当前是否有更新
        /// </summary>
        System.Action<bool> m_finishUpdateCallback = null;

        /// <summary>
        /// 下载完毕回调
        /// </summary>
        System.Action m_finishDownloadCallback = null;

        /// <summary>
        /// 是否开启版本检查
        /// </summary>
        public static bool CheckNewVersion = true;

        /// <summary>
        /// 所有协程句柄
        /// </summary>
        List<Coroutine> m_listCoroutine;

        /// <summary>
        /// 设置资源服务器URL地址
        /// </summary>
        /// <param name="absolutePath"></param>
        public void SetSourceAssetBundleURL(string absolutePath)
        {
            itfBaseDownloadingURL = absolutePath;
        }

        #region version check
        /// <summary>
        /// 异步检查更新
        /// </summary>
        public void CheckUpdate(System.Action<bool> finishUpdateCallback = null)
        {
            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    return;
            //}

            m_finishUpdateCallback = finishUpdateCallback;

            Debug.Log("Begin CheckUpdate");
            Coroutine routine = StartCoroutine(_CheckUpdate());
            m_listCoroutine.Add(routine);
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <returns></returns>
        private IEnumerator _CheckUpdate()
        {
            // 不检查更新
            if (CheckNewVersion == false)
            {
                // 调用更新完成回调
                if (m_finishUpdateCallback != null)
                {
                    m_finishUpdateCallback(false);
                }

                yield break;
            }

            // 开始检查更新
            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    yield break;
            //}

            // 下载BundleInfo包,服务器上的bundleinfofile，用于对比本地文件下载新的包，包下载完毕以后才能将该bundleinfo文件保存到本地
            Coroutine routine = StartCoroutine(_DownloadAssetBundleInternal(m_BundleInfoFileName, true));
            m_listCoroutine.Add(routine);
            yield return routine;

            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    yield break;
            //}

            // 对比需要更新的bundle
            string[] toUpdateAssetBundleNames = _CompareBundleInfo();

            // 没有需要更新的资源,中断协程
            if (toUpdateAssetBundleNames == null
                || toUpdateAssetBundleNames.Length == 0)
            {
                Debug.Log("nothing need to be updated!");

                // 调用更新完成回调
                if (m_finishUpdateCallback != null)
                {
                    m_finishUpdateCallback(false);
                }
                yield break;
            }

            //if (m_gameobject == null)
            //{
            //    Debug.LogError("GameMainObj not exist!");
            //    yield break;
            //}
            // 开始更新
            Coroutine routine2 = StartCoroutine(_DownloadAssetBundlesAsyc(toUpdateAssetBundleNames));
            m_listCoroutine.Add(routine2);
            yield return routine2;

            // 保存版本信息
            _SaveFile(m_BundleInfoFileName, m_serverBundleInfo.itfBuffer);

            if (AssetBundleInfoManager.IsExits())
                AssetBundleInfoManager.GetSingel().ReloadBundleInfo(AssetBundleManager.Instance.itfDownloadPath);

            // 强制停止所有协程
            _StopAllCoroutine();

            // 调用更新完成回调
            if (m_finishUpdateCallback != null)
            {
                m_finishUpdateCallback(true);
            }
        }

        /// <summary>
        /// 对比版本文件
        /// </summary>
        /// <returns></returns>
        private string[] _CompareBundleInfo()
        {
            string[] toUpdateAssetBundleNames = null;

            if (AssetBundleInfoManager.IsExits() == false)
                return null;

            // 读取下载路径上的版本文件
            tagBundleInfo localBundleInfo = AssetBundleInfoManager.GetSingel().GetBundleInfo(AssetBundleManager.Instance.itfDownloadPath);
            if (localBundleInfo != null)
            {
                return tagBundleInfo.Compare(localBundleInfo, m_serverBundleInfo);
            }

            // 读取包内路径上的版本文件
            localBundleInfo = AssetBundleInfoManager.GetSingel().GetBundleInfo(AssetBundleManager.Instance.itfStreamingPath);
            if (localBundleInfo != null)
            {
                return tagBundleInfo.Compare(localBundleInfo, m_serverBundleInfo);
            }

            //if (toUpdateAssetBundleNames != null
            //    && toUpdateAssetBundleNames.Length != 0)
            //{
            //    // begin download!
            //    for (int i = 0; i < toUpdateAssetBundleNames.Length; i++)
            //    {
            //        Debug.Log("Compare: " + toUpdateAssetBundleNames[i]);
            //    }
            //}

            Debug.Log("local BundleInfo is null!");

            return toUpdateAssetBundleNames;
        }
        #endregion

        #region download method
        /// <summary>
        /// 异步下载assetbundles
        /// </summary>
        /// <param name="arrABNames"></param>
        /// <param name="finishDownloadCallback"></param>
        public void DownLoadAssetBundlesAsyc(string[] arrABNames, System.Action finishDownloadCallback = null)
        {
            //if (m_gameobject == null)
            //{
            //    Debug.LogError("behaviour not exist!");
            //    return;
            //}

            // 设置下载完成回调
            m_finishDownloadCallback = finishDownloadCallback;

            //开始下载
            Coroutine routine = StartCoroutine(_DownloadAssetBundlesAsyc(arrABNames));
            m_listCoroutine.Add(routine);
        }

        /// <summary>
        /// 下载assetbundles
        /// </summary>
        /// <returns></returns>
        private IEnumerator _DownloadAssetBundlesAsyc(string[] arrABNames)
        {
            // 不使用variaant，不会出现重复的包名，不需要特殊处理
            // 还是检查一下重名
            List<string> assetBundleNamesNonmultiple = new List<string>();
            for (int i = 0; i < arrABNames.Length; i++)
            {
                if (assetBundleNamesNonmultiple.Contains(arrABNames[i]) == false)
                {
                    assetBundleNamesNonmultiple.Add(arrABNames[i]);
                }
                else
                    Debug.LogWarning("appear multiple assetbundle, name: " + arrABNames[i]);
            }

            //循环加载所有包，更新时不需要加载其依赖包
            for (int i = 0; i < assetBundleNamesNonmultiple.Count; i++)
            {
                Debug.Log("begin download " + i + "/" + assetBundleNamesNonmultiple.Count + " " + assetBundleNamesNonmultiple[i]);
                //if (m_gameobject == null)
                //{
                //    Debug.LogError("GameMainObj not exist!");
                //    if (m_finishDownloadCallback != null)
                //    {
                //        m_finishDownloadCallback();
                //    }

                //    yield break;
                //}

                Coroutine routine = StartCoroutine(_DownloadAssetBundleInternal(assetBundleNamesNonmultiple[i]));
                m_listCoroutine.Add(routine);
                yield return routine;
            }

            if (m_finishDownloadCallback != null)
            {
                m_finishDownloadCallback();
            }
        }

        /// <summary>
        /// 实际使用www下载包的地方
        /// </summary>
        /// <param name="strABName"></param>
        /// <returns></returns>
        private IEnumerator _DownloadAssetBundleInternal(string strABName, bool isBundleInfoFile = false)
        {
            WWW download = null;
            string url = m_strBaseDownloadingURL + strABName;
            Debug.Log(url);
            download = new WWW(url);
            yield return download;

            if (download.error != null)
            {
                Debug.LogError(download.error);
                yield break;
            }

            if (download.isDone
                && download.assetBundle != null)
            {
                if (isBundleInfoFile == true
                    && AssetBundleInfoManager.IsExits())
                {
                    //解决乱码问题将下载的bundleinfo 字节流按照assetbundle的方式加载 而不是直接使用字节流
                    AssetBundle.UnloadAllAssetBundles(false); //这里是因为在某个地方已经加载了本地的bundleinfo ，如果不释放掉就不能加载刚刚下下来的这个buffer（暂时还没找到）
                    AssetBundle bundleinfo = AssetBundle.LoadFromMemory(download.bytes);
                   // AssetBundle bundleinfo = bundleinfoRe.assetBundle;
                    if (bundleinfo != null)
                    {
                        TextAsset text = bundleinfo.LoadAsset<TextAsset>("bundleinfo");
                        if (text != null)
                        {
                            m_serverBundleInfo = AssetBundleInfoManager.GetSingel().LoadBundleInfo(text.bytes);
                        }
                    }
                    bundleinfo.Unload(true);
                    // _SaveFile(strABName, download.bytes);

                }

                //如果是bundleinfo的话不应该是直接用字节流去读取数据因为这个字节流是元始的喂解密的bundle字节流
                // _SaveServerInfoAndLoad(strABName, download.bytes);
                
                else
                    _SaveFile(strABName, download.bytes);

                //包加载成功立即卸载，只需要缓存到本地即可
                if (download.assetBundle != null) {
                    download.assetBundle.Unload(false);
                }
                Debug.Log("download " + strABName + " bundle succeed!");
            }
            else
                //包加载失败
                Debug.LogError("assetBundle:" + strABName + " is invalid!");

            //也可以不调用Dispose，unity会在适当的时候释放www资源，如果dispose正在下载的资源，有极大问题，可能崩溃
            //download.Dispose();
        }

        /// <summary>
        /// 保存服务器版本文件到本地
        /// </summary>
        private void _SaveFile(string strFileName, byte[] arrBuffer)
        {
            if (strFileName == null
                || strFileName.Length == 0
                || arrBuffer == null
                || arrBuffer.Length == 0)
            {
                Debug.LogError("fileName or buffer cannot be null!");
                return;
            }

            string filePath = AssetBundleManager.Instance.itfDownloadPath + strFileName;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Create);

                fs.Write(arrBuffer, 0, arrBuffer.Length);

                fs.Flush();

                fs.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("write " + filePath + " error, an IO exception has been thrown, msg: " + ex.ToString());
            }

            Debug.Log("Save BundleInfo success!");
        }
        #endregion


        /// <summary>
        //保存服务器下载的bundleinfo并且加载
        /// </summary>
        /// 
        private void _SaveServerInfoAndLoad(string strFileName, byte[] arrBuffer) {
            _SaveFile(strFileName + "server", arrBuffer);
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

        //
        // 摘要:
        //     释放
        public void OnDestroy()
        {
            _StopAllCoroutine();
        }

        //
        // 摘要:
        //     回退到登录界面，比如切换帐号小退
        public void OnReturnToLoin()
        {
            _StopAllCoroutine();
        }
    }
}
