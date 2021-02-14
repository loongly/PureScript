using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehaviourScript : MonoBehaviour
{
    public GameObject testObj;
    public Transform testTrans;
    private Vector3 moveTarget;
    bool firsttest = true;
    // Start is called before the first frame update
    void Start()
    {
        testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testTrans = testObj.transform;
        moveTarget = new Vector3(1, 0, 0);

        firsttest = true;

        Debug.LogError("==== aa: " + this.name);

        StartCoroutine(RunCoroutineTest("teststr",1));

        Application.focusChanged += OnFocus;
        DebugHelper.InitLog(true);


        Debug.LogError("==== persistentDataPath: " + Application.streamingAssetsPath);
        Debug.LogError("==== isPlaying: " + Application.isPlaying);
        Debug.LogError("==== platform: " + Application.platform);
        Debug.LogError("==== tag: " + gameObject.tag);
        Application.OpenURL("http://www.baidu.com");
        Application.targetFrameRate = 10;
        
    }

    void OnFocus(bool focus)
    {
        Debug.LogError("==focus: " + focus);
    }

    // Update is called once per frame
    void Update()
    {
        var curPos = testTrans.position;
        if (curPos.x > 6)
            moveTarget = Vector3.left;
        else if (curPos.x < -6)
            moveTarget = Vector3.right;

        //testTrans.Rotate(Vector3.up, 1);
        testTrans.position = curPos + moveTarget * Time.deltaTime* 3;

        if(firsttest)
        {
            firsttest = false;
            TestGetMonoBehaviour();
        }
    }


    void TestGetMonoBehaviour()
    {
        System.GC.Collect();
        Debug.LogError("== TestGetMonoBehaviour == " + testObj.name);
        var scrpit = gameObject.GetComponent<TestBehaviourScript>();
        System.GC.Collect();
        scrpit.Print();
        Debug.LogError("== isequl: " + (scrpit == this));
        System.GC.Collect();
    }

    public void Print()
    {
        Debug.LogError("==== print: " + testObj.name);
    }

    IEnumerator RunCoroutineTest(string str, int a = 1)
    {
        Debug.LogError("===== RunCoroutineTest ===== " + str);
        int testValue = 2;
        yield return null;

        Debug.LogError("FFF");
        
        yield return StartCoroutine(TestSecendEnumerator());


        Debug.LogError("00" + str);
        yield return new WaitForEndOfFrame();
        //var resReq = Resources.LoadAsync("test/res");
        //yield return resReq;

        Debug.LogError("AAA" + str);

        yield return new WaitForSeconds(2.0f);


        int tv = 2;
        Debug.LogError("BBB");
        float end = Time.time + 3.0f;
        yield return new WaitUntil(() => Time.time > end);

        Debug.LogError("CCC");
        yield return new WaitForSeconds(3.0f);
        yield return 0;

        Debug.LogError("DDD");
        yield return new CostomWait(Time.time + 3.0f);

        Debug.LogError("EEE");
        yield return null;

        yield return StartCoroutine(TestSecendEnumerator());
        Debug.LogError("FFF");
        yield return null;
        Debug.LogError("GG");
    }

    IEnumerator TestSecendEnumerator()
    {
        Debug.LogError("TestSecendEnumerator >>");
        yield return new WaitForSeconds(1.0f);
        Debug.LogError("aaaa");
       // yield return null;
        Debug.LogError("bbbb");
        yield return new WaitForSeconds(5.0f);
        Debug.LogError("ccc");
    }

    public class CostomWait : CustomYieldInstruction
    {
        float waitTime;
        public CostomWait(float time)
        {
            waitTime = time;
        }
        public override bool keepWaiting
        {
            get
            {
                return Time.time < waitTime;
            }
        }
    }
}
