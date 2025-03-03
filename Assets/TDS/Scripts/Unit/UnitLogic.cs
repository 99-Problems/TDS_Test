using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Data;
using UnityEngine.AI;
using TMPro;
using UniRx.Triggers;
using UniRx;
using Cysharp.Threading.Tasks;
using Data.Managers;
using static Data.Define;
using System.Linq;

public class UnitLogic : MonoBehaviour
{
    [LabelText("�ִ� ü��")]
    public long life = 100;
    [LabelText("���� ü��")]
    public long hp = 1; // ���� ���� HP ��ġ

    public long atk = 1;
    public long GetOriginalFullHp => life;

    [Title("���� �ɼ�", horizontalLine: true)]
    public int skillID = -1;
    [LabelText("���� ������")]
    public Vector3 size;
    [LabelText("���� ��� �ð�")]
    public float delayTime = 0;
    [LabelText("�ݺ� �ð�")]
    public float tick = 1;
    [LabelText("��� �ð�")]
    public float duration = 0.1f;
    [LabelText("Ŀ���� ���� ��ġ")]
    public bool isCustomPosition;
    [ShowIf("@isCustomPosition == true")]
    [LabelText("���� ��ġ")]
    public Vector3 offset;
    [LabelText("����ü")]
    public bool isMove;
    [ShowIf("@isMove")]
    public float projectileSpeed;

    protected bool initialized;

    [LabelText("������ ǥ�� ��ġ ����")]
    public Vector3 textPosition;

    [ReadOnly]
    public UnitLogic aggroTarget;
    public int UnitID;
    public Define.EUnitType unitType;

    protected bool _isDie;

    public Action<float> OnDamageEvent;
    public Action<UnitLogic> OnDieEvent;
    protected int fontTime;
    protected int fontInitTime;
    public int fontTimeMax = 30;
    protected float dieTime;
    public float dieDelay = 1f;

    [ShowInInspector]
    protected InGamePlayInfo playInfo;
    protected InGamePlayerInfo owner;
    public InGamePlayerInfo Owner => owner;
    protected UnitInfoScript Info;

    public virtual void Reset()
    {
        hp = GetOriginalFullHp;
        dieTime = 0;
    }
    public virtual void Clear()
    {

    }

    public virtual void FrameMove(float _deltaTime)
    {
       
    }

    public void Initialization(InGamePlayInfo _info, InGamePlayerInfo _owner, UnitInfoScript _unitInfo)
    {
        playInfo = _info;
        owner = _owner;
        Info = _unitInfo;
    }

    
#if LOG_ENABLE && UNITY_EDITOR
    internal string log;
#endif
    public long CalcDamage(Define.EDAMAGE_TYPE edamageType, UnitLogic _attacker, long _skillDamage,
        ref bool _isCritical, ref bool _retAvoid)
    {
        _retAvoid = false;
        float random = 0f;

#if LOG_ENABLE && UNITY_EDITOR
        log = "";
        log += $"{_attacker.name} -> {this.name}";
#endif

        long damage = 0;
        damage = _skillDamage;


#if LOG_ENABLE && UNITY_EDITOR
        log += $"{Environment.NewLine} ���� ������ : "+ $"{damage}".ToColor(Color.green);

        Debug.Log(log);
#endif
        return damage;
    }
    public virtual long TakeDamage(UnitLogic _attacker, Define.EDAMAGE_TYPE _damageType,
        long _skillDamage, ref bool _isCritical,
        ref bool _retAvoid)
    {
        long damage = 0;
        long calc = 0;

        damage = CalcDamage(_damageType, _attacker, _skillDamage, ref _isCritical, ref _retAvoid);
        
        if (!IsDie)
        {
            calc = damage < 0 ? 0 : AddHp(_attacker, -damage);
        }
        else
        {
            return 0;
        }
        if(playInfo)
        {
            playInfo.AddDamageParticle(this,
          _damageType,
          damage,
          _isCritical,
          _retAvoid,
          transform.position + new Vector3(0, (_isCritical ? fontTime * 0.15f : 0), 0) + textPosition,
          _attacker.transform.position.x < transform.position.x,
          -1);
        }
       
        return calc;
    }
    public virtual long AddHp(UnitLogic _effector, long _calcHp)
    {
        if (_calcHp < 0)
        {
            OnDamageEvent?.Invoke(_calcHp);
        }

        hp += _calcHp;
        if (hp <= 0)
        {
            _calcHp -= hp;
            hp = 0;
            OnDieEvent?.Invoke(this);
        }
        if(hp > GetOriginalFullHp)
        {
            hp = GetOriginalFullHp;
        }

        return _calcHp;
    }
    
    public virtual bool IsDie
    {
        get
        {
            return _isDie;
        }
        set
        {
            _isDie = value;
        }
    }
}
