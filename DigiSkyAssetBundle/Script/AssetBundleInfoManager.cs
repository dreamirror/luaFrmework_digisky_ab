using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DigiSky.AssetBundleKit
{
    /// <summary>
    /// 版本信息
    /// </summary>
    public class tagBundleInfo
    {
        /// <summary>
        /// 单个包的信息
        /// </summary>
        class tagOneBundleInfo
        {
            // 包名
            public string strABName;

            // 包hash，用于更新
            public string strHashcode;

            // 包crc，用于检查包是否破损，暂时不用
            public string strCrc;
        }

        // BundleInfo文件字节缓存
        byte[] m_byteBuffer;

        // 所有包信息
        Dictionary<string, tagOneBundleInfo> m_allBundleInfo;

        // BundleInfo文件字节缓存接口
        public byte[] itfBuffer
        {
            get
            {
                return m_byteBuffer;
            }
        }

        // 初始化
        public void Init(byte[] buffer)
        {
            m_allBundleInfo = new Dictionary<string, tagOneBundleInfo>();

            m_byteBuffer = buffer;

            _Analysis(m_byteBuffer);
        }

        /// <summary>
        /// 检查assetBundle是否已经缓存
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public bool IsCached(string assetBundleName)
        {
            if (m_allBundleInfo != null)
            {
                return m_allBundleInfo.ContainsKey(assetBundleName);
            }

            return false;
        }

        /// <summary>
        /// 解析文件
        /// </summary>
        /// <param name="buffer"></param>
        private void _Analysis(byte[] buffer)
        {
            
            if (buffer == null
                || buffer.Length == 0)
            {
                Debug.LogWarning("buffer can not be null!");
                return;
            }
            bool isCache = IsCached("bundleinfo");
            if (isCache) {
                Debug.LogWarning("IS CACHEED");
            }

            Stream temp = new MemoryStream(buffer);
            if (temp != null)
            {
                StreamReader reader = new StreamReader(temp);
                if (reader != null)
                {
                    // 读一行
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 字段间用制表符分隔
                        string[] items = line.Split('\t');
                        if (items != null
                            && items.Length == 3)
                        {
                            tagOneBundleInfo oneBundleInfo = new tagOneBundleInfo();
                            oneBundleInfo.strABName = items[0];
                            oneBundleInfo.strHashcode = items[1];
                            oneBundleInfo.strCrc = items[2];
                            if (m_allBundleInfo.ContainsKey(oneBundleInfo.strABName) == false)
                            {
                                m_allBundleInfo.Add(oneBundleInfo.strABName, oneBundleInfo);
                            }
                            else
                                Debug.LogWarning("file BundleInfo contain multiple assetbundle: " + oneBundleInfo.strABName);
                        }
                        else
                            Debug.LogError("invaild bundlefileinfo:\"" + line + "\"");
                    }
                }
                else
                    Debug.LogError("create StreamReader failed!");
            }
            else
                Debug.LogError("create MemoryStream failed!");
        }

        /// <summary>
        /// 对比两个BundleInfo
        /// </summary>
        /// <param name="localBundleInfo"></param>
        /// <param name="serverBundleInfo"></param>
        /// <returns></returns>
        public static string[] Compare(tagBundleInfo localBundleInfo, tagBundleInfo serverBundleInfo)
        {
            // 本地BundleInfo可以为空，但是服务器的一定存在
            if (serverBundleInfo == null)
            {
                Debug.LogError("serverBundleInfo cannot be null!");
                return null;
            }

            List<string> toUpdateList = new List<string>();

            // 遍历服务器包信息，hashcode有变化或者本地没有的包都需要更新
            Dictionary<string, tagOneBundleInfo>.Enumerator serverItor = serverBundleInfo.m_allBundleInfo.GetEnumerator();
            while (serverItor.MoveNext())
            {
                string serverBundleName = serverItor.Current.Value.strABName;
                //string servercrc = serverItor.Current.Value.crc;
                string serverHashcode = serverItor.Current.Value.strHashcode;
                Hash128 serverHash = Hash128.Parse(serverHashcode);
                if (serverHash.isValid == false)
                {
                    Debug.LogError("Parse Hash128 failed, string: " + serverHashcode);
                }
                Debug.Log("____________"+ serverBundleName); 
                Debug.Log("local code=="+ localBundleInfo.m_allBundleInfo[serverBundleName].strHashcode);
                Debug.Log("server code =="+ serverHashcode);
                if (localBundleInfo == null
                    || !localBundleInfo.m_allBundleInfo.ContainsKey(serverBundleName)
                    || localBundleInfo.m_allBundleInfo[serverBundleName].strHashcode != serverHashcode)
                {
                    toUpdateList.Add(serverBundleName);
                }
            }

            return toUpdateList.ToArray();
        }
    }

    public class AssetBundleInfoManager : DigiSky.AssetBundleKit.SingelBehaviour<AssetBundleInfoManager>
    {
        private Dictionary<string, tagBundleInfo> m_dicBundleInfos = null;

		protected override void Awake()
		{
			base.Awake();

			m_dicBundleInfos = new Dictionary<string, tagBundleInfo>();
		}

        /// <summary>
        /// 获取bundleinfo
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public tagBundleInfo GetBundleInfo(string strPath)
        {
            if (m_dicBundleInfos == null)
                return null;

            if (m_dicBundleInfos.ContainsKey(strPath))
            {
                return m_dicBundleInfos[strPath];
            }
            else
            {
                tagBundleInfo bundleInfo = _LoadBundleInfo(strPath, "bundleinfo");
                m_dicBundleInfos[strPath] = bundleInfo;

                return bundleInfo;
            }
        }

        /// <summary>
        /// 重新加载bundleinfo
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public tagBundleInfo ReloadBundleInfo(string strPath)
        {
            tagBundleInfo bundleInfo = _LoadBundleInfo(strPath, "bundleinfo");
            m_dicBundleInfos[strPath] = bundleInfo;

            return bundleInfo;
        }

        /// <summary>
        /// 加载bundleInfo
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        private tagBundleInfo _LoadBundleInfo(string strPath, string strAssetName)
        {
            // 加载并初始化bundleInfo包
            AssetBundle assetBundle = AssetBundle.LoadFromFile(strPath + strAssetName);

            if (assetBundle != null)
            {
                TextAsset text = assetBundle.LoadAsset<TextAsset>(strAssetName);
                if (text != null)
                {
                    tagBundleInfo bundleInfo = new tagBundleInfo();
                    bundleInfo.Init(text.bytes);
                    assetBundle.Unload(true);
                    return bundleInfo;
                }
            }
            
            Debug.LogError("加载bundleInfo失败，path：" + strPath);
            return null;
        }

        /// <summary>
        /// 加载bundleInfo
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public tagBundleInfo LoadBundleInfo(byte[] buffer)
        {
            tagBundleInfo bundleInfo = new tagBundleInfo();
            bundleInfo.Init(buffer);

            return bundleInfo;
        }

        //
        // 摘要:
        //     释放
        public void OnDestroy()
        {
            
        }

        //
        // 摘要:
        //     回退到登录界面，比如切换帐号小退
        public void OnReturnToLoin()
        {
            
        }
    }
}