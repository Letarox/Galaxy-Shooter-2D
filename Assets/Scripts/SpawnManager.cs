using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{    
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject[] _powerups;
    [SerializeField] private GameObject _enemyContainer, _powerUpContainer;
    private WaitForSeconds _enemySpawnDelay = new(5f);
    private Vector3 _enemySpawnPosition, _powerUpSpawnPosition;
    private float _maxXPosition = 9.2f;
    private float _minXPosition = -9.2f;
    private float _spawnPositionY = 7.35f;
    private bool _isPlayerAlive = false;

    private void Start()
    {
        StartCoroutine(EnemySpawnRoutine());
        StartCoroutine(SpawnPowerUpRoutine());
    }

    public void StopSpawning()
    {
        _isPlayerAlive = true;
    }

    private IEnumerator EnemySpawnRoutine()
    {
        while (!_isPlayerAlive)
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _enemySpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            Instantiate(_enemyPrefab, _enemySpawnPosition, Quaternion.identity, _enemyContainer.transform);
            yield return _enemySpawnDelay;
        }
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        while (!_isPlayerAlive)
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _powerUpSpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            int randomPowerUP = Random.Range(0, _powerups.Length);
            Instantiate(_powerups[randomPowerUP], _powerUpSpawnPosition, Quaternion.identity, _powerUpContainer.transform);
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }
}
