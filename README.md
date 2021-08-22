# PureScript

[![license](http://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/loongly/PureScript/blob/master/LICENSE)[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg)](https://github.com/loongly/PureScript/pulls)


一个支持Unity3D的C#热更框架，基于Mono的[MONO_AOT_MODE_INTERP](https://www.mono-project.com/news/2017/11/13/mono-interpreter/)模式。  


>支持在iOS平台Assembly.Load  
>构建时自动绑定Unity的Il2cpp代码。  
>支持大部分Unity特性，包括MonoBehaviour、Coroutine。  
>支持配置程序集运行环境（Il2cpp/aot/interp）  
>支持`Cocoapods`自动集成  
>支持对"magic code"的自定义绑定实现

[iOS真机演示视频](https://www.zhihu.com/zvideo/1351991103290302464)  
[项目介绍](https://zhuanlan.zhihu.com/p/355280556)

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
    config.json（默认配置为Demo工程配置）：  

        ScriptEngineDir： 指向PureScript\ScriptEngine目录
        AdapterSet：      配置即要在Il2cpp又要在Mono中使用的dll,一般为一些通用插件
        InterpSet：       配置需要热更的dll,否则运行在aot中   


4. ScriptEngine启动接口请参考 DemoProject\Assets\Scripts\Lancher.cs, (注意修改'reloadDir'变量)。

### iOS平台
  iOS平台需要安装[Cocoapods](https://cocoapods.org/)和[Ninja](https://ninja-build.org/) 。并在项目的podfile内添加PureScript引用。 
例： */iOS/Podfile-example  
导出xcode工程，然后  

        pod install  


### Windows平台
  Windows平台仅用来调试，目前未添加自动集成，在构建项目后，需编译 ScriptEngine/ScriptEngine.vcxproj,替换原来Plugins目录下的的ScriptEngine.dll。  
  ScriptEngine.vcxproj 属性/VC++目录/包含目录中使用了宏：$(UnityEditorPath) 指向Unity/Editor目录。  

调试步骤：    

1. 修改Lancher.cs中的reloadDir，替换其中的{path_to_ScriptEngine}指向PureScript\ScriptEngine\Managed目录 
2. Unity导出VS工程。  
3. 需要删除Unity导出目录下的Managed目录例如($(ExportPath)/DemoProject/Managed),否则Mono会默认从此处加载dll,Il2cpp并不会使用此目录，但是每次构建都会导出。
4. 在导出的解决方案中添加ScriptEngine.vcxproj,并在主项目中添加ScriptEngine项目的依赖（方便调试）。  
5. 修改ScriptEngine.vcxproj中的输出目录为 $(ExportPath)/build/bin/DemoProject_Data/Plugins/,即替换原本的ScriptEngine.dll。  
6. 运行项目


## 例子
以下两段代码是等效的，详细参考DemoProject/*/MonoEntry.cs。  
```c#
  ScriptEngine.Setup(reloadDir, "TestEntry.dll");

  // equal to:

  Assembly assembly = Assembly.Load("TestEntry.dll");
  Type type = assembly.GetType("MonoEntry");
  MethodInfo mi = type.GetMethod("Main");
  var res = mi.Invoke(null, null)
```
## `注意`   
1. 需要热更新的程序集可以使用UnityEditor上的菜单PureScript/Build构建。    
2. 目前iOS平台带的mono库为arm64指令集没有开启bitcode，如有其它需求请自己构建一个替换。
3. 如果是windows平台记得根据上面说明生成ScriptEngine.dll 替换导出目录中的dll
4. 暂时不支持Unity2020
5. 建议所有网络或文件IO操作统一在il2cpp内执行，配置为Adapter提供给Mono使用。

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
1. 绑定分两部分，构建时会自动生成Adapter.gen.dll(Il2cpp内执行)和Adapter.wrapper.dll(Mono内执行)，生成代码参考：ScriptEngine/Adapter/glue/*.cs。
2. 运行时ScriptEngine.Setup会将需要绑定的接口生成代理对象，然后序列化到非托管内存中，同时将内存指针报错到ScriptEngine中,参考：ScriptEngine.c。
3. Mono运行时启动后，执行Main函数时会首先从ScriptEngine读取内存指针，然后反序列化为代理，供Wrapper调用。
4. Mono运行时内，调用被Adapter绑定过的程序集时会自动指向Adapter.wrapper.dll内的Wrapper实现。   
注：代理的序列化与反序列化[参考](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.getfunctionpointerfordelegate)：  
 ```
 Marshal.GetFunctionPointerForDelegate
 Marshal.GetDelegateForFunctionPointer
 ```

## 自动绑定过程
* 构建Unity项目时PureScriptBuilder.cs内注册了回调，并通过hook的方式分别在`StripAssemblies`前/后添加了绑定调用,此处是要再Il2cpp前将已经`Strip`的Assemblies拷贝出一份，并且进行Adapter绑定的代码生成。
* 第一次绑定调用是做Adapter绑定的代码生成，生产物是Adapter.gen.dll和Adapter.wrapper.dll，这步需要在`Strip`前，否则会触发Il2cpp构建错误。
* 第二次绑定调用是在`Strip`后，此次绑定会将已经`Strip`的Assemblies拷贝出一份。  
  然后进行icall绑定，生产物是ScriptEngine/generated/*.c  
  如果是iOS平台，会进行aot构建,首先生成`build.ninja`,然后通过ninja生成ScriptEngine/aot/*.a

----------------------------------

正式项目请将Mono库(mono*.dll/mono*.a) 替换为自己[编译](https://github.com/mono/mono/tree/master/sdks)的。

如果大家对这个方案有兴趣再补充详细文档，同时欢迎提交PR或者Star。

有问题欢迎提交issue 或 loongly@foxmail.com
