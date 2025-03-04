using Cysharp.Threading.Tasks;
using Data;
using Data.Managers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;

public struct IngamePlayData
{
    public float currentTime;
    public float limitTime;
}

public class InGamePlayInfo : MonoBehaviour
{
    public enum EPLAY_STATE
    {
        SORT,
        READY,
        PLAY,
        PAUSE,
        STOP,
        END,
    }
    public ReactiveProperty<EPLAY_STATE> playState = new ReactiveProperty<EPLAY_STATE>(EPLAY_STATE.SORT);

    protected IGameData gameData;
    private GameObject particleParentObj;
    public GameObject GetParticleParentObject()
    {
        if (particleParentObj == null)
        {
            particleParentObj = new GameObject("Particle");
            particleParentObj.transform.SetParent(transform);
        }

        return particleParentObj;
    }

    private IStageClearCondition condition;
    [HideInInspector]
    public List<InGamePlayerInfo> listPlayer = new List<InGamePlayerInfo>();

    public bool isLastEvent { get; private set; } = false;

    public IngamePlayData playData;
    private bool isStop;
    

    public ConcurrentBag<DamageParticleData> damageParticleInfo = new ConcurrentBag<DamageParticleData>();


    private void Start()
    {
        gameData = Managers.Scene.CurrentScene as IGameData;

        gameData.OnLoadingComplete.Subscribe(_ =>
        {

        }).AddTo(this);
    }

    private void Update()
    {
        SpawnDamageParticle();
    }
    public void Init()
    {
        playData.limitTime = GameSceneInit.playTime;
        Clear();
    }

    public virtual void FrameMove(float _deltaTime)
    {
        if (playState.Value != EPLAY_STATE.END)
        {
            if (IsEndCondition() || (condition != null && condition.IsStageEndCondition()))
            {
                playState.Value = EPLAY_STATE.END;
                //Managers.Time.SetGameSpeed(1);
                
                return;
            }
        }
        switch (playState.Value)
        {
            case EPLAY_STATE.SORT:
                return;
            case EPLAY_STATE.READY:
                break;
            case EPLAY_STATE.PLAY:
                playData.currentTime = Math.Min(playData.currentTime + _deltaTime, playData.limitTime);
               
                break;
            case EPLAY_STATE.PAUSE:
            case EPLAY_STATE.STOP:
                break;
            case EPLAY_STATE.END:
                return;
            default:
                break;
        }

        foreach (var mit in listPlayer)
        {
            mit.FrameMove(_deltaTime);
        }
    }

    public void EndGame()
    {
        foreach (var player in listPlayer)
        {
            foreach (var unit in player.listUnit)
            {
                unit.Clear();
            }
        }
    }
    public void Clear()
    {
    }

    internal bool IsStageEndCondition()
    {
        return playState.Value == EPLAY_STATE.END;
    }

    public void SetStageClearCondition(IStageClearCondition _condition)
    {
        condition = _condition;
    }

    public void JoinPlayers(IEnumerable<InGamePlayerInfo> players)
    {
        foreach (var mit in players)
        {
            mit.transform.SetParent(gameObject.transform);
            mit.Entry();
            listPlayer.Add(mit);
        }
    }

    public async UniTask SetStageReady()
    {
        await UniTask.WaitUntil(() => IngameLoadingImage.instance != null ? IngameLoadingImage.instance.isloadingComplete : true);
        playState.Value = EPLAY_STATE.READY;

        StageStart();
    }
    public void StageStart()
    {
        playState.Value = EPLAY_STATE.PLAY;


        Debug.ColorLog("게임 시작", Color.green);
    }

    public bool isEnd = false;
    protected virtual bool IsEndCondition()
    {
        if (!isEnd)
            return false;

#if UNITY_EDITOR
        if (Managers.isinfinityMode) return false;
#endif
        return IsAnyPlayerDie() || playData.currentTime >= playData.limitTime || isStop;
    }

    internal bool IsAnyPlayerDie()
    {
        foreach (var player in listPlayer)
        {
            if(player.IsPlayerDie())
                return true;
        }
        return false;
    }

    public void AddDamageParticle(UnitLogic _unitLogic, Define.EDAMAGE_TYPE _type, long _calcHp, bool _isCritical, bool _isAvoid, Vector3 position, bool isMoveRight, long accountID)
    {
        damageParticleInfo.Add(new DamageParticleData
        {
            type = _type,
            calcHp = _calcHp,
            isCritical = _isCritical,
            isAvoid = _isAvoid,
            position = position,
            isMoveRight = isMoveRight,
            accountID = accountID,
        });
    }

    private void SpawnDamageParticle()
    {
        while (!damageParticleInfo.IsEmpty)
        {
            if (damageParticleInfo.TryTake(out var info))
            {
                var clone = Managers.Pool.PopDamageParticle();
                if (clone == null)
                    return;
                clone.transform.SetParent(GetParticleParentObject().transform);
                if (info.isAvoid)
                {
                    clone.transform.position = new Vector3(info.position.x, info.position.y + 0.1f, info.position.z);
                }
                else if (false == info.isCritical)
                {
                    //            clone.transform.position = new Vector3(position.x + UnityEngine.Random.Range(-0.3f, 0.3f),
                    //              position.y + UnityEngine.Random.Range(-0.8f, 0), position.z);

                    clone.transform.position = new Vector3(info.position.x + UnityEngine.Random.Range(-0.3f, 0.3f),
                        info.position.y + UnityEngine.Random.Range(-0.5f, -0.2f), info.position.z);
                }
                else
                {
                    //            clone.transform.position = new Vector3(position.x, position.y + 0.4f, position.z);
                    clone.transform.position = new Vector3(info.position.x + UnityEngine.Random.Range(-0.15f, 0.15f),
                        info.position.y + UnityEngine.Random.Range(-0.15f, 0.15f), info.position.z);
                }

                var particle = clone.GetComponent<DamageParticle>();

                particle.Init(info.type, info.calcHp, info.isCritical, info.isAvoid);
            }
        }
    }
}
