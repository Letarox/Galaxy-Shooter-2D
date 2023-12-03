using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{    
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _enemyContainer;
    private WaitForSeconds _enemySpawnDelay;
    private Vector3 _spawnPosition;
    private float _maxXPosition = 9.2f;
    private float _minXPosition = -9.2f;
    private bool _stopSpawning = false;

    void Start()
    {
        _enemySpawnDelay = new WaitForSeconds(5f);
        StartCoroutine(EnemySpawnRoutine());
    }

    void Update()
    {
        
    }

    public void StopSpawning()
    {
        _stopSpawning = true;
    }

    IEnumerator EnemySpawnRoutine()
    {
        while (!_stopSpawning)
        {
            _spawnPosition.Set(Random.Range(_minXPosition, _maxXPosition), 7.35f, 0f);
            Instantiate(_enemyPrefab, _spawnPosition, Quaternion.identity, _enemyContainer.transform);
            yield return _enemySpawnDelay;
        }
    }
}
