
/********************************************************/
/*Auto Create File*/
/*Source : ExcelToJsonConvert*/
/********************************************************/
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
public class StringEnglishScript
{

    public int stringID;
    public int langaugeType;
    public int stringData;

}

public partial class DataManager
{
    [Serializable]
    public class StringEnglishScriptAll
    {
        public List<StringEnglishScript> result;
    }



    private List<StringEnglishScript> listStringEnglishScript = null;


    public StringEnglishScript GetStringEnglishScript(Predicate<StringEnglishScript> predicate)
    {
        return listStringEnglishScript?.Find(predicate);
    }
    public List<StringEnglishScript> GetStringEnglishScriptList { 
        get { 
                return listStringEnglishScript;
        }
    }



    void ClearStringEnglish()
    {
        listStringEnglishScript?.Clear();
    }


    async UniTask LoadScriptStringEnglish()
    {
        List<StringEnglishScript> resultScript = null;
        if(resultScript == null)
        {
            var load = await Managers.Resource.LoadScript("scripts/string", "stringEnglish"); 
            if (load == "") 
            {
                Debug.LogWarning("StringEnglish is empty");
                return;
            }
            var json = JsonUtility.FromJson<StringEnglishScriptAll>("{ \"result\" : " + load + "}");
            resultScript = json.result;
        }



        listStringEnglishScript = resultScript;
    }
}


