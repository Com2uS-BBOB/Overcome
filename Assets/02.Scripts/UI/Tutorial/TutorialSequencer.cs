using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialSequencer : MonoBehaviour
{
    [System.Serializable]
    public class HighlightTarget
    {
        public RectTransform Target;
        [TextArea(2, 4)]
        public string Description;
        public bool Deactivate;
    }

    [Header("Targets")]
    [SerializeField] private List<HighlightTarget> _targets = new List<HighlightTarget>();
    
    [Header("UI References")]
    [SerializeField] private Image _darkOverlay;
    [SerializeField] private RectTransform _highlightMask;
    [SerializeField] private TutorialDescriptionUI _descriptionUI;

    [Header("Settings")]
    [SerializeField] private float _descriptionOffset = 50f;
    [SerializeField] private float _transitionDelay = 0.3f;
    [SerializeField] private float _fadeoutTime = 0.3f;
    [SerializeField] private bool _autoStart = false;

    private Material _darkOverlayMaterial;
    private RectTransform _darkCanvasRect;
    private readonly Vector3[] _worldCorners = new Vector3[4];
    private static readonly int HoleCenter = Shader.PropertyToID("_HoleCenter");
    private static readonly int HoleSize = Shader.PropertyToID("_HoleSize");

    private int _currentIndex;
    private bool _isWaitingForNext;
    private bool _isPlaying;
    private float _prevTimeScale;

    private void Awake()
    {
        if (_darkOverlay == null)
        {
            enabled = false;
            return;
        }
        _darkCanvasRect = _darkOverlay.canvas.GetComponent<RectTransform>();
        _darkOverlayMaterial = _darkOverlay.material;

        _darkOverlay.gameObject.SetActive(false);
        _highlightMask.gameObject.SetActive(false);
        _descriptionUI.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (_autoStart)
        {
            PlayTutorial();
        }
    }

    private void PlayTutorial()
    {
        if (_isPlaying) return;
        UIInputSystem.Instance.BlockInput();

        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        _currentIndex = 0;
        _isPlaying = true;
        StartCoroutine(TutorialSequenceCoroutine());
    }

    private IEnumerator TutorialSequenceCoroutine()
    {
        _darkOverlay.gameObject.SetActive(true);
        var transitionDelay = new WaitForSecondsRealtime(_transitionDelay);
        
        while (_currentIndex < _targets.Count)
        {
            HighlightTarget target = _targets[_currentIndex];

            if (target.Target == null)
            {
                Debug.LogWarning($"[TutorialSequencer] Target at index {_currentIndex} is null");
                _currentIndex++;
                continue;
            }
            target.Target.gameObject.SetActive(true);
            SetHighlightUI(target.Target);
            _descriptionUI.ShowNearTarget(target.Description, target.Target, _descriptionOffset, OnNextClicked);

            _isWaitingForNext = true;
            yield return new WaitUntil(() => !_isWaitingForNext);

            if (target.Deactivate)
            {
                target.Target.gameObject.SetActive(false);
            }
            yield return transitionDelay;
            _currentIndex++;
        }

        EndTutorial();
    }

    private void SetHighlightUI(RectTransform target)
    {
        SetHighlightTransform(target);
        SetHighlightMaterial(target);
    }

    private void SetHighlightTransform(RectTransform target)
    {
        _highlightMask.gameObject.SetActive(true);
        _highlightMask.pivot = target.pivot;
        _highlightMask.anchorMin = target.anchorMin;
        _highlightMask.anchorMax = target.anchorMax;
        _highlightMask.position = target.position;
        _highlightMask.anchoredPosition = target.anchoredPosition;
        _highlightMask.sizeDelta = target.sizeDelta;
    }

    private void SetHighlightMaterial(RectTransform target)
    {
        target.GetWorldCorners(_worldCorners);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _darkCanvasRect,
            _worldCorners[0],
            null,
            out Vector2 minLocal
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _darkCanvasRect,
            _worldCorners[2],
            null,
            out Vector2 maxLocal
        );

        Vector2 canvasSize = _darkCanvasRect.sizeDelta;
        Vector2 uvOffset = new Vector2(0.5f, 0.5f);
        Vector2 minUV = (minLocal / canvasSize) + uvOffset;
        Vector2 maxUV = (maxLocal / canvasSize) + uvOffset;
        Vector2 centerUV = (minUV + maxUV) * 0.5f;
        Vector2 sizeUV = (maxUV - minUV) * 0.5f;

        _darkOverlayMaterial.SetVector(HoleCenter, centerUV);
        _darkOverlayMaterial.SetVector(HoleSize, sizeUV);
    }

    private void OnNextClicked()
    {
        _isWaitingForNext = false;
    }

    private void EndTutorial()
    {
        _isPlaying = false;
        _descriptionUI.Hide();

        _darkOverlay.DOFade(0, _fadeoutTime).OnComplete(() =>
        {
            _darkOverlay.gameObject.SetActive(false);
            _highlightMask.gameObject.SetActive(false);
        });

        _darkOverlayMaterial.SetVector(HoleCenter, Vector4.zero);
        _darkOverlayMaterial.SetVector(HoleSize, Vector4.zero);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = _prevTimeScale;

        UIInputSystem.Instance.UnblockInput();
        
        GameEventHandler.GameStart();
    }

    private void SkipTutorial()
    {
        if (!_isPlaying) return;

        StopAllCoroutines();
        EndTutorial();
    }
}