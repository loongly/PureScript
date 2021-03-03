
using PureScript.Mono;
using System;
using PureScript;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;
namespace PureScript
{
	public class StartInfo : WObject
	{
		public static System.String ReloadDllName
		{
			get
			{
				System.String res = MonoBind.PureScript_StartInfo_get_ReloadDllName();
				ScriptEngine.CheckException();
				return res;
			}
		}
		public static System.String ReloadClassName
		{
			get
			{
				System.String res = MonoBind.PureScript_StartInfo_get_ReloadClassName();
				ScriptEngine.CheckException();
				return res;
			}
		}
		public static System.String TestMethodName
		{
			get
			{
				System.String res = MonoBind.PureScript_StartInfo_get_TestMethodName();
				ScriptEngine.CheckException();
				return res;
			}
		}
		public StartInfo()
		{
			var h = MonoBind.PureScript_StartInfo__ctor_1();
			ScriptEngine.CheckException();
			SetHandle(h);
			ObjectStore.Store(this, h);
		}
	}
}

