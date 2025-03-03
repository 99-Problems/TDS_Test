using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Data;

public static class UnitLogicExtension
{
    public static async void AddDamage(this UnitLogic _unit
        , int _skillID
       , Vector3 _pos
       , float moveTime
       , Vector3 _size, float _tick,
       float _delayTime, float _duration, long _damage, Define.EDAMAGE_TYPE _damageType, bool _isMove,Vector3 _dir, float _power = 0)
    {
        ProjectileLogic collider = null;
        switch (_damageType)
        {
            case Define.EDAMAGE_TYPE.PLAYER:
                collider = Managers.Pool.PopBulletCollider();
                if (collider == null)
                    collider = await Managers.Pool.CreateBulletCollider();
                break;
            case Define.EDAMAGE_TYPE.ENEMY:
                collider = Managers.Pool.PopBoxCollider();
                if (collider == null)
                    collider = await Managers.Pool.CreateBoxCollider();
                break;
            default:
                break;
        }
        

        if(_size != Vector3.zero)
        {
            collider.transform.localScale = new Vector3(1 * _size.x, 1 * _size.y, 1 * _size.z);
        }

//#if UNITY_EDITOR
//        collider.name = $"{_unit.name}";
//#endif
        collider.transform.position = _pos;

        collider.Init(_unit, _skillID, _tick, _delayTime, _duration, _damage, _damageType, _isMove, _dir,_power);
        _unit.Owner.AddObject(collider);
    }

}