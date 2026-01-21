using System;
using UnityEngine;

public static class GameEventHandler
{
    public static bool IsOnGame = false;
    public static event Action OnGameStart;
    public static event Action OnGameEnd;
    public static event Action<int> OnGameClear;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
 
    public static void GameStart() => OnGameStart?.Invoke();
    public static void GameEnd() => OnGameEnd?.Invoke();
    public static void GameClear(int additionalScore) => OnGameClear?.Invoke(additionalScore);
    public static void GamePause() => OnGamePause?.Invoke();
    public static void GameResume() => OnGameResume?.Invoke();
}
