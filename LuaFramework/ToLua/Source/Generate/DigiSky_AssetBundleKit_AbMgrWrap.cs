﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class DigiSky_AssetBundleKit_AbMgrWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(DigiSky.AssetBundleKit.AbMgr), typeof(System.Object));
		L.RegFunction("LoadAssetBundle", LoadAssetBundle);
		L.RegFunction("LoadAsset", LoadAsset);
		L.RegFunction("UnLoadAssetBundle", UnLoadAssetBundle);
		L.RegFunction("New", _CreateDigiSky_AssetBundleKit_AbMgr);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateDigiSky_AssetBundleKit_AbMgr(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				DigiSky.AssetBundleKit.AbMgr obj = new DigiSky.AssetBundleKit.AbMgr();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: DigiSky.AssetBundleKit.AbMgr.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAssetBundle(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes<string, string, string>(L, 1))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				string arg2 = ToLua.ToString(L, 3);
				DigiSky.AssetBundleKit.AbMgr.LoadAssetBundle(arg0, arg1, arg2);
				return 0;
			}
			else if (TypeChecker.CheckTypes<string, string, string>(L, 1) && TypeChecker.CheckParamsType<object>(L, 4, count - 3))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				string arg2 = ToLua.ToString(L, 3);
				object[] arg3 = ToLua.ToParamsObject(L, 4, count - 3);
				DigiSky.AssetBundleKit.AbMgr.LoadAssetBundle(arg0, arg1, arg2, arg3);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: DigiSky.AssetBundleKit.AbMgr.LoadAssetBundle");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadAsset(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<string, string>(L, 1))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				DigiSky.AssetBundleKit.AbMgr.LoadAsset(arg0, arg1);
				return 0;
			}
			else if (TypeChecker.CheckTypes<string, string>(L, 1) && TypeChecker.CheckParamsType<object>(L, 3, count - 2))
			{
				string arg0 = ToLua.ToString(L, 1);
				string arg1 = ToLua.ToString(L, 2);
				object[] arg2 = ToLua.ToParamsObject(L, 3, count - 2);
				DigiSky.AssetBundleKit.AbMgr.LoadAsset(arg0, arg1, arg2);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: DigiSky.AssetBundleKit.AbMgr.LoadAsset");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UnLoadAssetBundle(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1 && TypeChecker.CheckTypes<UnityEngine.AssetBundle>(L, 1))
			{
				UnityEngine.AssetBundle arg0 = (UnityEngine.AssetBundle)ToLua.ToObject(L, 1);
				DigiSky.AssetBundleKit.AbMgr.UnLoadAssetBundle(arg0);
				return 0;
			}
			else if (count == 1 && TypeChecker.CheckTypes<string>(L, 1))
			{
				string arg0 = ToLua.ToString(L, 1);
				DigiSky.AssetBundleKit.AbMgr.UnLoadAssetBundle(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: DigiSky.AssetBundleKit.AbMgr.UnLoadAssetBundle");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

