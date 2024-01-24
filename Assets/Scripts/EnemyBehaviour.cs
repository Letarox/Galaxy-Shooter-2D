using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private enum EnemyType
    {
        Default,
        Zigzag,
        Aggressive,
        Smart,
        Evasive
    }

    [SerializeField] private float _enemySpeed = 4f;
    [SerializeField] private int _health = 1;
    [SerializeField] GameObject _laserPrefab;
    [SerializeField] GameObject _horizontalLaserPrefab;
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] GameObject _shieldPrefab;
    [SerializeField] GameObject[] _turboPrefab;
    [SerializeField] private EnemyType _type;    
    private readonly float _maxYPosition = 7.35f;
    private readonly float _minYPosition = -5.5f;
    private readonly float _maxXPosition = 9.2f;
    private readonly float _minXPosition = -9.2f;
    private Vector3 _laserOffset = new(0f, -1.4f, 0f);
    private Vector3 _newPosition;
    private PowerUp[] _allPowerUps;
    private Player _player;
    private Animator _animator;
    private int _onEnemyDeathHash;
    private AudioSource _audioSource;
    private float _canFire = -1f;
    private float _canFireAtPowerup = -1f;
    private float _fireRate = 3f;
    private float _avoidDistance = 3.0f;
    private int _direction = 1;
    private bool isMovingSideways = false;
    private bool _isShieldActive = false;
    private Transform _spriteTransform;

    private void Start()
    {
        InitializeEnemyComponents();
        InitializeMovement();

        if (Random.Range(0, 11) <= 3)
        {
            SetShieldState(true);
        }

        DetermineIfPowerupInfront();
    }

    private void InitializeMovement()
    {
        switch (_type)
        {
            case EnemyType.Default:
                _ = StartCoroutine(MoveSidewaysRoutine());
                break;
            case EnemyType.Zigzag:
                _ = StartCoroutine(ZigZagMovementRoutine());
                break;
            case EnemyType.Aggressive:
                break;
            default:
                break;
        }
    }

    private void InitializeEnemyComponents()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_player == null)
            Debug.LogError("Player is NULL on " + gameObject.name);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.LogError("Audio Source is NULL on " + gameObject.name);

        switch (_type)
        {
            case EnemyType.Default:
                _animator = GetComponent<Animator>();
                if (_animator == null)
                    Debug.LogError("Animator is NULL on " + gameObject.name);
                _onEnemyDeathHash = Animator.StringToHash("OnEnemyDeath");
                break;
            case EnemyType.Smart:
                _spriteTransform = transform;
                break;
            default:
                _spriteTransform = transform.GetChild(0);
                break;
        }
    }

    private void Update()
    {
        CalculateMovement();
        if (Time.time >= _canFire && _type != EnemyType.Zigzag)
        {
            if (_type != EnemyType.Smart)
            {
                DetermineIfPowerupInfront();
                FireLaser(true);
            }
            else
            {
                Vector2 toPlayer = _player.transform.position - transform.position;
                float angle = Vector2.SignedAngle(-transform.right, toPlayer.normalized);
                float alignmentThreshold = 5f;
                float horizontalAlignment = Mathf.Abs(toPlayer.y) / Mathf.Abs(toPlayer.x);

                if (Mathf.Abs(angle) < alignmentThreshold && horizontalAlignment < 1f)
                    FireLaser(true);
            }
        }
    }
    private void DetermineIfPowerupInfront()
    {
        _allPowerUps = SpawnManager.Instance.transform.GetChild(1).GetComponentsInChildren<PowerUp>();
        for (int i = 0; i < _allPowerUps.Length; i++)
        {
            Vector2 toPowerup = _allPowerUps[i].transform.position - transform.position;
            float angle = Vector2.SignedAngle(-transform.up, toPowerup.normalized);
            float alignmentThreshold = 5f;
            float verticalAlignment = Mathf.Abs(toPowerup.x) / Mathf.Abs(toPowerup.y);

            if (Mathf.Abs(angle) < alignmentThreshold && verticalAlignment < 1f && _canFireAtPowerup <= Time.time)
                FireLaser(false);
        }
    }
    private void FireLaser(bool fireAtPlayer)
    {
        if(_type == EnemyType.Aggressive)
            _fireRate = Random.Range(1.5f, 3f);
        else
            _fireRate = Random.Range(3f, 7f);

        if(fireAtPlayer)
            _canFire = Time.time + _fireRate;
        else
            _canFireAtPowerup = Time.time + _fireRate;

        if (_type != EnemyType.Smart)
        {
            _laserOffset.Set(0f, -1.4f, 0f);
            _ = Instantiate(_laserPrefab, transform.position + _laserOffset, Quaternion.identity);
        }
        else
        {
            if(_direction > 0)
                _laserOffset.Set(1.35f, 0f, 0f);
            else
                _laserOffset.Set(-1.35f, 0f, 0f);
            GameObject laserObj = Instantiate(_horizontalLaserPrefab, transform.position + _laserOffset, Quaternion.identity);
            LaserBehaviour laser = laserObj.GetComponent<LaserBehaviour>();
            if (laser != null)
                    laser.SetDirection(_direction);
        }
    }

    private void CalculateMovement()
    {
        switch (_type)
        {
            case EnemyType.Default:
                if (isMovingSideways)
                    transform.Translate(_enemySpeed / 2f * _direction * Time.deltaTime * Vector3.right);

                transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
                CheckForBounce();
                break;
            case EnemyType.Zigzag:
                transform.Translate(_enemySpeed * _direction * Time.deltaTime * Vector3.right);
                transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
                CheckForBounce();
                break;
            case EnemyType.Aggressive:
                if (IsPlayerNearby())
                {
                    Vector3 playerPosition = _player.transform.position;
                    transform.position = Vector3.MoveTowards(transform.position, playerPosition, _enemySpeed * Time.deltaTime);
                }
                transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
                CheckForBounce();
                break;
            case EnemyType.Smart:
                transform.Translate(_enemySpeed * Time.deltaTime * Vector3.down);
                break;
            case EnemyType.Evasive:
                Vector2 _enemyMoveDirection = Vector2.down;
                bool _foundLaser = false;
                LaserBehaviour[] _trackLasers = SpawnManager.Instance.PlayerLaserContainer.GetComponentsInChildren<LaserBehaviour>();

                for (int i = 0; i < _trackLasers.Length; i++)
                {
                    if (_trackLasers[i] != null && _trackLasers[i].gameObject != null && _trackLasers[i].gameObject.activeSelf)
                    {
                        Vector2 _directionToLaser = _trackLasers[i].transform.position - transform.position;

                        if (Mathf.Abs(_directionToLaser.magnitude) < _avoidDistance)
                        {
                            _enemyMoveDirection = -_directionToLaser;
                            _foundLaser = true;
                        }
                    }
                }
                if(_foundLaser)
                    transform.Translate(_enemySpeed / 2f * Time.deltaTime * _enemyMoveDirection);
                else
                    transform.Translate(_enemySpeed * Time.deltaTime * _enemyMoveDirection);
                break;
            default:
                break;
        }

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
        switch (_type)
        {
            case EnemyType.Zigzag:
                if (_spriteTransform.rotation.y == 0f)
                {
                    _spriteTransform.rotation = Quaternion.Euler(0f, 180f, -45f);
                }
                else
                {
                    _spriteTransform.rotation = Quaternion.Euler(0f, 0f, -45f);
                }
                break;

            case EnemyType.Smart:
                if (_direction > 0)
                {
                    _direction = 1;
                    _spriteTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else
                {
                    _direction = -1;
                    _spriteTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                break;
        }
    }

    private void CheckForBounce()
    {
        if (transform.position.x >= _maxXPosition || transform.position.x <= _minXPosition)
        {
            if(_type == EnemyType.Zigzag)
            {
                _direction *= -1;
                RotateEnemy();
            }               
            else
            {
                _direction *= -1;
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
            DetermineIfPowerupInfront();
            if (_type == EnemyType.Smart)
                RotateEnemy();
        }
    }

    private bool IsPlayerNearby()
    {
        float detectionRadius = 3f;
        LayerMask playerLayer = LayerMask.GetMask("Player");

        if(Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer) != null)
        {
            return true;
        }

        return false;
    }

    public void IncreaseSpeed(float multiplier)
    {
        _enemySpeed *= multiplier;
        _enemySpeed = Mathf.Clamp(_enemySpeed, 4f, 8f);
    }

    private void SetShieldState(bool active)
    {
        _shieldPrefab.SetActive(active);
        _isShieldActive = active;
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
            {
                _player.AddScorePoints(Random.Range(5, 13));
                _player.TakeDamage();
            }
            TakeDamage(100);
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isShieldActive)
        {
            SetShieldState(false);
            return;
        }

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
