using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private GameObject[] _enemiesPrefab;    
    [SerializeField] private GameObject[] _powerups;
    [SerializeField] private GameObject _enemyContainer, _powerUpContainer, _playerLaserContainer, _enemyLaserContainer;
    private readonly WaitForSeconds _powerupSpawnDelay = new(6f);
    private readonly Vector3 _bossSpawnPosition = new(0f, 9f, 0f);
    private readonly float _maxXPosition = 9.2f;
    private readonly float _minXPosition = -9.2f;
    private readonly float _spawnPositionY = 7.35f;
    private Vector3 _enemySpawnPosition, _powerUpSpawnPosition;
    private bool _isPlayerAlive = false;
    private Player _player;
    private int _currentWave = 1;
    private int _enemiesPerWave = 5;
    private float _spawnInterval = 5.0f;
    private float _enemySpeedMultiplier = 1f;
    private float _spawnIntervalDecreasePercentage = 10f;
    private List<GameObject> _activeEnemies = new();
    private int _weightedTotal;

    public GameObject PlayerLaserContainer => _playerLaserContainer;
    public List<GameObject> ActiveEnemies => _activeEnemies;

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
    }

    private IEnumerator EnemySpawnRoutine()
    {
        yield return UIManager.Instance.NextWaveSpawnRoutine(_currentWave);
        while (!_isPlayerAlive && (!GameManager.Instance.IsGameOver || !GameManager.Instance.PlayerWon))
        {
            if(_currentWave == 5 && _activeEnemies.Count == 0)
            {
                SpawnBoss();

                while (_activeEnemies.Count > 0)
                {
                    yield return null;
                }

                GameManager.Instance.Victory();
                break;
            }
            else
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
    }

    private GameObject ChooseRandomEnemy()
    {
        _weightedTotal = 0;

        int[] enemyTable =
        {
            25, // Default Enemy
            25, // Zigzag Enemy
            15, // Aggressive Enemy
            15, // Smart Enemy
            15, // Evasive Enemy
        };

        foreach (var enemy in enemyTable)
            _weightedTotal += enemy;

        int randomNumber = Random.Range(0, _weightedTotal);
        int index = 0;

        foreach (var weight in enemyTable)
        {
            if (randomNumber <= weight)
                return _enemiesPrefab[index];

            index++;
            randomNumber -= weight;
        }

        return _enemiesPrefab[0];
    }

    private GameObject ChooseRandomPowerUp()
    {
        _weightedTotal = 0;

        int[] powerupTable =
        {
            30, //TripleShot
            40, //Speed            
            20, //Shield
            5,  //Special
            10, //Slow
            60, //Ammo
            5,  //Life
            10  //Homing
        };

        foreach (var powerup in powerupTable)
            _weightedTotal += powerup;

        int randomNumber = Random.Range(0, _weightedTotal);
        int index = 0;

        foreach (var weight in powerupTable)
        {
            if (randomNumber <= weight)
                return _powerups[index];

            index++;
            randomNumber -= weight;
        }

        return _powerups[0];
    }

    private void SpawnBoss()
    {
        GameObject boss = Instantiate(_bossPrefab, _bossSpawnPosition, Quaternion.identity, _enemyContainer.transform);
        _activeEnemies.Add(boss);
        BossBehaviour bossBehaviour = boss.GetComponent<BossBehaviour>();
        if (bossBehaviour != null)
            bossBehaviour.SetPlayer(_player);
    }

    private void SpawnEnemy()
    {
        float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
        _enemySpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
        GameObject enemy = Instantiate(ChooseRandomEnemy(), _enemySpawnPosition, Quaternion.identity, _enemyContainer.transform);
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
        while (!_isPlayerAlive && (!GameManager.Instance.IsGameOver || !GameManager.Instance.PlayerWon))
        {
            float randomPositionX = Random.Range(_minXPosition, _maxXPosition);
            _powerUpSpawnPosition.Set(randomPositionX, _spawnPositionY, 0f);
            _ = Instantiate(ChooseRandomPowerUp(), _powerUpSpawnPosition, Quaternion.identity, _powerUpContainer.transform);
            yield return new WaitForSeconds(Random.Range(3, 8));
        }
    }

    public EnemyBehaviour[] GetAllEnemyBehaviors()
    {
        EnemyBehaviour[] enemyBehaviours = GetComponentsInChildren<EnemyBehaviour>();

        return enemyBehaviours;
    }
}
