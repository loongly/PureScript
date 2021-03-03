
using PureScript.Mono;
using System;
using UnityEngine;
using PureScript;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;
namespace PureScript
{
	public class ExceptionTest : WObject
	{
		public  System.Action _callback;
		static Delegate5523eb28 callbackAction = Oncallback;
		static void Oncallback(int arg0_h)
		{
			Exception __e = null;
			try
			{
				var arg0Obj = ObjectStore.Get<global::PureScript.ExceptionTest>(arg0_h);
				arg0Obj._callback();
			}
			catch(Exception e)
			{
				__e = e;
			}
			if(__e != null)
			ScriptEngine.OnException(__e.ToString());
		}
		public  System.Action callback
		{
			set
			{
				bool attach = (_callback == null);
				_callback = value;
				if(attach)
				{
					var callbackAction_p = Marshal.GetFunctionPointerForDelegate(callbackAction);
					MonoBind.PureScript_ExceptionTest_set_callback(this.Handle, callbackAction_p);
					ScriptEngine.CheckException();
				}
			}
			get
			{
				return _callback;
			}
		}
		public void NullPointException()
		{
			MonoBind.PureScript_ExceptionTest_NullPointException(this.Handle);
			ScriptEngine.CheckException();
			return ;
		}
		public void TestCallBack()
		{
			MonoBind.PureScript_ExceptionTest_TestCallBack(this.Handle);
			ScriptEngine.CheckException();
			return ;
		}
		public ExceptionTest()
		{
			var h = MonoBind.PureScript_ExceptionTest__ctor();
			ScriptEngine.CheckException();
			SetHandle(h);
			ObjectStore.Store(this, h);
		}
	}
}

