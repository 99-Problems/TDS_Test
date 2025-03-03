using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEditor;
using UnityEngine;

[HideMonoScript]
[ExecuteInEditMode]
public abstract class BaseTrigger : MonoBehaviour
{
    [ShowInInspector]
    [ReadOnly]
    protected bool isDone;

    [NonSerialized]
    internal IGameData gameData;

    public virtual bool IsDone => isDone;

    public virtual UniTask Load()
    {
        return UniTask.CompletedTask;
    }

    public virtual void Awake()
    {
        var collider = GetComponent<Collider>();
        if (collider)
        {
            collider.enabled = false;
            collider.isTrigger = true;
        }
    }
#if UNITY_EDITOR
    public virtual void OnDrawGizmos()
    {
        if (IsDone) return;
        var collider = GetComponent<SphereCollider>();
        if (collider == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collider.radius);
    }
#endif
    public virtual void Done()
    {
        isDone = true;
        var collider = GetComponent<Collider>();
        if (collider)
            collider.enabled = false;
    }

    public abstract void Enter(UnitLogic _unit);

 
    public void Init()
    {
        var collider = GetComponent<Collider>();
        gameData.OnLoadingComplete.Subscribe(_1 =>
        {
            if (collider)
                collider.enabled = true;
        }).AddTo(this);
    }

    //public virtual int GetCurrentEventProgress()
    //{
    //    return IsDone ? 1 : 0;
    //}

    //public virtual int GetMaxEventProgress()
    //{
    //    return 1;
    //}

}
