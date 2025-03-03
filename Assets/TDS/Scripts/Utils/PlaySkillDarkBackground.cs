using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlaySkillDarkBackground : MonoBehaviour
{
    SpriteRenderer spBackground;
    Color skillColor = new Color(0.5f, 0.5f, 0.5f, 1);
    Color nomalColor = new Color(1, 1, 1, 1);

    private bool currentBackDark = false;

    public List<ParticleSystem> particleList;

    void Start()
    {
        spBackground = GetComponent<SpriteRenderer>();

        for (int i = 0; i < particleList.Count; i++)
        {
            particleList[i].Play(true);
        }

        //sceneInit.OnActiveSkillUnitCount.Subscribe(_1 => { StopBackground(_1 != 0); });
    }

    // Update is called once per frame
    public void StopBackground(bool _stop)
    {
        if (spBackground && currentBackDark != _stop)
        {
            spBackground.color = _stop ? skillColor : nomalColor;
            for (int i = 0; i < particleList.Count; i++)
            {
                if (_stop)
                {
                    particleList[i].Pause(true);
                }
                else
                {
                    particleList[i].Play(true);
                }
            }

            currentBackDark = _stop;
        }

    }
}
