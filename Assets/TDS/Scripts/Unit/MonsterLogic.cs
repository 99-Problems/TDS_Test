using UnityEngine;
using Data;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using Data.Managers;

public class MonsterLogic : UnitLogic
{
    [ReadOnly]
    public Define.EMONSTER_STATE currentState = Define.EMONSTER_STATE.RUN;

    public float moveSpeed = 2f;
    public float jumpHeight = 2f;
    public Vector2 jumpSpeed = new Vector2(2f, 3f);
    public float jumpCool = 2f;
    private float curJumpCool;
    public float attackRange = 0.6f;
    public int columnIndex = 0;

   

    public override bool IsDie
    {
        get
        {
            _isDie = hp <= 0 || currentState == Define.EMONSTER_STATE.DIE
                                || currentState == Define.EMONSTER_STATE.DESTROY;
            return _isDie;
        }
        set
        {
            _isDie = hp <= 0 || currentState == Define.EMONSTER_STATE.DIE
                                || currentState == Define.EMONSTER_STATE.DESTROY;
        }
    }

    [Space(15)]
    public LayerMask unitLayer;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    [ReadOnly]
    private bool isJumping = false;
    [ReadOnly]
    private bool isFalling = false;
    [ReadOnly]
    private bool isPush;

    private float curGroundY;

    public float gravity = -9.8f;
    [ShowInInspector]
    public Vector2 GetSpeed => _speed;
    [LabelText("이동 방향")][Tooltip("이동 방향: (좌/우)")]
    public Define.EUNIT_DIRECTION direction = Define.EUNIT_DIRECTION.LEFT;
    public bool _gravityActive = true;
    protected float _currentGravity = 0;
    protected Vector2 _speed;
    protected Vector2 _newPosition;
    protected Vector2 _prevPosition = Vector2.zero;
    protected float _initJumpHeight;
    protected BoxCollider2D _boxCollider;

    protected Vector2 _boundsTopLeftCorner;
    protected Vector2 _boundsBottomLeftCorner;
    protected Vector2 _boundsTopRightCorner;
    protected Vector2 _boundsBottomRightCorner;
    protected Vector2 _boundsCenter;
    protected Vector2 _bounds;
    protected float _boundsWidth;
    protected float _boundsHeight;

    protected Vector2 _horizontalRayCastFromBottom = Vector2.zero;
    protected Vector2 _horizontalRayCastToTop = Vector2.zero;


    protected RaycastHit2D[] _sideHitsStorage;


    protected const float _obstacleHeightTolerance = 0.05f;

    [Tooltip("the number of rays cast horizontally")]
    public int NumberOfHorizontalRays = 8;
    public bool drawGizmos = true;
    public static bool DebugDrawEnabled;
    public float horizontalRayLength = 5f;
    public float verticalRayLength = 2f;
    public float pushForce = 1.5f;
    public float pushCool = 1f;
    public float pushDelay = 0.2f;
    

    private MonsterLogic curUnderMonster;

    public virtual float Width()
    {
        return _boundsWidth;
    }

    public virtual float Height()
    {
        return _boundsHeight;
    }

    public virtual Vector2 Bounds
    {
        get
        {
            _bounds.x = _boundsWidth;
            _bounds.y = _boundsHeight;
            return _bounds;
        }
    }
    public Animator anim { get; private set; }

    private void Awake()
    {
#if UNITY_EDITOR
        DebugDrawEnabled = drawGizmos;
#endif
    }

    public virtual void Init(int _index)
    {
        initialized = true;
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        SetRaysParameters();
        pushForce = _boundsWidth + moveSpeed * Time.deltaTime;
        curGroundY = float.MinValue;
        columnIndex = _index;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        
        curJumpCool = 0;
        SetState(Define.EMONSTER_STATE.RUN);
    }
    public override void Clear()
    {
        base.Clear();
        SetState(Define.EMONSTER_STATE.RUN);
    }

