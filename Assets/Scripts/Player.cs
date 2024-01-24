using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 5f;
    [SerializeField] private float _powerUpSpeedMultiplier = 1f;
    [SerializeField] private float _thrusterSpeedMultipler = 1.5f;
    [SerializeField] private GameObject _laserPrefab, _tripleShotPrefab, _homingMissilePrefab;
    [SerializeField] private GameObject _shield, _thruster;
    [SerializeField] private GameObject[] _wings;
    [SerializeField] private int _playerLives = 3;
    [SerializeField] private float _fireDelay = 0.25f;
    [SerializeField] private AudioClip _laserSoundClip;
    [SerializeField] private Color[] _shieldColors;
    [SerializeField] private CameraShake _cameraShake;
    private readonly float _minYPosition = -3.85f;
    private readonly float _maxYPosition = 5.85f;
    private readonly float _maxXPosition = 11.2f;
    private readonly float _minXPosition = -11.2f;
    private readonly float _teleportXPosition = 11f;
    private readonly float _laserOffsetY = 1.05f;
    private readonly WaitForSeconds _invulnerabilityDelay = new(1f);
    private Vector3 _direction, _limitY, _teleportPositionX, _laserOffset;
    private float _canFire = -1f;
    private Coroutine _tripleShotRoutine, _speedRoutine, _slowRoutine, _homingMissileRoutine;
    private int _score;
    private int _shieldStrength;
    private int _ammoCount = 15;
    private AudioSource _audioSource;
    private bool _isTripleShotOn = false;
    private bool _isInvulnerable = false;
    private bool _isThrusterActive = false;
    private bool _isPlayerSlowed = false;
    private bool _isHomingActive = false;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.LogError("Audio Source is NULL on the Player");
        else
            _audioSource.clip = _laserSoundClip;
        transform.position = Vector3.zero;
    }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        _direction.Set(moveX, moveY, 0);

        MovePlayer(_direction);

        if (Input.GetKeyDown(KeyCode.Space) && _canFire <= Time.time)
        {
            if(_ammoCount > 0)
            {
                _ammoCount--;
                UIManager.Instance.UpdateAmmoAmount(_ammoCount);
                if (_isHomingActive)
                    FireHomingMissile();
                else
                    FireLaser();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CollectAllPowerups();
        }

        if (Input.GetKey(KeyCode.LeftShift) && !_isPlayerSlowed && _powerUpSpeedMultiplier == 1f && UIManager.Instance.CanUseThrusterBoost())
        {
            _isThrusterActive = true;
            _thruster.SetActive(true);
            _ = StartCoroutine(UIManager.Instance.ThrusterBoostSliderDownRoutine());
        }
        else
        {
            _isThrusterActive = false;
            if (_powerUpSpeedMultiplier == 1f)
                _thruster.SetActive(false);
            _ = StartCoroutine(UIManager.Instance.ThrusterBoostSliderUpRoutine());
        }
    }

    private void CollectAllPowerups()
    {
        PowerUp[] powerups = SpawnManager.Instance.transform.GetChild(1).GetComponentsInChildren<PowerUp>();
        foreach (PowerUp powerup in powerups)
        {
            powerup.ActivateBeingCollected();
        }
    }

    private void FireLaser()
    {
        _canFire = Time.time + _fireDelay;
        if (_isTripleShotOn)
        {
            _laserOffset.Set(transform.position.x, transform.position.y, 0);
            _ = Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity, SpawnManager.Instance.PlayerLaserContainer.transform);
        }
        else
        {
            _laserOffset.Set(transform.position.x, transform.position.y + _laserOffsetY, 0);
            _ = Instantiate(_laserPrefab, _laserOffset, Quaternion.identity, SpawnManager.Instance.PlayerLaserContainer.transform);
        }

        _audioSource.Play();
    }

    private void FireHomingMissile()
    {
        _canFire = Time.time + _fireDelay;
        _laserOffset.Set(transform.position.x, transform.position.y + _laserOffsetY, 0);
        _ = Instantiate(_homingMissilePrefab, transform.position, Quaternion.identity, SpawnManager.Instance.PlayerLaserContainer.transform);
    }

    private void MovePlayer(Vector3 direction)
    {
        RestrictYPosition();
        TeleportOnXAxis();

        if (_isPlayerSlowed)
        {
            transform.Translate(_playerSpeed / 2f * Time.deltaTime * direction);
            return;
        }            

        if(_isThrusterActive && _powerUpSpeedMultiplier == 1f)
            transform.Translate(_playerSpeed * _thrusterSpeedMultipler * Time.deltaTime * direction);
        else
            transform.Translate(_playerSpeed * _powerUpSpeedMultiplier * Time.deltaTime * direction);
    }

    private void RestrictYPosition()
    {
        float clampedY = Mathf.Clamp(transform.position.y, _minYPosition, _maxYPosition);
        _limitY.Set(transform.position.x, clampedY, 0f);
        transform.position = _limitY;
    }

    private void TeleportOnXAxis()
    {
        if (IsOutsideXBounds())
        {
            int direction = (transform.position.x > _maxXPosition) ? -1 : 1;
            TeleportPlayer(direction);
        }
    }

    private bool IsOutsideXBounds()
    {
        return transform.position.x > _maxXPosition || transform.position.x < _minXPosition;
    }

    private void TeleportPlayer(int direction)
    {
        float newX = direction * _teleportXPosition;
        _teleportPositionX.Set(newX, transform.position.y, 0f);
        transform.position = _teleportPositionX;
    }

    private IEnumerator EnableTripleShotRoutine(float duration)
    {
        _isTripleShotOn = true;
        yield return new WaitForSeconds(duration);
        _isTripleShotOn = false;
        _tripleShotRoutine = null;
    }

    private IEnumerator EnableSpeedRoutine(float duration)
    {
        _powerUpSpeedMultiplier = 2.5f;
        if(!_isPlayerSlowed)
            _thruster.SetActive(true);
        yield return new WaitForSeconds(duration);
        _powerUpSpeedMultiplier = 1f;
        _thruster.SetActive(false);
        _speedRoutine = null;
    }

    private IEnumerator EnableSlowRoutine(float duration)
    {
        _isPlayerSlowed = true;
        if(_powerUpSpeedMultiplier > 1f)
            _thruster.SetActive(false);
        yield return new WaitForSeconds(duration);
        if (_powerUpSpeedMultiplier > 1f)
            _thruster.SetActive(true);
        _isPlayerSlowed = false;
    }

    private IEnumerator EnableHomingMissileRoutine(float duration)
    {
        _isHomingActive = true;
        yield return new WaitForSeconds(duration);
        _isHomingActive = false;
        _homingMissileRoutine = null;
    }

    private void SetShieldState(int strength)
    {
        if(strength == 0)
        {
            _shield.SetActive(false);
            return;
        }

        strength = Mathf.Clamp(strength, 0, 3);
        _spriteRenderer.color = _shieldColors[strength - 1];
    }

    public void ActivateTripleShotPowerUp(float duration)
    {
        if (_tripleShotRoutine != null)
            StopCoroutine(_tripleShotRoutine);

        _tripleShotRoutine = StartCoroutine(EnableTripleShotRoutine(duration));
    }

    public void ActivateSpeedPowerUp(float duration)
    {
        if (_speedRoutine != null)
            StopCoroutine(_speedRoutine);

        _speedRoutine = StartCoroutine(EnableSpeedRoutine(duration));
    }

    public void ActivateShieldPowerUp()
    {
        _shieldStrength = 3;
        _shield.SetActive(true);
        if (_spriteRenderer == null)
        {
            _spriteRenderer = _shield.GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
                Debug.LogError("Sprite Renderer is NULL on the Player");
        }
        SetShieldState(_shieldStrength);
    }

    public void GrantBonusAmmo()
    {
        _ammoCount += Random.Range(10,16);
        _ammoCount = Mathf.Clamp(_ammoCount, 0, 100);
        UIManager.Instance.UpdateAmmoAmount(_ammoCount);
    }

    public void GrantBonusLife()
    {
        _playerLives++;
        _playerLives = Mathf.Clamp(_playerLives, 0, 3);
        UIManager.Instance.UpdateLivesDisplay(_playerLives);

        if (_playerLives == 3)
            SetAllWings(false);
        else
        {
            SetRandomWingEffect(false);
        }
    }

    public void ActivateSpecialPowerUp()
    {
        var enemies = SpawnManager.Instance.GetAllEnemyBehaviors();
        foreach(var enemy in enemies)
        {
            enemy.TakeDamage(100);
        }
    }

    public void ActivateSlowPowerUp(float duration)
    {
        if (_slowRoutine != null)
            StopCoroutine(_slowRoutine);

        _slowRoutine = StartCoroutine(EnableSlowRoutine(duration));
    }

    public void ActivateHomingMissilePowerUp(float duration)
    {
        if (_homingMissileRoutine != null)
            StopCoroutine(_homingMissileRoutine);

        _homingMissileRoutine = StartCoroutine(EnableHomingMissileRoutine(duration));
    }

    public void AddScorePoints(int value)
    {
        _score += value;
        UIManager.Instance.UpdateScoreText(_score);
    }

    private void ActivateWingsEffect(int lives)
    {
        if (lives == 2)
            SetRandomWingEffect(true);
        else
        {
            SetAllWings(true);
        }
    }

    private void SetAllWings(bool active)
    {
        foreach (var wings in _wings)
        {
            wings.SetActive(active);
        }
    }

    private void SetRandomWingEffect(bool active)
    {
        int randomNumber = Random.Range(0, _wings.Length);
        _wings[randomNumber].SetActive(active);
    }

    public void TakeDamage()
    {
        if (_isInvulnerable)
            return;

        if (_shieldStrength > 0)
        {
            _shieldStrength--;
            _ = StartCoroutine(InvulnerabilityRoutine());
            SetShieldState(_shieldStrength);
            return;
        }

        _playerLives--;
        _ = StartCoroutine(InvulnerabilityRoutine());
        _ = StartCoroutine(_cameraShake.CameraShakeRoutine());
        ActivateWingsEffect(_playerLives);
        UIManager.Instance.UpdateLivesDisplay(_playerLives);

        if (_playerLives <= 0)
        {
            SpawnManager.Instance.StopSpawning();
            Destroy(gameObject);
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        _isInvulnerable = true;
        yield return _invulnerabilityDelay;
        _isInvulnerable = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyLaser"))
        {
            TakeDamage();
            Destroy(other.gameObject);
        }
    }
}
