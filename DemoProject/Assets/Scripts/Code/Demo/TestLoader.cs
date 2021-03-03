using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

class TestLoader : MonoBehaviour
{
    private void Start()
    {
        Debug.LogError("== TestLoader Start");
        StartCoroutine(ResourcesLoad());
    }


    IEnumerator ResourcesLoad()
    {
        var resReq = Resources.LoadAsync<GameObject>("test/Capsule");
        yield return resReq;
        Debug.LogError("### Resources test/Capsule done. ");
        var obj = Instantiate(resReq.asset) as GameObject;
        if(obj != null)
        {
            obj.transform.position = Vector3.one;
        }

        var textReq = Resources.LoadAsync<TextAsset>("test/test_str");
        textReq.completed += (opt) =>
        {
            Debug.LogError("### Resources test/test_str done. ");
            var textAsset = textReq.asset as TextAsset;
            Debug.LogError("text:" + textAsset.text);
        };
    }

    private void OnDestroy()
    {
        Debug.LogError("== TestLoader OnDestroy ");
    }

}