    public override void FrameMove(float _deltaTime)
    {
        if (!initialized)
            return;

        if (Managers.Time.GetGameSpeed() <= 0f)
        {
            return;
        }

        FrameInit(_deltaTime);
        ApplyGravity(_deltaTime);
        IsGround();
        SetRaysParameters();
        CastRaysToTheSides(direction.GetDirecton(), unitLayer, Color.green);
        DetectAttackRange();
        ActionJump(_deltaTime);

        switch (currentState)
        {
            case Define.EMONSTER_STATE.RUN:
                UnitMove(_newPosition);
                break;
            case Define.EMONSTER_STATE.ATTACK:
                if (isJumping || isFalling)
                {
                    UnitMove(Vector2.up * _newPosition.y);
                }
                break;
            case Define.EMONSTER_STATE.DIE:
                DeadTick(_deltaTime);
                break;
            case Define.EMONSTER_STATE.DESTROY:
                break;
            default:
                break;
        }

        if (IsDie)
        {
            SetState(Define.EMONSTER_STATE.DIE);
            return;
        }

    }


    public void Jump()
    {
        isJumping = true;
        _initJumpHeight = jumpHeight + transform.position.y;
    }

    public void ActionJump(float _deltaTime)
    {
        if (isJumping == false)
        {
            return;
        }

        curJumpCool += _deltaTime;
        if (direction.GetDirecton().x < 0 && jumpSpeed.x > 0)
            jumpSpeed.x *= -1;
        _newPosition += jumpSpeed * _deltaTime;
        if (transform.position.y + _newPosition.y > _initJumpHeight + jumpHeight)
        {
            isJumping = false;
            curJumpCool = 0;
        }
    }

    /// <summary>
    /// 아래에 지면이나 다른 유닛이 있는지 확인하여, 없으면 추락 상태로 전환
    /// </summary>
    public bool IsGround()
    {
        bool isHit = false;
        var rayGround = RayCast((Vector2)transform.position + _boxCollider.offset - _prevPosition, Vector2.down, verticalRayLength, groundLayer, Color.yellow, ref isHit, DebugDrawEnabled);
        if (isHit)
        {
            isFalling = false;
            curGroundY = rayGround.point.y;
            return isHit;
        }
        var rayHit = RayCast((Vector2)transform.position + _boxCollider.offset - _prevPosition, Vector2.down , verticalRayLength, unitLayer, Color.yellow, ref isHit, false);
        if (isHit && rayHit)
        {
            if (isJumping)
            {
                isFalling = false;
                return isHit;
            }

            //isFalling = true;

            var monster = rayHit.collider.GetComponent<MonsterLogic>();
            if (monster && monster.columnIndex == columnIndex && !isPush)
            {
                PushTrigger(monster);
                //monster.UnitMove(Vector2.right * monster.pushForce);
            }
        }
        else
        {
            if (!isJumping)
            {
                isFalling = true;
            }
        }

        return isHit;
    }


    public void SetState(Define.EMONSTER_STATE _state)
    {
        currentState = _state;
        anim.SetBool("IsIdle", _state == Define.EMONSTER_STATE.RUN);
        anim.SetBool("IsAttacking", _state == Define.EMONSTER_STATE.ATTACK);
        anim.SetBool("IsDead", _state == Define.EMONSTER_STATE.DIE ||
                               _state == Define.EMONSTER_STATE.DESTROY);

    }

    public virtual void UnitMove(Vector2 pos)
    {
        if(transform.position.y + pos.y < curGroundY)
        {
            pos.y = curGroundY - transform.position.y;
        }
        _prevPosition = pos;
        transform.Translate(pos, Space.Self);
    }

    public bool logTest;
    public virtual async UniTask UnitPush(Vector2 pos)
    {
        var initPos = transform.position;
        await UniTask.Delay(TimeSpan.FromSeconds(pushDelay));
        if (logTest)
            Debug.ColorLog($"{this.name} pushed");

        UnitMove(pos);
    }

    protected virtual void ApplyGravity(float _deltaTime)
    {
        if (isFalling == false)
        {
            return;
        }

        _currentGravity = gravity;

        if (_gravityActive)
        {
            _newPosition.y += _currentGravity * _deltaTime;
        }
    }
    public virtual void SetGravity(bool isOn)
    {
        _gravityActive = isOn;
    }

