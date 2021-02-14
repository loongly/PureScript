using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.TestScript
{
    public class TestCoroutine : MonoBehaviour
    {
        int tv = 1;
        CoroutineW coroutine;
        private void OnEnable()
        {
            TestEnum();
            coroutine = new CoroutineW(RunTest("test start"));
            StartCoroutine(coroutine);
        }

        IEnumerator RunTest(string str,int a = 1)
        {
            int testValue = 2;
            yield return null;

            var resReq = Resources.LoadAsync("test/res");
            yield return resReq;

            Debug.Log("AAA" + str);
            yield return new WaitForSeconds(2.0f);
            tv = 2;
            Debug.Log("BBB");
            float end = Time.time + 3.0f;
            yield return new WaitUntil(()=> Time.time > end);

            Debug.Log("CCC");

            yield return 0;

            Debug.Log("DDD");
            yield return new CostomWait(Time.time + 3.0f);

            Debug.Log("EEE");

            yield return "test end";
            Debug.Log("FFF");
        }

        public void TestEnum()
        {
            int a = 1;
            int b = 2;
            /*FullScreenMovieControlMode aa = (FullScreenMovieControlMode)a;
            FullScreenMovieScalingMode bb = (FullScreenMovieScalingMode)b;
            Handheld.PlayFullScreenMovie("", Color.black, aa, bb);*/
        }
    }

    

   /* public abstract class CustomInstruction : IEnumerator
    {
        public abstract bool keepWaiting
        {
            get;
        }

        public object Current
        {
            get
            {
                return null;
            }
        }
        public bool MoveNext() { return keepWaiting; }
        public virtual void Reset() { }
    }*/

    public class CostomWait : CustomYieldInstruction
    {
        float waitTime;
        public CostomWait(float time)
        {
            waitTime = time;
        }
        public override bool keepWaiting
        {
            get {
                return Time.time < waitTime;
            }
        }
    }


    // ========================================================================

    public class CustomInstruction : CustomYieldInstruction
    {
        CustomYieldInstruction instance;
        public CustomInstruction(CustomYieldInstruction obj)
        {
            instance = obj;
        }
        public override bool keepWaiting {
            get {
                return Internal_KeepWaiting();
            }
        }
        public bool MoveNext() { return keepWaiting; }
        public virtual void Reset() { }

        public bool Internal_KeepWaiting()
        {
            return instance.keepWaiting;
        }
    }


    public class CoroutineW : IEnumerator
    {
        IEnumerator instance;
        public CoroutineW(IEnumerator obj)
        {
            instance = obj;
        }

        public object Current
        {
            get
            {
                var cur = instance.Current;

                CustomYieldInstruction customYield = cur as CustomYieldInstruction;
                if (customYield != null)
                    return new CustomInstruction(customYield);

                return cur;
            }
        }

        public bool MoveNext()
        {
            return instance.MoveNext();
        }

        public void Reset()
        {
            instance.Reset();
        }
    }
}
