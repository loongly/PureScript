# PureScript
Unity上的C#热更框架,基于Mono的[MONO_AOT_MODE_INTERP](https://www.mono-project.com/news/2017/11/13/mono-interpreter/)模式。自动绑定Unity的Il2cpp代码。

-------------------------------------------
## iOS平台  
使用“Mixed Mode Execution” 兼顾性能(aot)和灵活性(interpreter)

## windows平台
使用jit模式运行，可以导出Il2cpp工程，添加ScriptEngine项目断点调试。

## Andrroid平台
请直接使用`Assembly.Load`，同使用PureScript是等效的。

---------------------------------------------------
## 使用
Clone本工程，拷贝DemoProject/Assets/Plugins/PureScript目录  
+ iOS平台需要安装[Cocoapods](https://cocoapods.org/)和[Ninja](https://ninja-build.org/) 。在项目的podfile内添加PureScript引用。 
例： */iOS/Podfile-example  
然后  

        pod install  



+ Windows平台仅用来调试用，在构建项目后，编译 ScriptEngine/ScriptEngine.vcxproj,替换原来Plugins目录下的的dll，或者导出Il2cpp工程添加ScriptEngine.vcxproj项目调试运行。


## 例子
以下两段代码是等效的，详细参考DemoProject/*/MonoEntry.cs。

    ScriptEngine.Setup(reloadDir, "TestEntry.dll");

    // equal to:

    Assembly assembly = Assembly.Load("TestEntry.dll");
    Type type = assembly.GetType("MonoEntry");
    MethodInfo mi = type.GetMethod("Main");
    var res = mi.Invoke(null, null)

--------------------------------------------

## 实现
PureScript 附带了Mono运行时，代码生成器，实现了两种绑定方式。两种方式均在构建时自动生成绑定代码，调用方几乎无感知，具体参考DemoProject。
两种绑定均支持调用和回调。

* Internal call 绑定：
由c/cpp实现，直接使用Unity的dll，同时把Unity的Internal call重定向到UnityPlayerEngine实现，调用方无需修改。  
例：ScriptEngine/generated/icall_binding_gen.c。  
如果碰到“Unity magic”代码自动绑定无效，可以实现自己的绑定。  
例：ScriptEngine/custom/icall_binding.c。  
或者在CSharp层用Adapter绑定。


* Adapter绑定：
由纯CSharp实现，具有更好的兼容和灵活性。Mono运行时内的dll调用Il2cpp内的dll时用到，配置需要运行在Il2cpp内又需要在Mono内调用的dll，构建时自动生成绑定代码，调用时自动替换到绑定代码，调用方无需修改。
例：DemoProject/*/AdapterTest

----------------------------------

如果大家对这个方案有兴趣再补充详细文档，同时欢迎提交PR或者Star。

