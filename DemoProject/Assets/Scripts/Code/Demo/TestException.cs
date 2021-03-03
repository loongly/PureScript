using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PureScript;
using System.Runtime.InteropServices;

public class TestException : MonoBehaviour
{
    ExceptionTest other;

    [DllImport("ScriptEngine", EntryPoint = "OnExceptionIl2cpp", CallingConvention = CallingConvention.Cdecl)]
    public static extern void RaiseIl2cppException(string msg);

    private void Start()
    {
        Debug.LogError("##Start 1");
        other = new ExceptionTest();
        Debug.LogError("##Start 2");
        other.callback = TestCallbackException;
        Debug.LogError("##Start 3");
        //Application.focusChanged += OnFocusNullPointException;
        Application.focusChanged += OnFocusCallbackException;
        Debug.LogError("##Start 4");
    }

    /*
     Exception: System.NullReferenceException: Object reference not set to an instance of an object.
      * at PureScript.ExceptionTest.NullPointException () [0x00000] in <00000000000000000000000000000000>:0 
        at UnityBind.PureScript_ExceptionTest_NullPointException (System.Int32 thiz_h) [0x00000] in <00000000000000000000000000000000>:0 
        at (wrapper managed-to-native) PureScript.ScriptEngine.CheckException()
      * at PureScript.ExceptionTest.NullPointException () [0x00012] in <32aba121fea14339b39ae3ef7a81ab85>:0 
        at TestException.OnFocusNullPointException (System.Boolean focus) [0x00012] in <a78fd91ac283481f9b729fcc794fb0b0>:0 
        at UnityEngine.Application.InvokeFocusChanged (System.Boolean focus) [0x0000b] in <cf7fb9a754e74776ae71c25165ca083c>:0 
        at (wrapper native-to-managed) UnityEngine.Application.InvokeFocusChanged(bool,System.Exception&)
         */
    void OnFocusNullPointException(bool focus)
    {
        if(focus)
        {
            Debug.LogError("##OnFocus 1");
            other.NullPointException();
            Debug.LogError("##OnFocus 2");
        }
    }

    /*
     Exception: System.Exception: System.Exception: This exception turn around ~~
        at TestException.TestCallbackException () [0x0000c] in <ae28305b9ad94cbf995b02f06babebda>:0 
        at PureScript.ExceptionTest.Oncallback (System.Int32 arg0_h) [0x0000b] in <32aba121fea14339b39ae3ef7a81ab85>:0 
        at PureScript.ScriptEngine.CheckException () [0x00000] in <00000000000000000000000000000000>:0 
        at UnityBind.OnExceptionTest_callback (PureScript.ExceptionTest arg0) [0x00000] in <00000000000000000000000000000000>:0 
        at System.Action.Invoke () [0x00000] in <00000000000000000000000000000000>:0 
      * at PureScript.ExceptionTest.TestCallBack () [0x00000] in <00000000000000000000000000000000>:0 
        at UnityBind.PureScript_ExceptionTest_TestCallBack (System.Int32 thiz_h) [0x00000] in <00000000000000000000000000000000>:0 
        at (wrapper managed-to-native) PureScript.ScriptEngine.CheckException()
      * at PureScript.ExceptionTest.TestCallBack () [0x00012] in <32aba121fea14339b39ae3ef7a81ab85>:0 
        at TestException.OnFocusCallbackException (System.Boolean focus) [0x00012] in <ae28305b9ad94cbf995b02f06babebda>:0 
        at UnityEngine.Application.InvokeFocusChanged (System.Boolean focus) [0x0000b] in <cf7fb9a754e74776ae71c25165ca083c>:0 
        at (wrapper native-to-managed) UnityEngine.Application.InvokeFocusChanged(bool,System.Exception&)
         */
    void OnFocusCallbackException(bool focus)
    {
        if(focus)
        {
            //RaiseMonoException("testException");
            //return;

            Debug.LogError("##OnFocus 1");
            other.TestCallBack();
            Debug.LogError("##OnFocus 2");
            return;
        }
    }

    void TestCallbackException()
    {
        Debug.LogError("##OnCallback 1");
        throw new Exception("This exception turn around ~~");
        Debug.LogError("##OnCallback 2");
    }
}
