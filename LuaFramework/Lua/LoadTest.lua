--主入口函数。从这里开始lua逻辑
LoadTest = {}

--加载完成ab之后的回调
function LoadTest.load_call()
    print("LoadTest call back")

end

