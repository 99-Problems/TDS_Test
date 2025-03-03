using Data;
using Unity.Linq;
using UnityEngine;

public class PlayerLogic : UnitLogic
{
    public float attackRate = 1f;      // 공격 속도
    private float _deltaAtk;
    public float attackRange = 2f;     // 공격 범위
    public LayerMask enemyLayer;       // 적의 레이어

    public TruckLogic truck; 
    public float pushForce { get; set; }

    public virtual void Init()
    {
        initialized = true;

        foreach (var mit in gameObject.Ancestors())
        {
            var _truck = mit.GetComponent<TruckLogic>();
            if(_truck != null)
            {
                truck = _truck;
                break;
            }
        }

    }

    public override void FrameMove(float _deltaTime)
    {
        if(_deltaAtk > attackRate)
        {
            AutoAim();
            OnAttack();
            _deltaAtk = 0;
        }

        _deltaAtk += _deltaTime;
    }

    public void AutoAim()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        float nearestDistance = float.MaxValue;
        aggroTarget = null;
        foreach (Collider2D enemy in enemies)
        {
            var logic = enemy.GetComponent<UnitLogic>();
            if (logic == null || logic.IsDie)
                continue;

            var distance = Vector2.Distance(transform.position, enemy.transform.position);
            if(distance < nearestDistance)
            {
                nearestDistance = distance;
                aggroTarget = logic;
            }
        }
    }

    public void OnAttack()
    {
        Vector2 shootDirection = Vector2.right;

        if (aggroTarget != null)
        {
            shootDirection = (aggroTarget.transform.position - transform.position).normalized; 
        }
        else
        {
            return;
        }

        this.AddDamage(skillID, offset + transform.position,
            -1, size, tick, delayTime, duration, atk, Define.EDAMAGE_TYPE.PLAYER, isMove, shootDirection, projectileSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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
