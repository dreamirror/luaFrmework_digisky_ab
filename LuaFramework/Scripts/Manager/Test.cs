using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using DigiSky.AssetBundleKit;
    public class Test
    {
    public string b = "";
    public Test(string a) {
        b = a;
    }
        public void Say(string ab,string call) {
            Debug.Log("I am test!!11111111111111 and b =="+ call);
            AbMgr.LoadAssetBundle(ab, call);
        }
    }

