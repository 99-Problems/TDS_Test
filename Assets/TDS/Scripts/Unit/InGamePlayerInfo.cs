using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class InGamePlayerInfo : MonoBehaviour
{
    public readonly List<UnitLogic> listUnit = new List<UnitLogic>();

    public IGameData GameData { get; set; }
    public static Subject<InGamePlayerInfo> OnInitPlayer = new Subject<InGamePlayerInfo>();

    public List<ProjectileLogic> listLogicObject = new List<ProjectileLogic>();
    private List<ProjectileLogic> addLogicObject = new List<ProjectileLogic>();
    private List<ProjectileLogic> removeLogicObject = new List<ProjectileLogic>();

    public void Init(IEnumerable<UnitLogic> _listMyUnit)
    {
        listUnit.Clear();
        if (_listMyUnit != null)
        {
            foreach (var mit in _listMyUnit)
            {
                if (mit == null)
                    continue;

                mit.Reset();
                if (!mit.gameObject.activeSelf)
                    mit.gameObject.SetActive(true);
            }

            listUnit.AddRange(_listMyUnit);
        }
    }

    public void AddUnit(UnitLogic _myUnit)
    {
        if(_myUnit != null)
        {
            _myUnit.Reset();
            if (!_myUnit.gameObject.activeSelf)
                _myUnit.gameObject.SetActive(true);

            if(listUnit != null && !listUnit.Contains(_myUnit))
                listUnit.Add(_myUnit);
        }
    }
    public void AddUnit(IEnumerable<UnitLogic> _listMyUnit)
    {
        if (_listMyUnit != null)
        {
            foreach (var mit in _listMyUnit)
            {
                if (mit == null)
                    continue;

                mit.Reset();
                if (!mit.gameObject.activeSelf)
                    mit.gameObject.SetActive(true);
            }

            listUnit?.AddRange(_listMyUnit);
        }
    }

    public UnitLogic GetUnitFromID(int _mitUnitID)
    {
        for (var index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (mit.UnitID == _mitUnitID)
                return mit;
        }

        return null;
    }

    public void FrameMove(float _deltaTime)
    {
        for (int index = 0; index < listUnit.Count; index++)
        {
            var mit = listUnit[index];
            if (mit == null)
                continue;
            if (!mit.gameObject.activeInHierarchy)
                continue;

            mit.FrameMove(_deltaTime);
        }

        {
            foreach (var mit in listLogicObject.Distinct())
            {
                if (mit)
                {
                    mit.FrameMove(_deltaTime);
                }
            }

            listLogicObject.AddRange(addLogicObject);
            addLogicObject.Clear();

            foreach (var mit in removeLogicObject)
            {
                if (mit == null)
                    continue;
                Managers.Pool.PushCollider(mit);
                listLogicObject.Remove(mit);
            }

            removeLogicObject.Clear();
        }
    }

    public void Entry()
    {

    }

    public void AddObject(ProjectileLogic _logic)
    {
        addLogicObject.Add(_logic);
    }

    public void RemoveObject(ProjectileLogic _logic)
    {
        removeLogicObject.Add(_logic);
    }

    public float GetRandom(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public int GetRandom(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    internal bool IsPlayerDie()
    {
        var isDead = true;
        foreach (var unit in listUnit)
        {
            if (unit.IsDie == false)
            {
                isDead = false;
                break;
            }
        }
        return isDead;
    }

    public float GetPartyHpRate()
    {
        long fullHp = 0;
        float hp = 0;
        foreach (var unit in listUnit)
        {
            fullHp += unit.GetOriginalFullHp;
            hp += unit.hp;
        }

        return hp / fullHp;
    }
}
