using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private enum EnemyType
    {
        Default,
        Zigzag
    }

    [SerializeField] private float _enemySpeed = 4f;
    [SerializeField] private int _health = 1;
    [SerializeField] GameObject _laserPrefab;
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] private EnemyType _type;
    private readonly float _maxYPosition = 7.35f;
    private readonly float _minYPosition = -5.5f;
    private readonly float _maxXPosition = 9.2f;
    private readonly float _minXPosition = -9.2f;
    private Vector3 _newPosition;
    private Player _player;
    private Animator _animator;
    private int _onEnemyDeathHash;
    private AudioSource _audioSource;
    private float _canFire = -1f;
    private float _fireRate = 3f;
    private int _direction = 1;
    private bool isMovingSideways = false;
    private Transform _spriteTransform;

    private void Start()
    {
        InitializeEnemyComponents();
        if (_type == EnemyType.Zigzag)
            _ = StartCoroutine(ZigZagMovementRoutine());
        else 
            _ = StartCoroutine(MoveSidewaysRoutine());
    }

    private void InitializeEnemyComponents()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_player == null)
            Debug.LogError("Player is NULL on " + gameObject.name);

        if (_type == EnemyType.Default)
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
                Debug.LogError("Animator is NULL on " + gameObject.name);
            _onEnemyDeathHash = Animator.StringToHash("OnEnemyDeath");
        }
        else
        {
            _spriteTransform = transform.GetChild(0);
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.LogError("Audio Source is NULL on " + gameObject.name);
    }

    private void Update()
    {        
        CalculateMovement();

        if(Time.time >= _canFire && _type != EnemyType.Zigzag)
        {
            FireLaser();
        }
    }

    private void FireLaser()
    {
        _fireRate = Random.Range(3f, 7f);
        _canFire = Time.time + _fireRate;
        _ = Instantiate(_laserPrefab, transform.position, Quaternion.identity);
    }

    private void CalculateMovement()
    {
        switch (_type)
        {
            case EnemyType.Default:
                if (isMovingSideways)
                    transform.Translate(_enemySpeed / 2f * _direction * Time.deltaTime * Vector3.right);

                transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
                break;
            case EnemyType.Zigzag:
                transform.Translate(_enemySpeed * _direction * Time.deltaTime * Vector3.right);
                transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
                break;
            default:
                break;
        }
        
        CheckForBounce();
        TeleportNewPosition();
    }

    private IEnumerator MoveSidewaysRoutine()
    {
        while (true)
        {
            isMovingSideways = true;
            _direction = (Random.Range(0, 2) * 2) - 1;

            yield return new WaitForSeconds(Random.Range(0.5f, 1f));

            _direction = (Random.Range(0, 2) * 2) - 1;

            yield return new WaitForSeconds(Random.Range(0.5f, 1f));

            isMovingSideways = false;

            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
    }

    private IEnumerator ZigZagMovementRoutine()
    {
        while (true)
        {
            _direction = -_direction;
            RotateEnemy();

            yield return new WaitForSeconds(Random.Range(0.75f, 1.5f));
        }
    }

    private void RotateEnemy()
    {
        if (transform.rotation.y == 0)
        {
            Vector3 rotation = new(0f, 180f, -90f);
            _spriteTransform.Rotate(rotation);
        }
        else
        {
            Vector3 rotation = new(0f, 0f, -45f);
            _spriteTransform.Rotate(rotation);
        }
    }

    private void CheckForBounce()
    {
        if (transform.position.x >= _maxXPosition || transform.position.x <= _minXPosition)
        {
            if(_type == EnemyType.Default)
                _direction *= -1;
            else
            {
                _direction *= -1;
                RotateEnemy();
            }
        }
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

    public void IncreaseSpeed(float multiplier)
    {
        _enemySpeed *= multiplier;
        _enemySpeed = Mathf.Clamp(_enemySpeed, 4f, 8f);
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

    public void TakeDamage(int damage)
    {
        _health-= damage;
        if (_health <= 0)
        {
            if(_type == EnemyType.Default)
                _animator.SetTrigger(_onEnemyDeathHash);
            else
                _ = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

            _enemySpeed = 0f;
            _audioSource.Play();            
            SpawnManager.Instance.DestroyEnemy(gameObject);
            if (_type == EnemyType.Default)
                Destroy(gameObject, 2.5f);
            else
                Destroy(gameObject, 0.25f);
            Destroy(this);
        }
    }
}
