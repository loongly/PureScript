using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourListener : MonoBehaviour
{
    public string ClassKey;
    public Action OnAwake;
    public Action OnStart;
    public Action OnUpdate;
    public Action OnDoEnable;
    public Action OnDoDisable;
    public Action OnDoDestroy;
    
    private void Awake()
    {
        OnAwake?.Invoke();
    }
    void Start()
    {
        OnStart?.Invoke();
    }
    void Update()
    {
        OnUpdate?.Invoke();
    }
    private void OnEnable()
    {
        OnDoEnable?.Invoke();
    }
    private void OnDisable()
    {
        OnDoDisable?.Invoke();
    }
    private void OnDestroy()
    {
        OnDoDestroy?.Invoke();
    }
}