    protected virtual void FrameInit(float _deltaTime)
    {
        _speed = Vector2.zero;
        _speed.x = direction.GetDirecton().x * moveSpeed;
        _newPosition = _speed * _deltaTime;
    }

    public virtual void SetRaysParameters()
    {
        float top = _boxCollider.offset.y + (_boxCollider.size.y / 2f);
        float bottom = _boxCollider.offset.y - (_boxCollider.size.y / 2f);
        float left = _boxCollider.offset.x - (_boxCollider.size.x / 2f);
        float right = _boxCollider.offset.x + (_boxCollider.size.x / 2f);

        _boundsTopLeftCorner.x = left;
        _boundsTopLeftCorner.y = top;

        _boundsTopRightCorner.x = right;
        _boundsTopRightCorner.y = top;

        _boundsBottomLeftCorner.x = left;
        _boundsBottomLeftCorner.y = bottom;

        _boundsBottomRightCorner.x = right;
        _boundsBottomRightCorner.y = bottom;

        _boundsTopLeftCorner = transform.TransformPoint(_boundsTopLeftCorner);
        _boundsTopRightCorner = transform.TransformPoint(_boundsTopRightCorner);
        _boundsBottomLeftCorner = transform.TransformPoint(_boundsBottomLeftCorner);
        _boundsBottomRightCorner = transform.TransformPoint(_boundsBottomRightCorner);
        _boundsCenter = _boxCollider.bounds.center;

        _boundsWidth = Vector2.Distance(_boundsBottomLeftCorner, _boundsBottomRightCorner);
        _boundsHeight = Vector2.Distance(_boundsBottomLeftCorner, _boundsTopLeftCorner);
    }

    protected virtual void DetectAttackRange()
    {
        if (CastRaysToTheSides(direction.GetDirecton(), playerLayer, Color.red, attackRange) == false && !IsDie)
        {
            SetState(Define.EMONSTER_STATE.RUN);
        }
    }

    protected virtual bool CastRaysToTheSides(Vector3 raysDirection, LayerMask layer, Color rayColor, float addLength = 0)
    {
        _horizontalRayCastFromBottom = (_boundsBottomRightCorner + _boundsBottomLeftCorner) / 2;
        _horizontalRayCastToTop = (_boundsTopLeftCorner + _boundsTopRightCorner) / 2;
        _horizontalRayCastFromBottom = _horizontalRayCastFromBottom + (Vector2)transform.up * _obstacleHeightTolerance;
        _horizontalRayCastToTop = _horizontalRayCastToTop - (Vector2)transform.up * _obstacleHeightTolerance;

        float horizontalRayLength = Mathf.Abs(_speed.x * Time.deltaTime) + _boundsWidth / 2 + addLength;
        

        if (_sideHitsStorage.IsNullOrEmpty())
        {
            _sideHitsStorage = new RaycastHit2D[NumberOfHorizontalRays];
        }
        bool isHit = false;
       
        
        for (int i = 0; i < NumberOfHorizontalRays; i++)
        {
            Vector2 rayOriginPoint = Vector2.Lerp(_horizontalRayCastFromBottom, _horizontalRayCastToTop, (float)i / (float)(NumberOfHorizontalRays - 1));
            _sideHitsStorage[i] = RayCast(rayOriginPoint, raysDirection, horizontalRayLength, layer, rayColor, ref isHit, DebugDrawEnabled);

            if (isHit && (_sideHitsStorage[i].distance > 0 || i == NumberOfHorizontalRays - 1))
            {
                if (layer == unitLayer)
                {
                    var logic = _sideHitsStorage[i].collider.GetComponent<MonsterLogic>();
                    if (logic == null || logic.columnIndex != this.columnIndex)
                    {
                        continue;
                    }
                    if (!isJumping && !isPush && curJumpCool <= jumpCool)
                    {
                        if (logic.isJumping)
                            continue;
                        Jump();
                        break;
                    }
                }
                else if (layer == playerLayer)
                {
                    if (!IsDie && currentState != Define.EMONSTER_STATE.ATTACK)
                    {
                        var logic = _sideHitsStorage[i].collider.GetComponent<UnitLogic>();
                        if(logic == null || logic.IsDie)
                        {
                            continue;
                        }
                        aggroTarget = logic;
                        if(logic is PlayerLogic)
                        {
                            isJumping = false;
                            curJumpCool = 0;
                        }

                        SetState(Define.EMONSTER_STATE.ATTACK);
                        break;
                    }
                }

            }
        }
        
        return isHit;
    }

