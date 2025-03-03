using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;

public abstract class LogicBase : MonoBehaviour
{
    public abstract void FrameMove(float _delta);
}

public class ProjectileLogic : LogicBase
{
    [NonSerialized]
    public UnitLogic parentUnit;
    [NonSerialized]
    public InGamePlayerInfo owner;

    public LayerMask groundLayer;

    private float duration;
    [ShowInInspector]
    private float currentTime;
    private float tick;
    private float currentDelayTime;
    private float delayTime;
    private bool isMove;
    private float power;
    private Vector3 direction;

    [NonSerialized]
    private long damage;
    private Define.EDAMAGE_TYPE damageType;
    [NonSerialized]
    private int cur = 0;

    private int skillID;
    private IEnumerable<UnitLogic> enemy;
    private IEnumerable<UnitLogic> allow;
    List<UnitLogic> listTarget = new List<UnitLogic>(10);
    List<UnitLogic> listEffectTarget = new List<UnitLogic>(10);

    public float CurrentDelayTime => currentDelayTime;
    public float DelayTime => delayTime;
    public float DelayRatio => currentDelayTime / delayTime;

    private bool isHit;


    public virtual void Move(float _delta)
    {
        if (!isMove || !gameObject.activeSelf)
            return;

        transform.Translate(direction * power * _delta, Space.Self);
    }
    public override void FrameMove(float _delta)
    {
        if (currentDelayTime < delayTime)
        {
            currentDelayTime += _delta;
            return;
        }

        
        if (duration <= currentTime || parentUnit == null || (isMove && isHit))
        {
            owner.RemoveObject(this);
            return;
        }

        Move(_delta);

        currentTime += _delta;

        if(!isMove)
        {
            var i = (int)(currentTime / tick);
            if (cur > i)
            {
                return;
            }

            cur++;
        
            enemy = null;
            allow = null;
            listEffectTarget.Clear();
        }
        float totalDamage = 0;

        var targets = GetEnemy();
        if (damageType == Define.EDAMAGE_TYPE.PLAYER)
        {
            totalDamage += DamageToAggroTarget(parentUnit, damageType, targets.ToList(), damage, ref listEffectTarget);
        }
        else
        {
            totalDamage += DamageToTargets(parentUnit, damageType, targets.ToList(), damage, ref listEffectTarget);
        }

        if(totalDamage < 0)
            isHit = true;

        if(isHit && isMove)
        {
            listTarget.Clear();
        }
    }

    private long DamageToTargets(UnitLogic _owner
        , Define.EDAMAGE_TYPE _damageType
        , List<UnitLogic> _listTarget
        , long _skillDamage
        , ref List<UnitLogic> _listLogic)
    {
        long ret = 0;
        foreach (var target in _listTarget)
        {
            if (target && !target.IsDie)
            {
                bool isCritical = false;
                bool retAvoid = false;
                ret += target.TakeDamage(_owner, _damageType, _skillDamage, ref isCritical, ref retAvoid);

                if (!retAvoid)
                {
                    _listLogic?.Add(target);
                }
            }
        }   

        return ret;
    }

    private long DamageToAggroTarget(UnitLogic _owner
        , Define.EDAMAGE_TYPE _damageType
        , List<UnitLogic> _listTarget
        , long _skillDamage
        , ref List<UnitLogic> _listLogic)
    {
        long ret = 0;
        foreach (var target in _listTarget)
        {
            if (!target.IsDie && target == _owner.aggroTarget)
            {
                bool isCritical = false;
                bool retAvoid = false;
                ret += target.TakeDamage(_owner, _damageType, _skillDamage, ref isCritical, ref retAvoid);

                if (!retAvoid)
                {
                    _listLogic?.Add(target);
                }
                break;
            }
        }

        return ret;
    }

    
    private IEnumerable<UnitLogic> GetEnemy()
    {
        if (enemy == null)
        {
            enemy = listTarget.Where(target =>
            {
                switch (damageType)
                {
                    case Define.EDAMAGE_TYPE.PLAYER:
                        if (target is PlayerLogic || target is BoxFloor)
                            return false;
                        break;
                    case Define.EDAMAGE_TYPE.ENEMY:
                        if (target is MonsterLogic)
                            return false;
                        break;
                    default:
                        break;
                }
                if (target != null && !target.IsDie)
                    return true;

                return false;
            }).Distinct();
        }

        return enemy;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || gameObject.activeSelf == false)
            return;

        if(damageType == Define.EDAMAGE_TYPE.PLAYER)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
#endif
    public void Init(UnitLogic _unit, int _skillID,float _tick, float _delayTime, float _duration,
        long _damage, Define.EDAMAGE_TYPE _damageType, bool _isMove, Vector3 _dir, float _power = 0)
    {
        parentUnit = _unit;
        owner = _unit.Owner;
        skillID = _skillID;
        delayTime = _delayTime;
        duration = _duration;
        tick = _tick;
        damage = _damage;
        damageType = _damageType;
        isMove = _isMove;
        direction = _dir;
        power = _power;

        listTarget.Clear();
        listEffectTarget.Clear();
        currentTime = 0;
        currentDelayTime = 0;
        cur = 0;
        isHit = false;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        UnitLogic unitLogic = collision.GetComponent<UnitLogic>();
        
        if(unitLogic != null)
        {
            listTarget.Add(unitLogic);
        }
        else if(isMove)
        {
            if((groundLayer.value & (1 << collision.gameObject.layer)) != 0)
            {
                owner.RemoveObject(this);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        UnitLogic unitLogic = collision.GetComponent<UnitLogic>();

        if (unitLogic != null)
        {
            listTarget.Add(unitLogic);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isMove)
            return;

        UnitLogic unitLogic = collision.GetComponent<UnitLogic>();
        if (unitLogic != null)
        {

            listTarget.Remove(unitLogic);
        }
    }
}
