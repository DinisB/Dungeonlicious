using System.Collections.Generic;
using Dungeonlicious.Assets.Script;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockOnScript : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    private InputAction _lockOnAction;
    [SerializeField] private List<GameObject> _targets = new List<GameObject>();
    [SerializeField] private int currentTargetIndex = 0;

    private void Start()
    {
        _lockOnAction = InputSystem.actions.FindAction("LockOn");
    }

    void OnEnable()
    {
        EnemyController.OnEnemyDeath += (enemy) => EnemyDied(enemy);
    }

    void OnDisable()
    {
        EnemyController.OnEnemyDeath -= (enemy) => EnemyDied(enemy);
    }

    private void Update()
    {
        if (_lockOnAction.WasPressedThisFrame() && _targets.Count > 0)
        {
            if (!_cameraController.IsLocked)
            {
                currentTargetIndex = 0;
                _cameraController.LockOnTarget(_targets[currentTargetIndex].transform);
            }
            else
            {
                currentTargetIndex = (currentTargetIndex + 1) % _targets.Count;
                _cameraController.LockOnTarget(_targets[currentTargetIndex].transform);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8 && !_targets.Contains(other.gameObject))
        {
            _targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            _targets.Remove(other.gameObject);

            _cameraController.IsLocked = false;

            if (_targets.Count == 0)
            {
                currentTargetIndex = 0;
            }
            else if (_cameraController.GetCurrentTarget() == other.gameObject)
            {
                currentTargetIndex %= _targets.Count;
                _cameraController.LockOnTarget(_targets[currentTargetIndex].transform);
            }
        }
    }

    private void EnemyDied(GameObject enemy)
    {
        if (_targets.Contains(enemy))
        {
            _targets.Remove(enemy);

            _cameraController.IsLocked = false;
        }
    }
}