    public RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color, ref bool isHit, bool drawGizmo = false)
    {
        if (drawGizmo && DebugDrawEnabled)
        {
            Debug.DrawRay(rayOriginPoint, rayDirection * rayDistance, color);
        }
        var rays = Physics2D.RaycastAll(rayOriginPoint, rayDirection, rayDistance, mask);
        foreach (var ray in rays)
        {
            if (ray.collider == _boxCollider)
                continue;

            isHit = true;

            return ray;
        }

        isHit = false;

        return Physics2D.Raycast(rayOriginPoint, rayDirection, rayDistance, mask);
    }

    public RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, ref bool isHit)
    {
        {
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            var color = Color.blue;
            Vector3[] points = new Vector3[8];

            float halfSizeX = size.x / 2f;
            float halfSizeY = size.y / 2f;

            points[0] = rotation * (origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)); // top left
            points[1] = rotation * (origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)); // top right
            points[2] = rotation * (origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)); // bottom right
            points[3] = rotation * (origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)); // bottom left

            points[4] = rotation * ((origin + Vector2.left * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top left
            points[5] = rotation * ((origin + Vector2.right * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top right
            points[6] = rotation * ((origin + Vector2.right * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom right
            points[7] = rotation * ((origin + Vector2.left * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom left

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);

            Debug.DrawLine(points[4], points[5], color);
            Debug.DrawLine(points[5], points[6], color);
            Debug.DrawLine(points[6], points[7], color);
            Debug.DrawLine(points[7], points[4], color);

            Debug.DrawLine(points[0], points[4], color);
            Debug.DrawLine(points[1], points[5], color);
            Debug.DrawLine(points[2], points[6], color);
            Debug.DrawLine(points[3], points[7], color);

        }
        var rays = Physics2D.BoxCastAll(origin, size, angle, direction, length, mask);
        foreach (var ray in rays)
        {
            if (ray.collider == _boxCollider)
                continue;

            isHit = true;
        }
        return rays;
    }
    public RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, ref bool isHit)
    {
        var rays = Physics2D.BoxCastAll(origin, size, angle, direction, length, mask);
        foreach (var ray in rays)
        {
            if (ray.collider == _boxCollider)
                continue;

            isHit = true;

            return ray;
        }

        isHit = false;

        return Physics2D.BoxCast(origin, size, angle, direction, length, mask);
    }
    public void DeadTick(float _deltaTime)
    {
        dieTime += _deltaTime;
        if (dieTime >= dieDelay)
        {
            SetState(Define.EMONSTER_STATE.DESTROY);
            gameObject.SetActive(false);

            Managers.Pool.PushUnit(Info.assetPath, Info.prefabName, this);
        }
    }

    public void OnAttack()
    {
        //if (aggroTarget == null)
        //    return;

        this.AddDamage(skillID, offset + transform.position + direction.GetDirecton() * Bounds.x + Vector3.up * Bounds.y/2,
            -1, size, tick, delayTime, duration, atk, Define.EDAMAGE_TYPE.ENEMY, isMove, direction.GetDirecton(),projectileSpeed);
    }

    public async UniTask PushTrigger(MonsterLogic logic)
    {
        if(logic.isPush)
        {
            return;
        }
        isPush = true;
        logic.isPush = true;
        if (logTest)
            Debug.ColorLog($"{this.name} push {logic.name}");
        await logic.UnitPush(Vector2.right * logic.pushForce);
        await UniTask.DelayFrame(2);
        isPush = false;
        await UniTask.Delay(TimeSpan.FromSeconds(pushCool));
        logic.isPush = false;
    }
}
