# PureScript

[![license](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/loongly/PureScript/blob/master/LICENSE)[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/loongly/PureScript/pulls)


一个支持Unity3D的C#热更框架，基于Mono的[MONO_AOT_MODE_INTERP](https://www.mono-project.com/news/2017/11/13/mono-interpreter/)模式。  


>支持在iOS平台Assembly.Load  
>构建时自动绑定Unity的Il2cpp代码。  
>支持大部分Unity特性，包括MonoBehaviour、Coroutine。  
>支持配置程序集运行环境（Il2cpp/aot/interp）  
>支持`Cocoapods`自动集成  
>支持对"magic code"的自定义绑定实现

-------------------------------------------
## iOS平台  
使用“Mixed Mode Execution” 兼顾性能(aot)和灵活性(interpreter)

## Windows平台
使用Jit模式运行，可以导出Il2cpp工程，添加ScriptEngine项目断点调试。

## Android平台
请使用Unity的Mono运行时，可以直接调用`Assembly.Load`，同使用PureScript是等效的，工程结构无需修改。

---------------------------------------------------
## 使用
1. Clone本工程，拷贝DemoProject/Assets/Plugins/PureScript目录。
2. 修改 PureScriptBuilder.cs及ScriptEngine/Tools/config.json中的路径配置。
3. config.json中配置运行在interpreter模式的dll(否则以aot运行),以及运行在Il2cpp运行时内的dll(一般用作Adapter)。

+ iOS平台需要安装[Cocoapods](https://cocoapods.org/)和[Ninja](https://ninja-build.org/) 。在项目的podfile内添加PureScript引用。 
例： */iOS/Podfile-example  
然后  

        pod install  



+ Windows平台仅用来调试，在构建项目后，编译 ScriptEngine/ScriptEngine.vcxproj,替换原来Plugins目录下的的dll，或者导出Il2cpp的工程添加ScriptEngine.vcxproj项目调试运行。


## 例子
以下两段代码是等效的，详细参考DemoProject/*/MonoEntry.cs。

    ScriptEngine.Setup(reloadDir, "TestEntry.dll");
    
    // equal to:
    
    Assembly assembly = Assembly.Load("TestEntry.dll");
    Type type = assembly.GetType("MonoEntry");
    MethodInfo mi = type.GetMethod("Main");
    var res = mi.Invoke(null, null)

`注意` 需要热更新的程序集如果是Unity自动生成的工程，会自动引用一堆的无用dll，比如UntiyEditor*.dll,和一堆根本不会用到的System*.dll ，因为安装包内并没有带上这些，加载会失败。这时需要手动删掉这些用不到的引用。或者再建一个工程，手动管理引用，后面也可以考虑做自动化工具，strip掉没用的引用。    

--------------------------------------------

## 实现
PureScript 封装了Mono运行时，c/csharp代码生成器，pod项目自动集成，实现了两种绑定方式。两种方式均在构建时自动生成绑定代码，调用方几乎无感知，具体参考DemoProject。
两种绑定均支持调用和回调。

* Internal call 绑定：
由c/cpp实现，直接使用Unity的dll，同时把Unity的Internal call绑定到UnityEngine实现，几乎没有性能损失，调用方无需修改。  
例：ScriptEngine/generated/icall_binding_gen.c。  
如果碰到“Unity magic”代码自动绑定有问题，可以实现自己的绑定。  
例：ScriptEngine/custom/icall_binding.c。  
或者在CSharp层用Adapter绑定。


* Adapter绑定：
由纯CSharp实现，分别在Mono端和Il2cpp端生成绑定代码，具有更好的兼容和灵活性。Mono运行时内的dll调用Il2cpp内的dll时用到，在config.json中配置需要运行在Il2cpp内又需要在Mono内调用的dll，构建时自动生成绑定代码，在aot执行，调用时自动替换到绑定代码，调用方无需修改。
例：DemoProject/*/AdapterTest

----------------------------------

正式项目请将Mono库(mono*.dll/mono*.a) 替换为自己[编译](https://github.com/mono/mono/tree/master/sdks)的。

如果大家对这个方案有兴趣再补充详细文档，同时欢迎提交PR或者Star。

有问题请联系 loongly@foxmail.com
