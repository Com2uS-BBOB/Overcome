using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LoadingScene : MonoBehaviour
{
    private static readonly string[] _toolTips = new[]
    {
        "Dash Attack으로 적을 처치하면 쿨타임이 초기화 됩니다.",
        "Combo 등급에 비례하여 더 강한 데미지를 입힐 수 있습니다.",
        "Overdrive를 사용하면 Dash Attack과 Crescent(Guard) Gauge를 계속 사용할 수 있습니다."
    };

    [Header("Tooltip")]
    [SerializeField] private TextMeshProUGUI _toolTipText;
    [SerializeField] private float _tooltipDuration = 2f;
    [SerializeField] private float _toolTipAnimationSpeed = 0.25f;

    [SerializeField] private Vector2 _tooltipOffset = new Vector2(-100f, 0);
    private readonly Queue<int> _tooltipQueue = new Queue<int>();
    private Vector2 _tooltipPosition;
    

    private void Awake()
    {
        _tooltipPosition = _toolTipText.rectTransform.anchoredPosition;
        RefillTooltipQueue();

        int tooltipIndex = _tooltipQueue.Dequeue();
        _toolTipText.SetText(_toolTips[tooltipIndex]);

        StartCoroutine(SetTextCoroutine());
        StartCoroutine(LoadTargetSceneCoroutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        DOTween.Kill(this);
    }

    private void RefillTooltipQueue()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < _toolTips.Length; i++)
        {
            indices.Add(i);
        }

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        foreach (int index in indices)
        {
            _tooltipQueue.Enqueue(index);
        }
    }

    private IEnumerator SetTextCoroutine()
    {
        WaitForSeconds tooltipDuration = new WaitForSeconds(_tooltipDuration);
        yield return tooltipDuration;
        
        while (true)
        {
            if (_tooltipQueue.Count == 0)
            {
                RefillTooltipQueue();
            }
            int tooltipIndex = _tooltipQueue.Dequeue();

            yield return _toolTipText
                         .DOFade(0f, _toolTipAnimationSpeed)
                         .WaitForCompletion();

            _toolTipText.SetText(_toolTips[tooltipIndex]);
            _toolTipText.rectTransform.anchoredPosition = _tooltipOffset;

            _toolTipText.rectTransform
                        .DOAnchorPos(_tooltipPosition, _toolTipAnimationSpeed)
                        .SetEase(Ease.OutCubic);
            yield return _toolTipText
                         .DOFade(1f, _toolTipAnimationSpeed)
                         .WaitForCompletion();

            yield return tooltipDuration;
        }
    }

    private IEnumerator LoadTargetSceneCoroutine()
    {
        yield return null;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneController.Instance.TargetSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (progress >= 1.0f)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
