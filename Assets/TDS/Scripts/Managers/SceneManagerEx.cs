using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Data;
using UniRx;
using Unity.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{

    public Define.Scene CurrentSceneType = Define.Scene.Unknown;
    public BaseScene CurrentScene;

    public bool moveScene;

    //public async void LoadScene(Define.Scene sceneType)
    //{
    //    moveScene = true;
    //    DOTween.KillAll();
    //    Managers.Clear();
    //    var ingameLoadingImage = IngameLoadingImage.instance;
    //    if (ingameLoadingImage == null)
    //    {
    //        var loadingObj = Resources.Load<GameObject>("ingame_loading");
    //        var clone = GameObject.Instantiate(loadingObj);
    //        ingameLoadingImage = clone.GetComponent<IngameLoadingImage>();
    //    }
    //    await UniTask.WaitUntil(() => ingameLoadingImage != null);
    //    ingameLoadingImage.Show();
    //    bool isRelease = false;
    //    //ResourceReleaseSceneInit.releaseComplete.Subscribe(_ => { isRelease = true;});
    //    SceneManager.LoadScene(GetSceneName(Define.Scene.ResourceRelease));
    //    await UniTask.WaitWhile(() => !isRelease);

    //    SceneManager.LoadScene(GetSceneName(sceneType));
    //    await ingameLoadingImage.WaitLoading();

    //    moveScene = false;
    //}

    public void SetCurrentScene(BaseScene scene)
    {
        CurrentScene = scene;
    }

    public void Clear()
    {
        if (CurrentScene)
            CurrentScene.Clear();
    }

    public string GetSceneName(Define.Scene sceneType)
    {
        
        switch (sceneType)
        {
            case Define.Scene.Unknown:
                break;
            case Define.Scene.Login:
                break;
            case Define.Scene.GameScene:
                return "GameScene";
            case Define.Scene.Loading:
                break;
            case Define.Scene.Lobby:
                return "LobbyScene";
            case Define.Scene.ResourceRelease:
                return "ResourceReleaseScene";
            default:
                throw new ArgumentOutOfRangeException(nameof(sceneType), sceneType, null); ;
            case Define.Scene.Debug:
                return "DebugScene";
        }
        return "";
    }
}
