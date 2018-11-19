--主入口函数。从这里开始lua逻辑
function Main()					
	print("logic start111")
    --local Test = luanet.import_type("Test")
    local test = Test("sss");
    test:Say();	 		
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
    
end

function OnApplicationQuit()
end