using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
    private enum ProjectileType
    {
        VerticalLaser,
        HorizontalLaser,
        Homing
    }

    [SerializeField] private float _laserSpeed = 8f;
    [SerializeField] private bool _isPlayerLaser = true;
    [SerializeField] private ProjectileType _type;
    private readonly float _maxPosY = 8f;
    private readonly float _minPosY = -6f;    
    private readonly float _maxPosX = 12f;
    private readonly float _minPosX = -12f;
    private int _direction = 1;
    private GameObject _closetEnemy;

    private void Update()
    {
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        if (_isPlayerLaser)
        {
            switch (_type)
            {
                case ProjectileType.VerticalLaser:
                    transform.Translate(_laserSpeed * Time.deltaTime * Vector3.up);
                    break;
                case ProjectileType.Homing:
                    List<GameObject> enemies = SpawnManager.Instance.ActiveEnemies;
                    float distanceTreshold = 5f;
                    float closestDistance = 0f;

                    foreach (GameObject enemy in enemies)
                    {
                        Vector3 difference = transform.position - enemy.transform.position;
                        float distance = difference.magnitude;

                        if (distance < closestDistance || closestDistance == 0)
                        {
                            closestDistance = distance;
                            _closetEnemy = enemy;
                        }
                    }

                    if (_closetEnemy != null && _closetEnemy.activeSelf && closestDistance < distanceTreshold)
                    {
                        Vector3 enemyPosition = _closetEnemy.transform.position;
                        transform.position = Vector3.MoveTowards(transform.position, enemyPosition, _laserSpeed * Time.deltaTime);
                    }
                    else
                    {
                        transform.Translate(_laserSpeed * Time.deltaTime * Vector3.up);
                    }
                    break;
            }
        }
        else
        {
            switch (_type)
            {
                case ProjectileType.VerticalLaser:
                    transform.Translate(_laserSpeed * Time.deltaTime * Vector3.down);
                    break;
                case ProjectileType.HorizontalLaser:
                    Vector3 horizontalDirection = Vector3.right * _direction;
                    transform.Translate(_laserSpeed * Time.deltaTime * horizontalDirection);
                    break;
            }
        }
        DestroyLaserOutOfBounds();
    }

    public void DestroyLaserOutOfBounds()
    {
        if ((_isPlayerLaser && transform.position.y > _maxPosY) || (!_isPlayerLaser && transform.position.y < _minPosY))
        {
            Destroy(gameObject);
        }

        else if (_type == ProjectileType.HorizontalLaser && (transform.position.x > _maxPosX || transform.position.x < _minPosX))
        {
            Destroy(gameObject);
        }
    }

    public void SetDirection(int direction)
    {
        _direction = direction;
    }
}
