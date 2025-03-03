using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleSystemManualUpdate : MonoBehaviour
{
    ParticleSystem[] particleSystems;

    float[] simulationTimes;

    public float startTime = 0;
    public float simulationSpeedScale = 1;
    public Animator[] animators;

    public void Initialize()
    {
        if (particleSystems == null)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(false);
            animators = GetComponentsInChildren<Animator>(false);
            simulationTimes = new float[particleSystems.Length];
            for (int i = 0; i < particleSystems.Length; ++i)
            {
                simulationTimes[i] = particleSystems[i].main.simulationSpeed;
            }

            if (animators != null)
            {
                foreach (var mit in animators)
                {
                    mit.enabled = false;
                }
            }
        }

        for (int i = 0; i < particleSystems.Length; ++i)
        {
            var main = particleSystems[i].main;
            main.simulationSpeed = simulationTimes[i] * simulationSpeedScale;
        }
    }

    void Update()
    {
        if (animators != null)
        {
            foreach (var mit in animators)
            {
                mit.Update(Time.deltaTime * simulationSpeedScale);
            }
        }
        // particleSystems[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // for (int i = 1; i < particleSystems.Length; ++i)
        // {
        //     var system = particleSystems[i];
        //     system.Play(false);
        //     float deltaTime = system.main.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        //     simulationTimes[i] += deltaTime * system.main.simulationSpeed * simulationSpeedScale;
        //
        //     float currentSimulationTime = startTime + simulationTimes[i];
        //     system.Simulate(currentSimulationTime, false, false, false);
        // }
    }

    public bool IsAlive()
    {
        if (particleSystems == null || particleSystems.Length <= 0)
            return false;
        return particleSystems[0].IsAlive(true);
    }

    public ParticleSystem GetHead()
    {
        if (particleSystems == null || particleSystems.Length <= 0)
            return null;
        return particleSystems[0];
    }
}