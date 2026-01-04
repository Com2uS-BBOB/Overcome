using UnityEngine;

public class EnemyRewardTester : MonoBehaviour
{
    [Header("테스트용 누적 값")]
    [SerializeField] private int _totalScore;
    [SerializeField] private int _totalTime;

    private void OnEnable()
    {
        EnemyBase.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled(EnemyStatData stat)
    {
        _totalScore += stat.Score;
        _totalTime += stat.Playtime;

        Debug.Log(
            $"[Reward Test]\n" +
            $"- 처치한 적: {stat.EnemyType}\n" +
            $"- 획득 점수: +{stat.Score} (총 {_totalScore})\n" +
            $"- 획득 시간: +{stat.Playtime} (총 {_totalTime})"
        );
    }
}