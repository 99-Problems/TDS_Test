using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Data;
using UniRx;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using System.Linq;
using static System.String;
using static ResourceManager;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Data.Managers;
using UniRx.Triggers;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public enum ESIZE
    {
        S,
        M,
        L,
    }

    public static readonly int LIFE_COUNT_TEXT_ASSEST = 1;
    public static readonly int LIFE_COUNT_INGAME_OBJECT = 2;

    private static Subject<string> loadTextAsset = new Subject<string>();

    public static IObservable<string> OnLoadTextAsset
    {
        get { return loadTextAsset.AsObservable(); }
    }

    private static Subject<string> loadByteAsset = new Subject<string>();

    public static IObservable<string> OnLoadByteAsset
    {
        get { return loadByteAsset.AsObservable(); }
    }

    public static string AssetPath = "Assets/TDS/Prefabs/";

    private static ObjectPool objectPool = new ObjectPool();

    public static Dictionary<string, AsyncOperationHandle> loadedAssets = new Dictionary<string, AsyncOperationHandle>();

    public async UniTask<GameObject> Instantiate(string bundleName, string assetName, Transform parent = null)
    {
        var loadedObj = await LoadAsync<GameObject>(bundleName, String.Concat(assetName, ".prefab"));
        if (loadedObj == null)
        {
            Debug.Log($"Failed to load prefab : {bundleName}/{assetName}");
            return null;
        }

        GameObject go = Object.Instantiate(loadedObj, parent);
        go.name = loadedObj.name;
        return go;
    }

    public async UniTask<GameObject> LoadAsyncGameObject(string bundleName, string assetName)
    {
        return await LoadAsync<GameObject>(bundleName, assetName);
    }
    public async UniTask<GameObject> LoadAsyncGameObject(string bundleName, string assetName, Define.AssetLabel label)
    {
        return await LoadAsync<GameObject>(bundleName, assetName, label);
    }
    public async UniTask<GameObject> LoadAsyncParticle(string bundleName, string assetName)
    {
        return await LoadAsync<GameObject>(bundleName, assetName, Define.AssetLabel.Particle);
    }
    public async UniTask<TMP_FontAsset> LoadAsyncFont(string bundleName, string assetName)
    {
        return await LoadAsync<TMP_FontAsset>(bundleName, assetName, Define.AssetLabel.Font);
    }

    public async UniTask<Material> LoadAsyncMaterial(string bundleName, string assetName)
    {
        return await LoadAsync<Material>(bundleName, assetName, Define.AssetLabel.Material);
    }

    public async UniTask<T> LoadAsync<T>(string bundleName, string assetName, Define.AssetLabel label, int lifeCount = 1, CancellationToken cancellationToken = default(CancellationToken)) where T : UnityEngine.Object
    {
        var asset = await LoadAsset<T>(bundleName, assetName, lifeCount, cancellationToken, label);
        return asset;
    }

    public async UniTask<T> LoadAsync<T>(string bundleName, string assetName, int lifeCount = 1, CancellationToken cancellationToken = default(CancellationToken)) where T : UnityEngine.Object
    {
        var asset = await LoadAsset<T>(bundleName, assetName, lifeCount, cancellationToken);
        return asset;
    }

    public static async UniTask<T> LoadAsset<T>(string bundleName, string assetName, int lifeCount, CancellationToken cancellationToken = default(CancellationToken), Define.AssetLabel label = Define.AssetLabel.Default) where T : Object
    {
#if TEST_DOWNLOAD || !UNITY_EDITOR

        #region Addressables
        int index = assetName.LastIndexOf('.');

        if (index > 0)
        {
            assetName = assetName.Substring(0, index);
        }
        T retObj = null; 
        {
            bool isRet = false;
            try
            {
                await objectPool.Load<T>(bundleName, assetName, (ret) =>
                {
                    isRet = true;
                    retObj = ret;
                }, lifeCount, label).TimeoutWithoutException(TimeSpan.FromSeconds(5));

                await UniTask.WaitWhile(() => !isRet, PlayerLoopTiming.Update, cancellationToken);

                return retObj;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion
#elif UNITY_EDITOR             
        await UniTask.DelayFrame(1, PlayerLoopTiming.Update, cancellationToken);
        return objectPool.LoadOnEditor<T>(bundleName, assetName, lifeCount);
#endif
    }

    public async UniTask<GameObject> LoadGameObjectByLabel(string label, string assetName)
    {
        var asset = await LoadAssetByLabel<GameObject>(label, assetName);

        asset.OnDestroyAsObservable().Subscribe(_ =>
        {
            ReleaseAsset(assetName);
        }).AddTo(asset);

        return asset;
    }

    public async UniTask<GameObject> LoadPopup(string assetName)
    {
        var asset = await LoadAssetByLabel<GameObject>(Define.AssetLabel.Popup.GetLabelString(), assetName);
        return asset;
    }

    public async UniTask<TextAsset> LoadJsonByLabel(string assetName)
    {
        var asset = await LoadAssetByLabel<TextAsset>(Define.AssetLabel.Script.GetLabelString(), assetName);


        return asset;
    }

    public async UniTask<TMP_FontAsset> LoadFont(string assetName)
    {
        var asset = await LoadAssetByLabel<TMP_FontAsset>(Define.AssetLabel.Font.GetLabelString(), assetName);

        return asset;
    }

    public async UniTask<Material> LoadMaterial(string assetName)
    {
        var asset = await LoadAssetByLabel<Material>(Define.AssetLabel.Material.GetLabelString(), assetName);

        return asset;
    }

    void LoadAssetAsync<T>(IResourceLocation location, System.Action<T> callback, string assetName) where T : class
    {
        if (!loadedAssets.ContainsKey(assetName))
        {
            var handle = Addressables.LoadAssetAsync<T>(location);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedAssets[assetName] = handle;

                    callback?.Invoke(operation.Result);  // 비동기 작업이 완료되면 콜백 호출
                }
                else
                {
                    callback?.Invoke(null);
                }
            };
        }
    }

    public void ReleaseAsset(string key)
    {
        if (loadedAssets.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            loadedAssets.Remove(key);
        }
        else
        {
            Debug.Log($"Release failed: {key}", Color.red);
        }
    }
    public void ReleaseAllAssets()
    {
        foreach (var handle in loadedAssets.Values)
        {
            Addressables.Release(handle);
        }
        loadedAssets.Clear();
        Debug.Log("All assets successfully released.");
    }

    public async UniTask<T> LoadAssetByLabel<T>(string label, string assetName) where T : UnityEngine.Object
    {
        T ret = null;
        bool isRet = false;

        if(loadedAssets.ContainsKey(assetName))
        {
            return loadedAssets[assetName].Result as T;
        }
        Addressables.LoadResourceLocationsAsync(label, typeof(T)).Completed += (operation) =>
        {
            OnLocationsLoaded(operation);
           
        };

        async void OnLocationsLoaded(AsyncOperationHandle<IList<IResourceLocation>> handle)
        {
            var locations = handle.Result;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {

                bool isLoaction = false;
                // 필요한 리소스만 골라서 로드
                foreach (var location in locations)
                {
                    if (Path.GetFileNameWithoutExtension(location.PrimaryKey) == assetName)
                    {
                        isLoaction = true;
                        LoadAssetAsync<T>(location, asset =>
                        {
                            if (asset != null)
                            {
                                ret = asset;
                                isRet = true;
                            }
                            else
                            {
                                isRet = true;
                            }
                        }, assetName);
                        break;
                    }
                }

                if (isLoaction == false)
                    isRet = true;
            }
        }

        await UniTask.WaitUntil(() => isRet);

        return ret;
    }


    public void ReleasePopupAsset(Define.EPOPUP_TYPE _type)
    {
        ReleaseAsset(_type.ToString());
    }
   
    public async UniTask<string> LoadScript(string _dirName, string _fileName)
    {
        _fileName += ".json";
        var text = await LoadScriptAddresableAsync(_dirName, _fileName);
        return text;
    }

    public async UniTask<byte[]> LoadBytes(string _dirName, string _fileName)
    {
        _fileName += ".bytes";
        var textAsset = await LoadAsync<TextAsset>(_dirName, _fileName, LIFE_COUNT_TEXT_ASSEST);
        //TextAsset textAsset = new TextAsset();
        Debug.Log($"Load Text {_dirName}^{_fileName}= {textAsset.text}");
        Debug.Log($"Load Text Bytes {textAsset.bytes.Length} {_dirName}^{_fileName}= {textAsset.bytes}");
        loadByteAsset.OnNext(_fileName);
        return textAsset.bytes;
    }
    public async UniTask<string> LoadTextAsync(string bundleName, string assetName)
    {
        var textAsset = await LoadAsync<TextAsset>(bundleName, assetName, LIFE_COUNT_TEXT_ASSEST);
        //var textAsset = await LoadAsync<TextAsset>("Script", assetName,Define.AssetLabel.Script, LIFE_COUNT_TEXT_ASSEST);
        loadTextAsset.OnNext(assetName);
        var ret = textAsset != null ? textAsset.text : "";
        return ret;
    }

    public async UniTask<string> LoadScriptAddresableAsync(string bundleName, string assetName)
    {
        var textAsset = await LoadAsync<TextAsset>(bundleName, assetName, Define.AssetLabel.Script, LIFE_COUNT_TEXT_ASSEST);
        //var textAsset = await LoadAsync<TextAsset>("Script", assetName,Define.AssetLabel.Script, LIFE_COUNT_TEXT_ASSEST);
        loadTextAsset.OnNext(assetName);
        var ret = textAsset != null ? textAsset.text : "";
        return ret;
    }

    public partial class ObjectPool
    {
        [Serializable]
        public class Task
        {
            public string key;
            public List<object> callbacks;
        }

        [Serializable]
        public class ObjectItem
        {
            public string key;
            public string bundleName;
            public string assetName;
            public Object obj;
            public int lifeCount;
        }

        private ConcurrentDictionary<string, ObjectItem> items = new ConcurrentDictionary<string, ObjectItem>();
        private ConcurrentDictionary<string, Task> tasks = new ConcurrentDictionary<string, Task>();


        public async UniTask Load<T>(string bundleName, string assetName, Action<T> callback, int lifeCount, Define.AssetLabel label = Define.AssetLabel.Default)
            where T : Object
        {
            string key = String.Concat(bundleName, "/", assetName);

            if (items.TryGetValue(key, out ObjectItem ret))
            {
                ret.lifeCount = lifeCount;
                callback(ret.obj as T);
                return;
            }


            if (tasks.TryGetValue(key, out Task task))
            {
                task.callbacks.Add(callback);
                return;
            }

            Task newTask = new Task()
            {
                key = key,
                callbacks = new List<object>() { callback },
            };
            tasks.TryAdd(key, newTask);
            T newObj;
            if (label == Define.AssetLabel.Script)
            {
                newObj = (T)(await Managers.Resource.LoadJsonByLabel(assetName) as Object);
            }
            else if (label == Define.AssetLabel.Default)
            {
                newObj = (T)(await Managers.Resource.LoadGameObjectByLabel(label.GetLabelString(), assetName) as Object);
            }
            else if (label == Define.AssetLabel.Popup)
            {
                newObj = (T)(await Managers.Resource.LoadPopup(assetName) as Object);
            }
            else if (label == Define.AssetLabel.Font)
            {
                newObj = (T)(await Managers.Resource.LoadFont(assetName) as Object);
            }
            else if (label == Define.AssetLabel.Material)
            {
                newObj = (T)(await Managers.Resource.LoadMaterial(assetName) as Object);
            }
            else if (label == Define.AssetLabel.Particle)
            {
                newObj = (T)(await Managers.Resource.LoadGameObjectByLabel(label.GetLabelString(), assetName) as Object);
            }
            else
            {
                newObj = null;
                Debug.LogError("no Object");
            }

            if(newObj)
            {

                if (lifeCount > 0)
                {
                    items.TryAdd(key,
                        new ObjectItem { key = key, bundleName = bundleName, assetName = assetName, obj = newObj, lifeCount = lifeCount, });
                }

                for (var index = 0; index < newTask.callbacks.Count; index++)
                {
                    var newTaskCallback = newTask.callbacks[index] as Action<T>;
                    newTaskCallback?.Invoke(newObj);
                }

            }
            else
            {
                for (var index = 0; index < newTask.callbacks.Count; index++)
                {
                    var newTaskCallback = newTask.callbacks[index] as Action<T>;
                    newTaskCallback?.Invoke(null);
                }
            }
            tasks.TryRemove(key, out var delete);
        }

#if UNITY_EDITOR
        public T LoadOnEditor<T>(string dirName, string assetName, int lifeCount) where T : Object
        {
            string key = String.Concat(dirName, "/", assetName);
            if (items.TryGetValue(key, out ObjectItem ret))
            {
                ret.lifeCount = lifeCount;
                return ret.obj as T;
            }

            var assetPath = String.Concat(AssetPath, key);
            var newObj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (newObj != null)
            {
                var path2 = AssetDatabase.GetAssetPath(newObj);
                if (Path.GetFileName(assetPath).CompareTo(Path.GetFileName(path2)) != 0)
                {
                    Debug.LogError($"파일명의 대소문자를 확인해주세요! {assetPath}");
                }

                if (lifeCount > 0)
                {
                    items.TryAdd(key, new ObjectItem { key = key, obj = newObj, lifeCount = lifeCount, });
                }
            }

            return newObj;
        }

        public T LoadDefaultAssetOnEditor<T>(string dirName, string assetName, int lifeCount) where T : Object
        {
            string key = String.Concat(dirName, "/", assetName);
            if (items.TryGetValue(key, out ObjectItem ret))
            {
                ret.lifeCount = lifeCount;
                return ret.obj as T;
            }


            var newObj = AssetDatabase.LoadAssetAtPath<T>(String.Concat(AssetPath, dirName, "/", assetName));
            if (newObj != null)
            {
                if (lifeCount > 0)
                {
                    items.TryAdd(key, new ObjectItem { key = key, obj = newObj, lifeCount = lifeCount, });
                }
            }

            return newObj;
        }

#endif
        public void ReleaseReference()
        {
            items.Values.ForEach(x => x.lifeCount -= 1);

            var keys = items.Values.Where(x => x.lifeCount <= 0).Select(y => y.key).ToList();
            keys.ForEach(x => items.TryRemove(x, out var delete));
        }

        public void RemoveReferences(string bundleName)
        {
            var keys = items.Values.Where(x => x.bundleName == bundleName).Select(y => y.key).ToList();
            keys.ForEach(x => items.TryRemove(x, out var delete));
        }

        public void Clear()
        {
            items.Clear();
            tasks.Clear();
        }
    }
}
