using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using IngameDebugConsole;
using UnityEngine.UI;
using Data;
using System;

public class DebugTest : MonoBehaviour
{
    public enum EDEBUG_TYPE
    {
        RESOURCE_CLEAN,
        ADD_SCORE_100000,
        TIME_ADD_10S,
        END_GAME,
    }

    public EDEBUG_TYPE debugType;

    private Action logWindow;

    void Start()
    {
        var btn = GetComponent<Button>();
        btn.OnClickAsObservable().Subscribe(_ => { Click(); });

        logWindow = () =>
        {
            var gameData = Managers.Scene.CurrentScene as IGameData;
            if (debugType == EDEBUG_TYPE.ADD_SCORE_100000)
            {
                gameObject.SetActive(gameData != null);
            }
            else if (debugType == EDEBUG_TYPE.TIME_ADD_10S)
            {
                gameObject.SetActive(gameData != null && gameData.ContentType == Define.ECONTENT_TYPE.INGAME);
            }
            else if (debugType == EDEBUG_TYPE.END_GAME)
            {
                gameObject.SetActive(gameData != null && gameData.ContentType == Define.ECONTENT_TYPE.INGAME);
            }
        };
        DebugLogManager.Instance.OnLogWindowShown += logWindow;
    }

    private void OnDestroy()
    {
        DebugLogManager.Instance.OnLogWindowShown -= logWindow;
    }

    private void Click()
    {
#if LOG_ENABLE
            Debug.Log($"Click {debugType}");
        var gameState = Managers.Scene.CurrentScene as IGameState;
        var gameData = Managers.Scene.CurrentScene as IGameData;

        switch (debugType)
            {
                case EDEBUG_TYPE.RESOURCE_CLEAN:
                    Resources.UnloadUnusedAssets();
                    GC.Collect();
                    break;
            case EDEBUG_TYPE.ADD_SCORE_100000:
                AddCheatScore(100000);
                break;
            case EDEBUG_TYPE.TIME_ADD_10S:
                
                if (gameState == null || gameData == null)
                    return;
                if(gameState.GetGameState == Define.EGAME_STATE.PLAY)
                {
                    gameData.PlayInfo.playData.limitTime += 10;
                }
                break;
            case EDEBUG_TYPE.END_GAME:
                if (gameState == null || gameData == null)
                    return;

                if (gameState.GetGameState == Define.EGAME_STATE.PLAY)
                {
                    gameData.PlayInfo.playData.currentTime = gameData.PlayInfo.playData.limitTime;
                }
                break;

            default:
                break;
            }
#endif
    }

    private static void AddCheatScore(int score)
    {
        var gameData = Managers.Scene.CurrentScene as IGameData;

        //var playInfo = gameData.PlayInfo;
        //if(playInfo)
        //{
        //    playInfo.playData.score += score;
        //}
    }
    }
