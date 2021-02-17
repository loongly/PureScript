using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace PureScriptWrapper
{
    internal class EnumeratorWrapper : IEnumerator, IWrapper
    {
        private IntPtr m_CachedPtr = IntPtr.Zero;//Align with Object
        private uint Handle = 0;

        private IntPtr CurrentFuncPtr = IntPtr.Zero;
        private IntPtr MoveNextFuncPtr = IntPtr.Zero;
        private IntPtr ResetFuncPtr = IntPtr.Zero;

        public void Init()
        {
            if (CurrentFuncPtr != IntPtr.Zero)
                return;

            CurrentFuncPtr = WrapperUtils.GetFuncPtr(this, "get_Current");

            if (CurrentFuncPtr == IntPtr.Zero)
                CurrentFuncPtr = WrapperUtils.GetFuncPtr(this, "System.Collections.Generic.IEnumerator<System.Object>.get_Current");
            if (CurrentFuncPtr == IntPtr.Zero)
                CurrentFuncPtr = WrapperUtils.GetFuncPtr(this, "System.Collections.IEnumerator.get_Current");
            
            MoveNextFuncPtr = WrapperUtils.GetFuncPtr(this, "MoveNext");

            ResetFuncPtr = WrapperUtils.GetFuncPtr(this, "Reset");
            if (ResetFuncPtr == IntPtr.Zero)
                ResetFuncPtr = WrapperUtils.GetFuncPtr(this, "System.Collections.IEnumerator.Reset");
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern object InvokeCurrent(IntPtr methodPtr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern bool InvokeMoveNext(IntPtr methodPtr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern void InvokeReset(IntPtr methodPtr);



        public object Current
        {
            get
            {
                object cur = InvokeCurrent(CurrentFuncPtr);

                return cur;
            }
        }

        public bool MoveNext()
        {
            return InvokeMoveNext(MoveNextFuncPtr);
        }

        public void Reset()
        {
            InvokeMoveNext(ResetFuncPtr);
        }

        public void Dispose()
        {
            WrapperUtils.Dispose(this);
        }

        ~EnumeratorWrapper()
        {
            Dispose();
        }
    }
}