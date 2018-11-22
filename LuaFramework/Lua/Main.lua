--主入口函数。从这里开始lua逻辑

function Main()					
	print("lua main!!!")
    --local Test = luanet.import_type("Test")
    --local test = Test("sss");
   -- test:Say("sss","laod_call");
    
   --local ab_mgr = DigiSky.AssetBundleKit.AbMgr();
   DigiSky.AssetBundleKit.AbMgr.LoadAssetBundle("sss","load_call");		 		
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
    
end

function OnApplicationQuit()

end

