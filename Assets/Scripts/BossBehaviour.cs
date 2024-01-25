using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehaviour : MonoBehaviour
{
    private enum BossState
    {
        Sideways,
        Down
    }

    [SerializeField] private float _speed = 5f;
    [SerializeField] private GameObject _laserPrefab, _explosionPrefab;
    [SerializeField] private Transform _spriteTransform;
    private int _health = 10;
    private bool _isInvulnerable = false;
    private bool _readyForCombat = false;
    private BossState _state;
    private float _speedMultiplier = 1f;
    private Player _player;
    private int _direction = 1;
    private readonly float _maxYPosition = 5f;
    private readonly float _minYPosition = -3f;
    private readonly float _maxXPosition = 9f;
    private readonly float _minXPosition = -9f;
    private readonly float _laserYPosition = 7.5f;
    private readonly WaitForSeconds _flickDelay = new(0.17f);

    private void Start()
    {
        _state = BossState.Sideways;
        _isInvulnerable = true;
    }

    private void Update()
    {
        if(_readyForCombat)
            CalculateMovement();
        else
        {
            transform.Translate(_speed / 3f * Time.deltaTime * Vector3.down);
            if (transform.position.y <= _maxYPosition)
            {
                _readyForCombat = true;
                _isInvulnerable = false;
            }
        }
    }

    private void CalculateMovement()
    {
        switch (_state)
        {
            case BossState.Sideways:
                transform.Translate(_speed * _speedMultiplier * _direction * Time.deltaTime * Vector3.right);
                if(transform.position.x >= _maxXPosition || transform.position.x <= _minXPosition)
                {
                    ChangeDirection();
                }
                CheckPlayerPosition();
                break;
            case BossState.Down:
                transform.Translate(_speed * 1.5f * _speedMultiplier * _direction * Time.deltaTime * Vector3.down);
                if (transform.position.y <= _minYPosition)
                {
                    ShootLasers();
                    ChangeDirection();
                }
                if(transform.position.y >= _maxYPosition)
                {
                    _direction = (Random.Range(0, 2) * 2) - 1;
                    IncreaseSpeed();
                    ChangeState(BossState.Sideways);
                }
                break;
        }
    }

    private void IncreaseSpeed()
    {
        _speedMultiplier += 0.05f;
    }

    private void ChangeDirection()
    {
        _direction *= -1;
    }

    private void ChangeState(BossState newState)
    {
        _state = newState;
    }

    private void AttackPlayer()
    {
        ChangeState(BossState.Down);
        _direction = 1;
    }

    private void ShootLasers()
    {
        Vector2[] positions = new Vector2[] { new(-9.0f, -5f), new(-4.5f, -0.5f), new(0f, 4.5f), new(5.0f, 9.0f) };

        foreach (Vector2 laserPosition in positions)
        {
            float randomX = Random.Range(laserPosition.x, laserPosition.y);
            Vector3 laserSpawnPosition = new(randomX, _laserYPosition, 0.0f);
            _ = Instantiate(_laserPrefab, laserSpawnPosition, Quaternion.identity);
        }
    }

    private void CheckPlayerPosition()
    {
        if(_player != null)
        {
            Vector2 toPlayer = _player.transform.position - transform.position;
            float angle = Vector2.SignedAngle(-transform.up, toPlayer.normalized);
            float alignmentThreshold = 5f;
            float verticalAlignment = Mathf.Abs(toPlayer.x) / Mathf.Abs(toPlayer.y);

            if (Mathf.Abs(angle) < alignmentThreshold && verticalAlignment < 1f)
                AttackPlayer();
        }
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        if (_player != null)
            return;
        else
            Debug.LogError("Player is NULL on " + gameObject.name);
    }

    private IEnumerator BossInvulnerabilityRoutine()
    {
        _isInvulnerable = true;
        for (int i = 3; i >= 0; i--)
        {
            _spriteTransform.gameObject.SetActive(false);
            yield return _flickDelay;
            _spriteTransform.gameObject.SetActive(true);
            yield return _flickDelay;
        }
        _isInvulnerable = false;
    }

    public void TakeDamage()
    {
        if (_isInvulnerable)
            return;

        _health -= 1;
        _ = StartCoroutine(BossInvulnerabilityRoutine());

        if (_health <= 0)
        {
            _ = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            SpawnManager.Instance.DestroyEnemy(gameObject);
            Destroy(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLaser"))
        {
            Destroy(other.gameObject);
            TakeDamage();
        }
        else if (other.CompareTag("Player"))
        {
            if (_player != null)
                _player.TakeDamage();
            TakeDamage();
        }
    }
}
