using TMPro;
using UnityEngine;
using DG.Tweening;

public class UI_Score : MonoBehaviour
{
    [Header("Current Score")]
    [SerializeField] private GameObject _currentScoreObject;
    private TextMeshProUGUI _scoreText;
    private DOTweenAnimation[] _scoreAnimations;
    
    [Space(10)]
    [Header("High Score")]
    [SerializeField] private GameObject _highScoreObject;
    private TextMeshProUGUI _highScoreText;
    private DOTweenAnimation _highScoreAnimation;
    
    private bool _isHighScore = false;
    
    private void Awake()
    {
        // Current Score 바인딩
        _scoreText = _currentScoreObject.GetComponent<TextMeshProUGUI>();
        _scoreAnimations = _currentScoreObject.GetComponents<DOTweenAnimation>();
        foreach (DOTweenAnimation anim in _scoreAnimations)
        {
            anim.autoKill = false;
        }

        // High Score 바인딩
        _highScoreText = _highScoreObject.GetComponent<TextMeshProUGUI>();
        _highScoreAnimation = _highScoreObject.GetComponent<DOTweenAnimation>();
    }

    private void Start()
    {
        ScoreSystem system = ScoreSystem.Instance;
        system.OnScoreChanged += UpdateScoreUI;
        system.BreakHighScore += SetNewHighScore;

        // 초기화 시에는 애니메이션 없이 텍스트만 업데이트
        _scoreText.text = system.CurrentScore.ToString();
        _highScoreText.text = system.HighScore.ToString();
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
        if (!_isHighScore) return;
        UpdateHighScoreUI(highScore);
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
        foreach (DOTweenAnimation anim in _scoreAnimations)
        {
            anim.DORestart();
        }
    }
}
