using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{   
    private enum PowerUpType
    {
        TripleShot,
        Speed,
        Shield,
        Ammo,
        Life,
        Special,
        Slow,
        Homing
    }

    [SerializeField] private float _powerUpSpeed = 3f;
    [SerializeField] private float _duration = 5f;
    [SerializeField] private PowerUpType _type;
    [SerializeField] private AudioClip _powerUpClip;
    private readonly float _minYPosition = -6f;
    private bool _isBeingCollect = false;
    private GameObject _player;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
            Debug.LogError("Player is NULL on " + gameObject.name);
    }

    private void Update()
    {
        MovePowerUp();
    }

    private void MovePowerUp()
    {
        if (!_isBeingCollect || !_player.activeInHierarchy)
            transform.Translate(_powerUpSpeed * Time.deltaTime * Vector3.down);
        else
        {
            Vector3 playerPos = _player.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, playerPos, _powerUpSpeed * Time.deltaTime);
        }

        if (transform.position.y < _minYPosition)
            Destroy(gameObject);
    }

    public void ActivateBeingCollected()
    {
        _isBeingCollect = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                ActivatePowerUp(player);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("EnemyLaser"))
        {
            Destroy(gameObject);
        }
    }

    private void ActivatePowerUp(Player player)
    {
        AudioSource.PlayClipAtPoint(_powerUpClip, transform.position);
        switch (_type)
        {
            case PowerUpType.TripleShot:
                player.ActivateTripleShotPowerUp(_duration);
                break;
            case PowerUpType.Speed:
                player.ActivateSpeedPowerUp(_duration);
                break;
            case PowerUpType.Shield:
                player.ActivateShieldPowerUp();
                break;
            case PowerUpType.Ammo:
                player.GrantBonusAmmo();
                break;
            case PowerUpType.Life:
                player.GrantBonusLife();
                break;
            case PowerUpType.Special:
                player.ActivateSpecialPowerUp();
                break;
            case PowerUpType.Slow:
                player.ActivateSlowPowerUp(_duration);
                break;
            case PowerUpType.Homing:
                player.ActivateHomingMissilePowerUp(_duration);
                break;
            default:
                break;
        }
    }
}
