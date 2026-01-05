using UnityEngine;

public class EnemyRewardTester : MonoBehaviour
{
    [Header("테스트용 누적 값")]
    [SerializeField] private int _totalScore;
    [SerializeField] private int _totalTime;

    private EnemyBase _enemy;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        _enemy.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        _enemy.OnEnemyKilled -= OnEnemyKilled;
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