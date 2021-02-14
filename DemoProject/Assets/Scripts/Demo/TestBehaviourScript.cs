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
        testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testTrans = testObj.transform;
        moveTarget = new Vector3(1, 0, 0);
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
        testTrans.position = curPos + moveTarget * Time.deltaTime;
    }
}
