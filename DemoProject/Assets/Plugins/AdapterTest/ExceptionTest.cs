using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PureScript
{
    public class ExceptionTest
    {
        public Action callback { get; set; }
        GameObject obj { get; set; }
        public void NullPointException()
        {
            try
            {
                obj.name = "";
            }catch(ArgumentException e)
            {
                Debug.LogException(e);
            }
        }

        public void TestCallBack()
        {
            Debug.LogError("TestCallBack 1");
            callback?.Invoke();
            Debug.LogError("TestCallBack 2");
        }
    }
}
