
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if !WRAPPER_SIDE
using AOT;
#endif

internal static class StringStore
{

#if !WRAPPER_SIDE
    [MonoPInvokeCallback(typeof(Custom.StoreStringType))]
#endif
    public unsafe static int OnReceiveStr(char* c)
    {
        var str = new string(c);
        var h = ObjectStore.Store(str);
        return h;
    }

    public unsafe static int Store(string str)
    {
        var h = ObjectStore.GetHandle(str);
        if (h > 0)
            return h;

        int handle;
        fixed (char* chr = str)
        {
            handle = Custom.StoreString(chr);
        }

        ObjectStore.Store(str, handle);
        return handle;
    }

    public static string Get(int handle)
    {
        return (string)ObjectStore.Get(handle);
    }
}
