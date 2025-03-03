using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
//using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;


public struct ParticleSortData
{
    public int originOrder;
    public int prevOrder;
    public float originZ;
    public ParticleSystemRenderer particleRenderer;
}

public class ParticleSorter : MonoBehaviour
{
    private ParticleSortData[] data;

    public void LateUpdate()
    {
        if (data == null)
            return;
        for (var index = 0; index < data.Length; index++)
        {
            var mit = data[index];
            if (mit.particleRenderer == null)
                continue;
            mit.prevOrder = -(int)((transform.position.z + mit.originZ) * 200) + mit.originOrder;
            mit.particleRenderer.sortingOrder = mit.prevOrder;
            data[index] = mit;
        }
    }

    public void Reset()
    {
        if (data != null)
            return;
        data = gameObject.DescendantsAndSelf().OfComponent<ParticleSystemRenderer>().Where(_ => _)
            .Select(_ => new ParticleSortData
            {
                particleRenderer = _,
                originOrder = _.sortingOrder,
                originZ = _.transform.position.z - transform.position.z,
            }).ToArray();
    }
}