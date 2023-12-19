using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    [SerializeField] private GameObject[] _enemiesPrefab;
    [SerializeField] private GameObject[] _powerups, _utilitiesPowerups;
    [SerializeField] private GameObject _enemyContainer, _powerUpContainer;
    private readonly WaitForSeconds _powerupSpawnDelay = new(6f);    
    private readonly float _maxXPosition = 9.2f;
    private readonly float _minXPosition = -9.2f;
    private readonly float _spawnPositionY = 7.35f;
    private Vector3 _enemySpawnPosition, _powerUpSpawnPosition, _utilityPowerUpSpawnPosition;
    private bool _isPlayerAlive = false;
    private Player _player;
    private int _currentWave = 1;
    private int _enemiesPerWave = 5;
    private float _spawnInterval = 5.0f;
    private float _enemySpeedMultiplier = 1f;
    private float _spawnIntervalDecreasePercentage = 10f;
    private List<GameObject> _activeEnemies = new();

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
            Debug.LogError("Player is NULL on the Spawn Manager.");
    }

    public void StopSpawning()
    {
        _isPlayerAlive = true;
    }

    public void StartSpawning()
    {
        _ = StartCoroutine(EnemySpawnRoutine());
        _ = StartCoroutine(SpawnPowerUpRoutine());
        _ = StartCoroutine(SpawnUtilityPowerUpRoutine());
    }

    private IEnumerator EnemySpawnRoutine()
    {
        yield return UIManager.Instance.NextWaveSpawnRoutine(_currentWave);
        while (!_isPlayerAlive)
        {
            for (int i = 0; i < _enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(_spawnInterval);
            }

            while (_activeEnemies.Count > 0)
            {
                yield return null;
            }

            _currentWave++;
            _enemiesPerWave += 2;
            _spawnInterval *= 1.0f - (_spawnIntervalDecreasePercentage / 100.0f);
            _enemySpeedMultiplier += 0.05f;

            UIManager.Instance.UpdateWaveText(_currentWave);
            yield return UIManager.Instance.NextWaveSpawnRoutine(_currentWave);
        }
    }

    private void SpawnEnemy()
    {
        float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
        _enemySpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
        int randomEnemy = Random.Range(0, _enemiesPrefab.Length);
        GameObject enemy = Instantiate(_enemiesPrefab[randomEnemy], _enemySpawnPosition, Quaternion.identity, _enemyContainer.transform);
        _activeEnemies.Add(enemy);
        EnemyBehaviour enemyBehaviour = enemy.GetComponent<EnemyBehaviour>();
        if (enemyBehaviour != null)
            enemyBehaviour.IncreaseSpeed(_enemySpeedMultiplier);
    }

    public void DestroyEnemy(GameObject enemy)
    {
        _ = _activeEnemies.Remove(enemy);
    }

    private IEnumerator SpawnPowerUpRoutine()
    {
        yield return _powerupSpawnDelay;
        while (!_isPlayerAlive)
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _powerUpSpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            int randomPowerUP = Random.Range(0, _powerups.Length);
            if(randomPowerUP == 3)
                randomPowerUP = Random.Range(0, _powerups.Length);
            _ = Instantiate(_powerups[randomPowerUP], _powerUpSpawnPosition, Quaternion.identity, _powerUpContainer.transform);
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }

    private IEnumerator SpawnUtilityPowerUpRoutine()
    {
        yield return _powerupSpawnDelay;
        while (!_isPlayerAlive)
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _utilityPowerUpSpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            if(_player.GetPlayerLives() < 3)
            {
                int randomPowerUP = Random.Range(0, _utilitiesPowerups.Length);
                _ = Instantiate(_utilitiesPowerups[randomPowerUP], _utilityPowerUpSpawnPosition, Quaternion.identity, _powerUpContainer.transform);
            }
            else
                _ = Instantiate(_utilitiesPowerups[0], _utilityPowerUpSpawnPosition, Quaternion.identity, _powerUpContainer.transform);
            yield return new WaitForSeconds(Random.Range(5, 11));
        }
    }

    public EnemyBehaviour[] GetAllEnemyBehaviors()
    {
        EnemyBehaviour[] enemyBehaviours = GetComponentsInChildren<EnemyBehaviour>();

        return enemyBehaviours;
    }
}
