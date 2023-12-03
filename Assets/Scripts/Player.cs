using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 4f;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private int _playerLives = 3;
    private Vector3 _limitY, _teleportPositionX, _laserOffset;
    private float _minYPosition = -3.85f;
    private float _maxYPosition = 5.85f;
    private float _maxXPosition = 11.2f;
    private float _minXPosition = -11.2f;
    private float _teleportXPosition = 11f;
    private float _canFire = -1f;
    private float _laserOffsetY = 1.05f;
    [SerializeField] private float _fireDelay = 0.25f;

    private void Start()
    {
        transform.position = Vector3.zero;
    }

    private void Update()
    {
        //Get player input
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(moveX, moveY, 0);               

        //Restrict Y position
        RestrictYPosition();

        //Teleport player on the X-axis when leaving the screen
        TeleportOnXAxis();

        //Move the player
        MovePlayer(direction);

        //Fire the laser when pressing Space
        if (Input.GetKeyDown(KeyCode.Space) && _canFire <= Time.time)
        {
            FireLaser();
        }
    }

    private void FireLaser()
    {
        _canFire = Time.time + _fireDelay;
        _laserOffset.Set(transform.position.x, transform.position.y + _laserOffsetY, 0);
        Instantiate(_laserPrefab, _laserOffset, Quaternion.identity);
    }

    private void MovePlayer(Vector3 direction)
    {
        transform.Translate(_playerSpeed * Time.deltaTime * direction);
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

    public void TakeDamage()
    {
        _playerLives--;
        if (_playerLives <= 0)
        {
            SpawnManager.Instance.StopSpawning();
            Destroy(this.gameObject);
        }
    }
}
