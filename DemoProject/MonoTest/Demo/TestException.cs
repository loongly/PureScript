using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PureScript;
public class TestException : MonoBehaviour
{
    ExceptionTest other;

    private void Start()
    {
        Debug.LogError("##Start 1");
        other = new ExceptionTest();
        Debug.LogError("##Start 2");
        other.callback = OnCallback;
        Debug.LogError("##Start 3");
        Application.focusChanged += OnFocusCallbackException;
        Debug.LogError("##Start 4");
    }

    /*
     Exception: System.NullReferenceException: Object reference not set to an instance of an object.
    *   at PureScript.ExceptionTest.NullPointException () [0x00000] in <00000000000000000000000000000000>:0 
        at UnityBind.PureScript_ExceptionTest_NullPointException (System.Int32 thiz_h) [0x00000] in <00000000000000000000000000000000>:0 
        at (wrapper managed-to-native) System.Object.wrapper_native_00007FFA513268C0(int)
    *   at PureScript.ExceptionTest.NullPointException () [0x0000c] in <8365f2ae61e94f83a867dc955d743bc6>:0 
        at TestException.OnFocusNullPointException (System.Boolean focus) [0x00012] in <7bba8e115a884a978ebee1a64957c99e>:0 
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


    void OnFocusCallbackException(bool focus)
    {
        if(focus)
        {
            try
            {
                Debug.LogError("##OnFocus 1");
                other.TestCallBack();
                Debug.LogError("##OnFocus 2");
            }catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
            
        }
    }

    void OnCallback()
    {
        Debug.LogError("##OnCallback 1");
        throw new NullReferenceException("This exception turn around ~~");
        Debug.LogError("##OnCallback 2");
    }
}
