using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{    
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject[] _powerups;
    [SerializeField] private GameObject _enemyContainer, _powerUpContainer;
    private readonly WaitForSeconds _enemySpawnDelay = new(5f);    
    private readonly WaitForSeconds _initialDelay = new(2f);    
    private readonly float _maxXPosition = 9.2f;
    private readonly float _minXPosition = -9.2f;
    private readonly float _spawnPositionY = 7.35f;
    private Vector3 _enemySpawnPosition, _powerUpSpawnPosition;
    private bool _isPlayerAlive = false;

    public void StopSpawning()
    {
        _isPlayerAlive = true;
    }

    public void StartSpawning()
    {
        _ = StartCoroutine(EnemySpawnRoutine());
        _ = StartCoroutine(SpawnPowerUpRoutine());
    }

    private IEnumerator EnemySpawnRoutine()
    {
        yield return _initialDelay;
        while (!_isPlayerAlive)
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _enemySpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            _ = Instantiate(_enemyPrefab, _enemySpawnPosition, Quaternion.identity, _enemyContainer.transform);
            yield return _enemySpawnDelay;
        }
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        yield return _initialDelay;
        while (!_isPlayerAlive)
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _powerUpSpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            int randomPowerUP = Random.Range(0, _powerups.Length);
            _ = Instantiate(_powerups[randomPowerUP], _powerUpSpawnPosition, Quaternion.identity, _powerUpContainer.transform);
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }
}
