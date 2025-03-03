using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TruckLogic : MonoBehaviour
{
    public Transform testStopTransform;
    public float testDistance=8f;

    public List<BoxFloor> floors = new List<BoxFloor>();  
    public PlayerLogic player;  
    public float moveSpeed = 2f;  
    public float checkDistance = 1.5f;  
    public int maxBlockEnemies = 3;
    public float slowPerUnit = 0.2f;
    public Vector2 castEnemySize;
    public float rayAngle;
    public LayerMask enemyLayer; 
    public float pushForce = 0.3f;  

    private int currentFloorIndex = 0;  
    private bool isMove = true;  

    public Vector2 GetSpeed => _speed;
    public int moveDirection = 1;
    protected bool initialized;
    protected Vector2 _speed;
    protected Vector2 _newPosition;

    private float boxSizeY;

    void Start()
    {
        Init();
    }
    public virtual void Init()
    {
        initialized = true;
        foreach (var item in floors)
        {
            item.pushForce = pushForce;
        }
        
        player.pushForce = pushForce;
        var box = floors.FirstOrDefault().gameObject.GetOrAddComponent<BoxCollider2D>();
        boxSizeY = box.size.y;

        for (int i = 0; i < floors.Count; i++)
        {
            floors[i].Init(i);
            floors[i].OnDieEvent += (_logic) =>
            {
                BoxFloor logic = _logic as BoxFloor;
                if(logic)
                {
                    TowerBreak(logic.index);
                }
            };
        }
    }
    private void Update()
    {
        FrameMove(Time.deltaTime);
    }

    public virtual void FrameMove(float _deltaTime)
    {
        if(!initialized)
            return;

        if (Managers.Time.GetGameSpeed() <= 0f)
        {
            return;
        }

        if (IsStop())
            return;

        FrameInit(_deltaTime);
        UnitMove(_newPosition);
    }
    public bool IsStop()
    {
        var _dist = Vector2.Distance(testStopTransform.position, transform.position);
        return _dist < testDistance;
    }
    protected virtual void FrameInit(float _deltaTime)
    {
        _speed.x = moveDirection * moveSpeed;
        _newPosition = _speed * _deltaTime;
    }

    protected virtual void UnitMove(Vector2 pos)
    {
        var enemyCount = GetEnemiesCount();
        isMove = enemyCount < maxBlockEnemies;
        if (!isMove)
            return;

        var speedPer = Mathf.Max(0, 1 - enemyCount * slowPerUnit);

        transform.Translate(pos*speedPer, Space.Self);
    }


    public int GetEnemiesCount()
    {
        RaycastHit2D[] hits = BoxCast((Vector2)transform.position + _newPosition, castEnemySize, rayAngle, Vector2.right, checkDistance, enemyLayer);
        return hits.Length;  // 감지된 적 개수 반환
    }

    public RaycastHit2D[] BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask)
    {
        return Physics2D.BoxCastAll(origin, size, angle, direction, length, mask);
    }

    [Button]
    void TowerBreak(int _index)
    {
        var floor = floors.Find(_ => _.index == _index);
        if (floor == null || floor.IsDie)
            return;

        floor.gameObject.SetActive(false);
        Debug.Log($"{_index + 1}층 파괴!");

        foreach (var item in floors)
        {
            if (item.IsDie)
                continue;

            if(item == floor)
            {
                item.IsDie = true;
                item.gameObject.SetActive(false);
                continue;
            }

            if(item.index > floor.index)
                item.transform.Translate(Vector2.down * boxSizeY);
        }
        player.transform.Translate(Vector2.down * boxSizeY);
    }

    void OnDrawGizmos()
    {
        // 감지 거리 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * checkDistance);
    }
}
