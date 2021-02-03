using System;
using System.Collections.Generic;
namespace UnityEngine
{
    public class WObject
    {
        internal int Handle { get; private set; }
        internal void SetHandle(int handle)
        {
            Handle = handle;
        }

        internal WObject()
        {

        }

        internal WObject(int handle,IntPtr ptr)
        {
            Handle = handle;
        }
    }

    public static class WrapperStore
    {
        static WObject[] objects;

        static int MaxObjects;

        private static void Init(int maxObjects)
        {
            MaxObjects = maxObjects;
            objects = new WObject[maxObjects + 1];
        }

        public static T Store<T>(int handle,T obj) where T : WObject
        {
            if (handle <= 0 || handle > MaxObjects)
                return null;

            objects[handle] = obj;
            return obj;
        }

        public static T Get<T>(int handle) where T : WObject
        {
            var obj = objects[handle];
            if(obj != null)
                return obj as T;

            //create new
            var newObj = System.Activator.CreateInstance(typeof(T), handle,IntPtr.Zero) as T ;
            //return Store<T>(handle, newObj);
            objects[handle] = newObj;
            return newObj;
        }

        public static void Remove(int handle)
        {
            if (handle <= 0 || handle > MaxObjects)
                return;

            objects[handle] = null;
        }
    }
}