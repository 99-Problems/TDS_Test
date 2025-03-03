using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxFloor : UnitLogic
{
    public Transform floorTransform;
    public float pushForce;

    public int index { get; private set; }

    public Slider slider;
    public UIOpenAni hpAni;
    public float _aniTime = 1f;
    private bool isAni;

    private void Awake()
    {
        if(floorTransform == null)
            floorTransform = GetComponent<Transform>();
    }

    public virtual void Init(int _index)
    {
        index = _index;

        Reset();

        slider.maxValue = GetOriginalFullHp;
        slider.value = GetOriginalFullHp;

        OnDamageEvent += (damage) =>
        {
            if (IsDie)
                return;

            slider.value += damage;
            if (isAni && !hpAni.gameObject.activeSelf)
                isAni = false;

            if(!isAni)
            {
                isAni = true;
                hpAni.SetActive(true);
                hpAni.StartPlay();
                hpAni.SetActiveFalse(2f);
            }
            
        };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var monster = collision.GetComponent<MonsterLogic>();
        if (monster)
        {
            monster.UnitMove(Vector2.right * pushForce);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var monster = collision.GetComponent<MonsterLogic>();
        if (monster)
        {
            monster.UnitMove(Vector2.right * pushForce);
        }
    }
}
