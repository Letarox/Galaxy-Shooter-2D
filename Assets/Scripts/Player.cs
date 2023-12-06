using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 4f;
    [SerializeField] private float _powerUpSpeedMultiplier = 1f;
    [SerializeField] private GameObject _laserPrefab, _tripleShotPrefab;
    [SerializeField] private GameObject _shield;
    [SerializeField] private int _playerLives = 3;
    private Vector3 _direction, _limitY, _teleportPositionX, _laserOffset;
    private float _minYPosition = -3.85f;
    private float _maxYPosition = 5.85f;
    private float _maxXPosition = 11.2f;
    private float _minXPosition = -11.2f;
    private float _teleportXPosition = 11f;
    private float _canFire = -1f;
    private float _laserOffsetY = 1.05f;
    [SerializeField] private float _fireDelay = 0.25f;
    [SerializeField] private bool _isTripleShotOn = false;
    [SerializeField] private bool _isShieldOn = false;
    private Coroutine _tripleShotRoutine, _speedRoutine;
    private int _score;

    private void Start()
    {
        transform.position = Vector3.zero;
    }

    private void Update()
    {
        //Get player input
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        _direction.Set(moveX, moveY, 0);               

        //Call the movement method
        MovePlayer(_direction);

        //Fire the laser when pressing Space
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
            Instantiate(_tripleShotPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            _laserOffset.Set(transform.position.x, transform.position.y + _laserOffsetY, 0);
            Instantiate(_laserPrefab, _laserOffset, Quaternion.identity);
        }        
    }

    private void MovePlayer(Vector3 direction)
    {
        //Restrict Y position
        RestrictYPosition();

        //Teleport player on the X-axis when leaving the screen
        TeleportOnXAxis();

        //Move the player based on its receiving Input
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
        //Returns true if the player is outside of bounds on the X axis
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
        yield return new WaitForSeconds(duration);
        _powerUpSpeedMultiplier = 1f;
        _speedRoutine = null;
    }

    private void ActivateDeactivateShield(bool activate)
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
        ActivateDeactivateShield(true);
    }

    public void AddScorePoints(int value)
    {
        _score += value;
        UIManager.Instance.UpdateScoreText(_score);
    }

    public void TakeDamage()
    {
        if (_isShieldOn)
        {
            ActivateDeactivateShield(false);
            return;
        }

        _playerLives--;
        UIManager.Instance.UpdateLivesDisplay(_playerLives);

        if (_playerLives <= 0)
        {
            SpawnManager.Instance.StopSpawning();
            Destroy(gameObject);
        }
    }
}
