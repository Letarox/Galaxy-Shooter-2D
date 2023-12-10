using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private float _enemySpeed = 4f;
    [SerializeField] private int _health = 1;    
    private readonly float _maxYPosition = 7.35f;
    private readonly float _minYPosition = -5.5f;
    private readonly float _maxXPosition = 9.2f;
    private readonly float _minXPosition = -9.2f;
    private Vector3 _newPosition;
    private Player _player;
    private Animator _animator;
    private int _onEnemyDeathHash;
    private AudioSource _audioSource;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_player == null)
            Debug.LogError("Player is NULL on " + gameObject.name);

        _animator = GetComponent<Animator>();
        if(_animator == null)
            Debug.LogError("Animator is NULL on " + gameObject.name);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.LogError("Audio Source is NULL on " + gameObject.name);

        _onEnemyDeathHash = Animator.StringToHash("OnEnemyDeath");
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
            TakeDamage(1);
        }
        else if (other.CompareTag("Player"))
        {
            if (_player != null)
                _player.TakeDamage();
            TakeDamage(100);
        }
    }

    private void TakeDamage(int damage)
    {
        _health-= damage;
        if (_health <= 0)
        {
            _animator.SetTrigger(_onEnemyDeathHash);
            _enemySpeed = 0f;
            _audioSource.Play();
            Destroy(gameObject, 2.5f);
        }
    }
}
