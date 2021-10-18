using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if !WRAPPER_SIDE
using AOT;
#endif

internal static unsafe class Custom
{
    
#if WRAPPER_SIDE
        public static void DeSer(IntPtr ptr)
        {
        
        }
#else
    public static void Ser(IntPtr ptr)
    {
        
    }
    
#endif


}

