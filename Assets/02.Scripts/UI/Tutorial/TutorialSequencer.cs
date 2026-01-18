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
    }

    [Header("Targets")]
    [SerializeField] private List<HighlightTarget> _targets = new List<HighlightTarget>();

    [Header("UI References")]
    [SerializeField] private Image _darkOverlay;
    private RectTransform _darkCanvasRect;
    [SerializeField] private RectTransform _highlightMask;
    [SerializeField] private TutorialDescriptionUI _descriptionUI;

    [Header("Settings")]
    [SerializeField] private float _descriptionOffset = 50f;
    [SerializeField] private bool _autoStart = false;

    // Mask
    private Material _darkOverlayMaterial;
    private readonly Vector3[] _worldCorners = new Vector3[4];
    private static readonly int HoleCenter = Shader.PropertyToID("_HoleCenter");
    private static readonly int HoleSize = Shader.PropertyToID("_HoleSize");
    
    // Sequence 
    private int _currentIndex = 0;
    private bool _isWaitingForNext = false;
    private bool _isPlaying = false;
    private readonly WaitForSeconds _delay = new WaitForSeconds(0.3f);

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
    }

    private void Start()
    {
        _descriptionUI.gameObject.SetActive(false);
        if (_autoStart)
        {
            PlayTutorial();
        }
    }

    private void OnDestroy()
    {
        if (_darkOverlayMaterial)
        {
            Destroy(_darkOverlayMaterial);
        }
    }

    public void PlayTutorial()
    {
        if (_isPlaying) return;

        _currentIndex = 0;
        _isPlaying = true;
        StartCoroutine(TutorialSequenceCoroutine());
    }

    private IEnumerator TutorialSequenceCoroutine()
    {
        _darkOverlay.gameObject.SetActive(true);
        yield return _delay;

        while (_currentIndex < _targets.Count)
        {
            var target = _targets[_currentIndex];

            if (target.Target == null)
            {
                Debug.LogWarning($"[TutorialSequencer] {_currentIndex} target is null");
                _currentIndex++;
                continue;
            }

            SetHighlightUI(target.Target);

            _descriptionUI.ShowNearTarget(target.Description, target.Target, _descriptionOffset, OnNextClicked);

            _isWaitingForNext = true;
            yield return new WaitUntil(() => !_isWaitingForNext);

            yield return _delay;
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

        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            _darkCanvasRect,
            _worldCorners[0],
            null,
            out Vector2 minLocal
        );
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            _darkCanvasRect,
            _worldCorners[2],
            null,
            out Vector2 maxLocal
        );

        Vector2 canvasSize = _darkCanvasRect.sizeDelta;
        Vector2 minUV = (minLocal / canvasSize) + new Vector2(0.5f, 0.5f);
        Vector2 maxUV = (maxLocal / canvasSize) + new Vector2(0.5f, 0.5f);
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

        _darkOverlay.DOFade(0, 0.3f)
                    .OnComplete(() =>
                    {
                        _darkOverlay.gameObject.SetActive(false);
                        _highlightMask.gameObject.SetActive(false);
                    });
    }

    public void SkipTutorial()
    {
        if (!_isPlaying) return;

        StopAllCoroutines();
        EndTutorial();
    }
}