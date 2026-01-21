using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _hudUIObject;

    [Header("Game Systems")]
    [SerializeField] private TimeSystem _timeSystem;
    [SerializeField] private ComboSystem _comboSystem;
    [SerializeField] private ScoreSystem _scoreSystem;
    [SerializeField] private KillLogSystem _killLogSystem;
    
    private void OnEnable()
    {
        GameEventHandler.OnGameStart += OnGameStart;
        GameEventHandler.OnGamePause += OnGamePause;
        GameEventHandler.OnGameEnd += OnGameEnd;
        GameEventHandler.OnGameResume += OnGameResume;
    }

    private void OnDisable()
    {
        GameEventHandler.OnGameStart -= OnGameStart;
        GameEventHandler.OnGamePause -= OnGamePause;
        GameEventHandler.OnGameEnd -= OnGameEnd;
        GameEventHandler.OnGameResume -= OnGameResume;
    }

    private void Start()
    {
        OnGameStart();
    }
    
    private void OnGameStart()
    {
        SetCursor(false);
        _timeSystem?.GameStart();
        _comboSystem?.GameStart();
        _scoreSystem?.GameStart();
        _killLogSystem?.GameStart();
        _hudUIObject.SetActive(true);
    }
    
    private void OnGamePause()
    {
        SetCursor(true);
    }

    private void OnGameResume()
    {
        SetCursor(false);
    }
    
    private void OnGameEnd()
    {
        SetCursor(true);
        _timeSystem?.GameEnd();
        _comboSystem?.GameEnd();
        _scoreSystem?.GameEnd();
        _killLogSystem?.GameEnd();
    }

    private void SetCursor(bool active)
    {
        if (active)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
