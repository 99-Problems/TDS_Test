using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasManagerEx : MonoBehaviour
{
// SpriteAtlasManager.atlasRequested :  Asset을 받는 시점(Atlas가 필요하다고 판단되서 호출되는듯)에서 호출되는 event를 등록
    void OnEnable()
    {
        SpriteAtlasManager.atlasRequested += RequestLateBindingAtlas;
    }

    void OnDisable()
    {
        SpriteAtlasManager.atlasRequested -= RequestLateBindingAtlas;
    }

    void RequestLateBindingAtlas(string _spriteAtlasName, System.Action<SpriteAtlas> action)
    {
        async void LoadSpriteAtlas(string path1)
        {
            //var atlas = await Managers.Resource.LoadAsync<SpriteAtlas>(path1, _spriteAtlasName + ".spriteatlas", 1);
            //action.Invoke(atlas);
        }

        var path = _spriteAtlasName.Replace('^', '/');
        path = path.Remove(path.Length - 3);
        LoadSpriteAtlas(path);
    }
}