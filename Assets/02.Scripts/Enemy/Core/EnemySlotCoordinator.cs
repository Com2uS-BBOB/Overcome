using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySlotCoordinator : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private float _radius = 2.5f;
    [SerializeField] private int _slotCount = 6;

    private Dictionary<int, Transform> _occupiedSlots = new();

    public Vector3 GetSlotPosition(int slotIndex)
    {
        float angle = (360f / _slotCount) * slotIndex;
        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 rawPosition = _player.position + direction * _radius;

        // NavMesh 보정
        if (NavMesh.SamplePosition(rawPosition, out var hit, 1f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return rawPosition;
    }

    public int ClaimSlot(Transform enemy)
    {
        for (int i = 0; i < _slotCount; i++)
        {
            if (_occupiedSlots[i] == null)
            {
                _occupiedSlots.Remove(i);
                return i;
            }

            if (!_occupiedSlots.ContainsKey(i))
            {
                _occupiedSlots[i] = enemy;
                return i;
            }
        }
        return -1; // 자리 없음
    }

    public void ReleaseSlot(int slotIndex)
    {
        if (_occupiedSlots.ContainsKey(slotIndex))
        {
            _occupiedSlots.Remove(slotIndex);
        }
    }
}
