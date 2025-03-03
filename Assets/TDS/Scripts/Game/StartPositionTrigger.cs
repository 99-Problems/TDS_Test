using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StartPositionTrigger : BaseTrigger
{
    public bool isLookAtLeft = false;
#if UNITY_EDITOR
    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
#endif
    public override void Enter(UnitLogic _unit)
    {
    }
}
