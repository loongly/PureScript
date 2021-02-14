using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PureScriptWrapper
{
    public class MonoBehaviourWrapper : MonoBehaviour, IWrapper
    {
        private uint Handle = 0;
        private bool awakeAfterInit = false;

        IntPtr[] FuncPtr = new IntPtr[(int)MonoBehaviourMethod.Count];

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern void InvokeFunction(IntPtr methodPtr);

        private void TryInvoke(MonoBehaviourMethod method)
        {
            IntPtr ptr = FuncPtr[(int)method];
            if (ptr != IntPtr.Zero)
            {
                InvokeFunction(ptr);
            }
        }

        public void Init()
        {
            for (int i = 0; i < FuncPtr.Length; i++)
            {
                MonoBehaviourMethod method = (MonoBehaviourMethod)i;
                FuncPtr[i] = WrapperUtils.GetFuncPtr(this, method.ToString());

                if (FuncPtr[i] != IntPtr.Zero)
                    Debug.LogError("bind method: " + method);
            }

            if(awakeAfterInit)
            {
                TryInvoke(MonoBehaviourMethod.Awake);
                awakeAfterInit = false;
            }
        }

        private void Awake()
        {
            if (Handle != 0)
                TryInvoke(MonoBehaviourMethod.Awake);
            else
                awakeAfterInit = true;
        }

        private void OnEnable()
        {
            TryInvoke(MonoBehaviourMethod.OnEnable);
        }
        void Start()
        {
            TryInvoke(MonoBehaviourMethod.Start);
        }

        void Update()
        {
            TryInvoke(MonoBehaviourMethod.Update);
        }
        private void OnDisable()
        {
            TryInvoke(MonoBehaviourMethod.OnDisable);
        }
        private void OnDestroy()
        {
            TryInvoke(MonoBehaviourMethod.OnDestroy);
            Dispose();
        }

        private void OnApplicationQuit()
        {
            TryInvoke(MonoBehaviourMethod.OnApplicationQuit);
        }

        public void Dispose()
        {
            WrapperUtils.Dispose(Handle);
        }
    }
    enum MonoBehaviourMethod
    {
        Awake,
        OnEnable,
        Start,
        Update,
        OnDisable,
        OnDestroy,
        OnApplicationQuit,

        Count
    }
}