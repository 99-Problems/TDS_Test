using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

public partial class DataManager
{
    public int cntLoad = 0;
   public int maxCnt = 2;
    
    public void Complete()
    {
        cntLoad++;
    }
    
    public float LoadProcess()
    {
        return (float)cntLoad / (float)maxCnt;
    }

    public async UniTask LoadAllParser()
    {
        cntLoad = 0;
        ClearStringEnglish();
        ClearUnitInfo();

    await UniTask.WhenAll(
            LoadScriptStringEnglish(),
            LoadScriptUnitInfo());
    }
}
