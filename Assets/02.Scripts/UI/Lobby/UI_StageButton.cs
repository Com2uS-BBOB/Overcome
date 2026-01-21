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
    private LobbyScene _lobbyScene;

    [Space(10)]
    [Header("Background Image Setting")]
    [SerializeField] private bool _isBossStage;
    [SerializeField] private Color _blockColor = Color.gray;
    [SerializeField] private Color _normalColor = Color.green;
    [SerializeField] private Color _bossColor = Color.yellow;
    
    [Space(10)]
    [Header("Star Image")]
    [SerializeField] private Sprite[] _starSprites = new Sprite[2];
    
    private StageProgress _stageProgress;

    private void Awake()
    {
        _openButton = GetComponent<Button>();
        _openButton.onClick.AddListener(SelectStage);
        _lobbyScene = FindFirstObjectByType<LobbyScene>();
    }
    
    private void Start()
    {
        SetDefaultText();
        SetProgressUI();
    }

    private void OnEnable()
    {
        SetProgressUI();
    }
    
    private void SetBackgroundImage(bool isUnlocked)
    {
        if (!isUnlocked)
        {
            _stageSelectImage.color = _blockColor;
            _openButton.interactable = false;
        }
        else if (_isBossStage)
        {
            _stageSelectImage.color = _bossColor;
            _openButton.interactable = true;
        }
        else
        {
            _stageSelectImage.color = _normalColor;
            _openButton.interactable = true;
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

    private void SetDefaultText()
    {
        _stageText.text = $"{_chapter}-{_level}";
    }

    private void SetProgressUI()
    {
        StageManager stageManager = StageManager.Instance;
        if (stageManager == null) return;
        
        bool isUnlocked = stageManager.IsStageUnlocked(_chapter, _level);
        SetBackgroundImage(isUnlocked);
        _stageProgress = stageManager.GetStageProgress(_chapter, _level);
        SetStarUI();
    }

    private async void SelectStage()
    {
        UI_StageInfo stageInfo = await UIController.Instance.OpenUI<UI_StageInfo>();
        stageInfo.OpenStageInfoPanel(_chapter, _level);
        stageInfo.OnGameStartRequested += _lobbyScene.HandleGameStart;
    }
}
