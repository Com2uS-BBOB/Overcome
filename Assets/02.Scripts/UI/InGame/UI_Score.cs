using TMPro;
using UnityEngine;
using DG.Tweening;

public class UI_Score : MonoBehaviour
{
    [Header("Current Score")]
    [SerializeField] private GameObject _currentScoreObject;
    private TextMeshProUGUI _scoreText;
    private DOTweenAnimation _scoreAnimation;
    
    [Space(10)]
    [Header("High Score")]
    [SerializeField] private GameObject _highScoreObject;
    private TextMeshProUGUI _highScoreText;
    private DOTweenAnimation _highScoreAnimation;
    
    [Space(10)]
    [Header("Rank")]
    [SerializeField] private GameObject _rankObject;
    private TextMeshProUGUI _rankText;
    private DOTweenAnimation _rankAnimation;
    private string _prevGrade = "C"; 
    
    private bool _isHighScore = false;
    
    private void Awake()
    {
        // Current Score 바인딩
        _scoreText = _currentScoreObject.GetComponent<TextMeshProUGUI>();
        _scoreAnimation = _currentScoreObject.GetComponent<DOTweenAnimation>();

        // High Score 바인딩
        _highScoreText = _highScoreObject.GetComponent<TextMeshProUGUI>();
        _highScoreAnimation = _highScoreObject.GetComponent<DOTweenAnimation>();
        
        _rankText = _rankObject.GetComponent<TextMeshProUGUI>();
        _rankAnimation = _rankObject.GetComponent<DOTweenAnimation>();
    }

    private void Start()
    {
        ScoreSystem system = ScoreSystem.Instance;
        system.OnScoreChanged += UpdateScoreUI;
        system.BreakHighScore += SetNewHighScore;

        // 초기화 시에는 애니메이션 없이 텍스트만 업데이트
        _scoreText.text = system.CurrentScore.ToString();
        _highScoreText.text = system.HighScore.ToString();
        _rankText.text = system.GetGradeConfig().Grade;
    }

    private void OnDisable()
    {
        ScoreSystem system = ScoreSystem.Instance;
        if (system == null) return;
        system.OnScoreChanged -= UpdateScoreUI;
        system.BreakHighScore -= SetNewHighScore;
    }

    private void UpdateScoreUI(int currentScore, int highScore)
    {
        UpdateCurrentScoreUI(currentScore);
        UpdateGradeUI();
        if (!_isHighScore) return;
        UpdateHighScoreUI(highScore);
    }
    private void UpdateGradeUI()
    {
        GradeConfig grade = ScoreSystem.Instance.GetGradeConfig();
        if (_prevGrade == grade.Grade) return;
        _prevGrade = grade.Grade;
        _rankText.text = grade.Grade;
        _rankAnimation.DORestart();
    }

    private void SetNewHighScore()
    {
        _isHighScore = true;
        _highScoreAnimation.DOPlay();
    }
    
    private void UpdateCurrentScoreUI(int score)
    {
        _scoreText.text = score.ToString();
        PlayScoreEvent();
    }

    private void UpdateHighScoreUI(int score)
    {
        _highScoreText.text = score.ToString();
    }

    private void PlayScoreEvent()
    {
        _scoreAnimation.DORestart();
    }
}
