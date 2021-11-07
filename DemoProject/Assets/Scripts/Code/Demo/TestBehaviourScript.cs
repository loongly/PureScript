using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehaviourScript : MonoBehaviour
{
    public GameObject testObj;
    public Transform testTrans;
    private Vector3 moveTarget;
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError($"TestBehaviourScript: Start");


        testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testTrans = testObj.transform;
        moveTarget = new Vector3(1, 0, 0);

        Debug.LogError($"==== name: {this.name} tag:{gameObject.tag}" );

        StartCoroutine(RunCoroutineTest("teststr",1));
        StartCoroutine(StartTestGC());
        TestGetComponent();
        Debug.LogError($"TestBehaviourScript: Start End");
    }
    
    // Update is called once per frame
    void Update()
    {
        var curPos = testTrans.position;
        if (curPos.x > 6)
            moveTarget = Vector3.left;
        else if (curPos.x < -6)
            moveTarget = Vector3.right;

        testTrans.Rotate(Vector3.up, 1);
        testTrans.position = curPos + moveTarget * Time.deltaTime* 3;
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
        System.GC.Collect();

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


    IEnumerator StartTestGC()
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.AddComponent<TestGC>().Info = "Test1";
        obj.AddComponent<TestGC>().Info = "Test2";

        yield return new WaitForSeconds(3.0f);
        Destroy(obj);

        yield return new WaitForSeconds(1.0f);
        System.GC.Collect();
    }

    private void OnDestroy()
    {
        Debug.LogError("== TestBehaviourScript OnDestroy");
    }

    ~TestBehaviourScript()
    {
        Debug.LogError("~TestBehaviourScript");
    }

    void TestGetComponent()
    {
        Debug.LogError("== TestGetMonoBehaviour == " + testObj.name);
        var scrpit = gameObject.GetComponent<TestBehaviourScript>();
        Debug.LogError("== isequl: " + (scrpit == this));

        var cps = gameObject.GetComponents<MonoBehaviour>();
        foreach (var cp in cps)
        {
            Debug.LogError("cp: " + cp.GetType().Name);
        }
    }
}
