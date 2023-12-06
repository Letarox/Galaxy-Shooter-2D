using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private float _enemySpeed = 4f;
    [SerializeField] private int _health = 1;
    private Vector3 _newPosition = new();
    private float _maxYPosition = 7.35f;
    private float _minYPosition = -5.5f;
    private float _maxXPosition = 9.2f;
    private float _minXPosition = -9.2f;
    private Player _player;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_player == null)
            Debug.LogError("Player is NULL on " + gameObject.name);
    }

    private void Update()
    {
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
        TeleportNewPosition();
    }

    private void TeleportNewPosition()
    {
        if (transform.position.y < _minYPosition)
        {
            float randomXPosition = Random.Range(_minXPosition, _maxXPosition);
            _newPosition.Set(randomXPosition, _maxYPosition, 0);
            transform.position = _newPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLaser"))
        {
            Destroy(other.gameObject);  
            if(_player != null)
                _player.AddScorePoints(Random.Range(5, 13));
            TakeDamage();
        }
        else if (other.CompareTag("Player"))
        {            
            Player player = other.GetComponent<Player>();
            if (player != null)
                player.TakeDamage();
            Destroy(gameObject);
        }
    }

    private void TakeDamage()
    {
        _health--;
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
