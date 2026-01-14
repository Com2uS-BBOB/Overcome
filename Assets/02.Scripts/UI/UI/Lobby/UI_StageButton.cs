using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_StageButton : MonoBehaviour
{
    [Header("Stage Info")]
    [SerializeField] private int _chapter;
    [SerializeField] private int _level;
    
    [Space(10)]
    [Header("UI References")]
    [SerializeField] private Image _stageSelectImage;
    [SerializeField] private TextMeshProUGUI _stageText;
    [SerializeField] private Image[] _starImages;
    private Button _openButton;

    [Space(10)]
    [Header("Background Image Setting")]
    [SerializeField] private bool _isBossStage;
    [SerializeField] private Color _blockColor = Color.gray2;
    [SerializeField] private Color _normalColor = Color.mediumAquamarine;
    [SerializeField] private Color _bossColor = Color.coral;
    
    [Space(10)]
    [Header("Star Image")]
    [SerializeField] private Sprite[] _starSprites = new Sprite[2];
    
    private StageProgress _stageProgress;
    private UI_StageButtons _uiStageButtons;

    private void Awake()
    {
        _uiStageButtons = GetComponentInParent<UI_StageButtons>();
        _openButton = GetComponent<Button>();
        if (_uiStageButtons == null)
        {
            Debug.LogError("[UI_StageButton] UI_StageButtons not found");
        }
        _openButton.onClick.AddListener(SelectStage);
    }
    
    private void Start()
    {
        SetDefaultUI();
        
        StageManager stageManager = StageManager.Instance;
        bool isUnlocked = stageManager.IsStageUnlocked(_chapter, _level);
        SetBackgroundImage(isUnlocked);
        if (!isUnlocked) return;
        
        _stageProgress = stageManager.GetStageProgress(_chapter, _level);
        SetStarUI();
    }
    
    private void SetBackgroundImage(bool isUnlocked)
    {
        if (!isUnlocked)
        {
            _stageSelectImage.color = _blockColor;
            _stageSelectImage.GetComponent<Button>().interactable = false;
        }
        else if (_isBossStage)
        {
            _stageSelectImage.color = _bossColor;
        }
        else
        {
            _stageSelectImage.color = _normalColor;
        }
    }

    private void SetStarUI()
    {
        int nStar = _stageProgress?.StarsEarned ?? 0;
        for (var i = 0; i < _starImages.Length; i++)
        {
            bool isFilled = (i < nStar);
            _starImages[i].gameObject.SetActive(true);
            _starImages[i].sprite = _starSprites[isFilled ? 1 : 0];
            _starImages[i].color = isFilled ? Color.yellow : Color.white;
        }
    }

    private void SetDefaultUI()
    {
        _stageText.text = $"{_chapter}_{_level}";
    }

    private void SelectStage()
    {
        _uiStageButtons.OpenStageInfoPanel(_chapter, _level);
    }
}
