using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;
using UnityEngine.UI;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;
using Data;

public struct DamageParticleData
{
    public Define.EDAMAGE_TYPE type;
    public long calcHp;
    public bool isCritical;
    public bool isAvoid;
    public Vector3 position;
    public bool isMoveRight;
    public long accountID;
}

public class DamageParticle : MonoBehaviour
{
    public TextMeshPro damageText;
    public TextMeshPro missText;
    public RectTransform rect;
    public float fontExpandOffsetSize = 0.4f;
    public float fontUpOffsetValue = 0.2f;
    public float fontExpandSize = 1.3f;
    public float fontUpValue = 0.2f;
    public float fontDelay = 0.1f;

    public bool exFontAction = true;

    public float fontSize;
    private bool isCritical;
    private bool isAvoid;
    private Transform textTrans;
    private Transform criticalTrans;
    private float initFontSize;
    private TextMeshPro targetText;
    public void Init(Define.EDAMAGE_TYPE edamageType, long _damage, bool _isCritical, bool _isAvoid)
    {
        transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, 0, 0);

        textTrans = damageText.transform;
        isCritical = _isCritical;
        isAvoid = _isAvoid;
        //StartCoroutine(DelayTime());

        if (_damage < 0) _damage *= -1;
        damageText.text = _damage.ToString();
        // text.rectTransform.anchoredPosition = new Vector2(_isMoveRight ? 0.5f : -0.5f, 0);
        //text.rectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        damageText.enableVertexGradient = true;
        damageText.sortingOrder = 100;
        if (isCritical)
            damageText.color = Color.red;
        else
        {
            switch (edamageType)
            {   
                case Define.EDAMAGE_TYPE.PLAYER:
                    damageText.color = Color.white;
                    break;
                case Define.EDAMAGE_TYPE.ENEMY:
                    damageText.color = Color.red;
                    break;
                default:
                    break;
            }
        }
        damageText.fontStyle = isCritical ? FontStyles.Bold : FontStyles.Normal;
        initFontSize = damageText.fontSize;

        damageText.gameObject.SetActive(!_isAvoid);
        missText.gameObject.SetActive(isAvoid);
        //if (isAvoid) Debug.Log("miss", Color.green);
        targetText = isAvoid ? missText : damageText;

        if (!_isAvoid)
        {
            if (isCritical)
            {
                if (!exFontAction)
                {
                    initFontSize = fontSize * 2f;
                    damageText.fontSize = initFontSize * 2f;
                }
            }
        }

        if (_isAvoid)
        {
            initFontSize = fontSize * 0.7f;
            missText.fontSize = initFontSize;
            upValue = fontUpValue + fontUpOffsetValue;
            delay = fontDelay * 3;
        }
        else if (_isCritical)
        {
            FontCustom(4);
        }
        else
        {
            if (_damage * 0.001 >= 1)
            {
                FontCustom(3);
            }
            else if (_damage * 0.01 >= 1)
            {
                FontCustom(2);
            }
            else if (_damage * 0.1 >= 1)
            {
                FontCustom(1);
            }
        }

        StartCoroutine(DelayTime());
    }

    private void FontCustom(int multiply)
    {
        expandSize = fontExpandSize + fontExpandOffsetSize * multiply;
        upValue = fontUpValue + fontUpOffsetValue * multiply;
        delay = fontDelay + fontDelay * multiply;
    }

    private static WaitForSeconds WaitForSeconds;
    private float expandSize;
    private float upValue;
    private float delay;

    private IEnumerator DelayTime()
    {
        DOTween.To(() => targetText.sortingOrder, x => targetText.sortingOrder = x, 0, 0.8f);

        if (isCritical)
        {
            //text.rectTransform.DOLocalMoveY(0.4f, 0.06f).SetEase(Ease.OutCubic);
        }
        else if (isAvoid)
        {
            targetText.rectTransform.DOLocalMoveY(0.4f, 0.5f).SetEase(Ease.OutCubic);
        }
        else
        {
            if (exFontAction)
            {
                targetText.DOFontSize(initFontSize * 3.0f, 0.1f).SetEase(Ease.InExpo).From();
                targetText.transform.DOShakePosition(0.1f, 0.2f, 25);
                yield return new WaitForSeconds(0.1f);
            }

            targetText.rectTransform.DOLocalMoveY(0.2f, 0.5f).SetEase(Ease.OutCubic);
        }

        if (isCritical)
        {
            if (exFontAction)
            {
                targetText.DOFontSize(initFontSize * 4.0f, 0.25f).SetEase(Ease.InExpo).From();
                targetText.transform.DOShakePosition(0.25f, 0.2f, 25);
                yield return new WaitForSeconds(0.25f);
                targetText.rectTransform.DOLocalMoveY(0.2f, 0.5f).SetEase(Ease.OutCubic);
                yield return new WaitForSeconds(0.25f);
                targetText.DOFade(0, delay);
            }
            else
            {
                //text.DOFontSize(originFontSize * fontExpandSize, 0.05f).SetEase(Ease.OutQuint);
                yield return new WaitForSeconds(0.06f);
                //text.DOFade(1, 0.14f).SetEase(Ease.InExpo);
                targetText.DOFontSize(initFontSize * 0.9f, 0.08f).SetEase(Ease.InExpo);
                yield return new WaitForSeconds(0.08f);
                targetText.rectTransform.DOLocalMoveY(0.2f, 0.3f).SetEase(Ease.OutCubic);
                targetText.DOFontSize(initFontSize, 0.3f).SetEase(Ease.Linear);
                yield return new WaitForSeconds(0.3f);
                targetText.DOFade(0, delay);
            }
        }
        else if (isAvoid)
        {
            //                 text.DOFontSize(originFontSize * fontExpandSize, 0.10f).SetEase(Ease.InOutCubic);
            //                 yield return new WaitForSeconds(0.10f);
            //                 text.DOFontSize(originFontSize * 0.9f, 0.08f).SetEase(Ease.Linear);
            //                 yield return new WaitForSeconds(0.08f);
            //                 text.DOFontSize(originFontSize, 0.11f).SetEase(Ease.Linear);
            //                 text.rectTransform.DOLocalMoveY(fontUpValue, 0.2f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(0.5f);
            targetText.DOFade(0, delay);
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
            targetText.DOFade(0, delay);
        }

        yield return new WaitForSeconds(delay);
        Managers.Pool.PushDamageParticle(this);
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    private void OnGUI()
    {
        targetText.ForceMeshUpdate(true, true);
    }
}