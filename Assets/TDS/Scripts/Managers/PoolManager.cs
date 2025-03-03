using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PoolManager : MonoBehaviour
{
    #region Pool

    class Pool<T> where T : MonoBehaviour
    {
        public T Original { get; private set; }
        public Transform Root { get; set; }
        public bool isDebug;
        Queue<T> _poolStack = new Queue<T>();

        public void Init(T original, string poolName, int count = 5)
        {
            Original = original;
            Original.gameObject.SetActive(false);
            Original.name = poolName + "_Origin";
            Root = new GameObject().transform;
            Root.gameObject.SetActive(false);
            Root.name = poolName;

            Original.transform.SetParent(Root);
            for (int i = 0; i < count; i++)
                Push(Create());
        }

        T Create()
        {
            GameObject go = GameObject.Instantiate(Original.gameObject);
            go.name = Root.name;
            return go.GetComponent<T>();
        }

        public void Push(T poolable)
        {
            if (poolable == null)
                return;
            poolable.transform.SetParent(Root);
            if (Original != null)
            {
                poolable.transform.localScale = Original.transform.localScale;
            }

            poolable.gameObject.SetActive(false);
            _poolStack.Enqueue(poolable);
        }

        public T Pop(Transform parent)
        {
            T poolable = null;
            int cnt = 0;
            while (true)
            {
                if (Original == null)
                    break;
                if (_poolStack.Count > 0)
                    poolable = _poolStack.Dequeue();
                else
                {
                    poolable = Create();
                    if (poolable == null)
                        return null;
                }

                if (poolable != null)
                    break;
            }

            if (poolable == null)
                return null;

            // DontDestroyOnLoad 해제 용도
            if (parent == null)
            {
                parent = Managers.Scene.CurrentScene.transform;
            }

            poolable.transform.parent = parent;

            poolable.gameObject.SetActive(true);

            return poolable;
        }

        public int GetCount()
        {
            return _poolStack.Count;
        }
    }
    #endregion

    Dictionary<string, Pool<UnitLogic>> unitLogicPool = new Dictionary<string, Pool<UnitLogic>>();
    Pool<DamageParticle> damageParticlePool = new Pool<DamageParticle>();

    Pool<ProjectileLogic> boxColliderPool = new Pool<ProjectileLogic>();
    Pool<ProjectileLogic> bulletColliderPool = new Pool<ProjectileLogic>();

    Transform _root;

    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
            DontDestroyOnLoad(_root);
        }

    }

    public void Clear()
    {
        
    }

    public UnitLogic PopUnit(UnitInfoScript unitInfo)
    {
        if (unitInfo == null)
            return null;

        return PopUnit(unitInfo.assetPath, unitInfo.prefabName);
    }

    public UnitLogic PopUnit(string assetPath, string prefabName)
    {
        var key = String.Concat(assetPath, "/", prefabName);
        if (unitLogicPool.TryGetValue(key, out var pool))
        {
            var unitLogic = pool.Pop(null);
            if (unitLogic == null)
            {
                return null;
            }

            unitLogic.gameObject.SetActive(false);
            return unitLogic;
        }

        //풀이 없음
        return null;
    }

    public async UniTask<UnitLogic> CreateUnitPool(UnitInfoScript unitInfo)
    {
        if (unitInfo == null)
            return null;

        return await CreateUnitPool(unitInfo.assetPath, unitInfo.prefabName);
    }

    public async UniTask<UnitLogic> CreateUnitPool(string assetPath, string prefabName)
    {
        var key = String.Concat(assetPath, "/", prefabName);
        var loadedUnit = await Managers.Resource.LoadAsyncGameObject(
            $"unit/{assetPath}",
            $"{prefabName}.prefab");
        if (loadedUnit == null)
        {
            Debug.LogWarning(key);
            return null;
        }

        if (Managers.Scene.moveScene)
        {
            return null;
        }

        if (unitLogicPool.TryGetValue(key, out var _logic))
            return _logic.Pop(null);

        var clone = GameObject.Instantiate(loadedUnit).GetComponent<UnitLogic>();
        clone.gameObject.SetActive(false);

        List<GameObject> removeClone = new List<GameObject>();
        {
            var origin = loadedUnit.GetComponent<UnitLogic>();
        }
        await UniTask.DelayFrame(5);
        removeClone.Destroy(true);

        if (unitLogicPool.TryGetValue(key, out var pool))
        {
            return clone;
        }

        var newPool = new Pool<UnitLogic>();
        newPool.Init(clone, prefabName, 1);
        newPool.Root.transform.SetParent(_root);

        unitLogicPool[key] = newPool;
        return newPool.Pop(null);
    }

    public void PushUnit(string assetPath, string prefabName, UnitLogic _logic)
    {
        if (_logic == null) return;

        if (!(assetPath.IsNullOrEmpty() || prefabName.IsNullOrEmpty()))
        {
            var key = String.Concat(assetPath, "/", prefabName);
            if (unitLogicPool.TryGetValue(key, out var pool))
            {
                //if (_logic.navMesh)
                //    _logic.navMesh.enabled = false;
                _logic.transform.position = new Vector3(9999, 9999, 9999);
                pool.Push(_logic);
                return;
            }
        }

        GameObject.Destroy(_logic.gameObject);
    }
    
    public async UniTask<DamageParticle> CreateDamageParticle()
    {
        var damage = await Managers.Resource.LoadAsyncGameObject("common", "DamageParticle.prefab");
        if (damage == null)
        {
            Debug.LogWarning(damage);
            return null;
        }

        var clone = GameObject.Instantiate(damage).GetComponent<DamageParticle>();
        clone.gameObject.SetActive(false);
        var newPool = new Pool<DamageParticle>();
        newPool.Init(clone, "damage", 10);
        newPool.Root.transform.SetParent(_root);
        damageParticlePool = newPool;
        return null;
    }
    public DamageParticle PopDamageParticle()
    {
        return damageParticlePool.Pop(null);
    }

    public void PushDamageParticle(DamageParticle _particle)
    {
        damageParticlePool.Push(_particle);
    }

    public async UniTask<ProjectileLogic> CreateBoxCollider()
    {
        var _prefab = await Managers.Resource.LoadAsync<GameObject>("common/collider", "box.prefab");
        var clone = GameObject.Instantiate(_prefab).GetComponent<ProjectileLogic>();
        if (boxColliderPool.Original != null)
        {
            return clone;
        }
        boxColliderPool.Init(clone, "box", 10);
        boxColliderPool.Root.transform.SetParent(_root);
        return boxColliderPool.Pop(null);
    }

    public ProjectileLogic PopBoxCollider()
    {
        return boxColliderPool.Pop(null);
    }

    public void PushCollider(ProjectileLogic mit)
    {
        if(mit.parentUnit.unitType == Define.EUnitType.Monster)
            boxColliderPool.Push(mit);
        else if(mit.parentUnit.unitType == Define.EUnitType.Player)
            bulletColliderPool.Push(mit);
    }

    public async UniTask<ProjectileLogic> CreateBulletCollider()
    {
        var _prefab = await Managers.Resource.LoadAsync<GameObject>("common/collider", "bullet.prefab");
        var clone = GameObject.Instantiate(_prefab).GetComponent<ProjectileLogic>();
        if (bulletColliderPool.Original != null)
        {
            return clone;
        }
        bulletColliderPool.Init(clone, "bullet", 10);
        bulletColliderPool.Root.transform.SetParent(_root);
        return bulletColliderPool.Pop(null);
    }

    public ProjectileLogic PopBulletCollider()
    {
        return bulletColliderPool.Pop(null);
    }
}