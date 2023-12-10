using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 5f;
    [SerializeField] private float _powerUpSpeedMultiplier = 1f;
    [SerializeField] private GameObject _laserPrefab, _tripleShotPrefab;
    [SerializeField] private GameObject _shield, _thruster;
    [SerializeField] private GameObject[] _wings;
    [SerializeField] private int _playerLives = 3;
    [SerializeField] private float _fireDelay = 0.25f;
    [SerializeField] private bool _isTripleShotOn = false;
    [SerializeField] private bool _isShieldOn = false;
    [SerializeField] private AudioClip _laserSoundClip;
    private readonly float _minYPosition = -3.85f;
    private readonly float _maxYPosition = 5.85f;
    private readonly float _maxXPosition = 11.2f;
    private readonly float _minXPosition = -11.2f;
    private readonly float _teleportXPosition = 11f;
    private readonly float _laserOffsetY = 1.05f;
    private Vector3 _direction, _limitY, _teleportPositionX, _laserOffset;
    private float _canFire = -1f;
    private Coroutine _tripleShotRoutine, _speedRoutine;
    private int _score;
    private AudioSource _audioSource;

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
            FireLaser();
        }
    }

    private void FireLaser()
    {
        _canFire = Time.time + _fireDelay;
        if (_isTripleShotOn)
        {
            _ = Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            _laserOffset.Set(transform.position.x, transform.position.y + _laserOffsetY, 0);
            _ = Instantiate(_laserPrefab, _laserOffset, Quaternion.identity);
        }

        _audioSource.Play();
    }

    private void MovePlayer(Vector3 direction)
    {
        RestrictYPosition();

        TeleportOnXAxis();

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
        _thruster.SetActive(true);
        yield return new WaitForSeconds(duration);
        _powerUpSpeedMultiplier = 1f;
        _thruster.SetActive(false);
        _speedRoutine = null;
    }

    private void SetShieldState(bool activate)
    {
        _isShieldOn = activate;
        _shield.SetActive(activate);
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
        SetShieldState(true);
    }

    public void AddScorePoints(int value)
    {
        _score += value;
        UIManager.Instance.UpdateScoreText(_score);
    }

    private void ActivateWingsEffect(int lives)
    {
        if (lives == 2)
            ActivateRandomWingEffect();
        else
        {
            ActivateAllWings();
        }
            
    }

    private void ActivateAllWings()
    {
        foreach (var wings in _wings)
        {
            wings.SetActive(true);
        }
    }

    private void ActivateRandomWingEffect()
    {
        int randomNumber = Random.Range(0, _wings.Length);
        _wings[randomNumber].SetActive(true);
    }

    public void TakeDamage()
    {
        if (_isShieldOn)
        {
            SetShieldState(false);
            return;
        }

        _playerLives--;
        ActivateWingsEffect(_playerLives);
        UIManager.Instance.UpdateLivesDisplay(_playerLives);

        if (_playerLives <= 0)
        {
            SpawnManager.Instance.StopSpawning();
            Destroy(gameObject);
        }
    }
}
