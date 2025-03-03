
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Data;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class UnitInfoScript
{
    public int unitID;
    public int nameID;
    public string prefabName;
    public string assetPath;
}

public partial class DataManager
{
    [Serializable]
    public class UnitInfoScriptAll
    {
        public List<UnitInfoScript> result;
    }



    private List<UnitInfoScript> listUnitInfoScript = null;


    public UnitInfoScript GetUnitInfoScript(Predicate<UnitInfoScript> predicate)
    {
        return listUnitInfoScript?.Find(predicate);
    }
    public List<UnitInfoScript> GetUnitInfoScriptList { 
        get { 
                return listUnitInfoScript;
        }
    }



    void ClearUnitInfo()
    {
        listUnitInfoScript?.Clear();
    }


    async UniTask LoadScriptUnitInfo()
    {
        List<UnitInfoScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/unit", "UnitInfo"); 
            if (load == "") 
            {
                Debug.LogWarning("UnitInfo is empty");
                return;
            }
            var json = JsonUtility.FromJson<UnitInfoScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }

        listUnitInfoScript = resultScript;
    }
}


