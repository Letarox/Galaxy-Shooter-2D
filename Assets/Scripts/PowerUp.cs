using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{   
    private enum PowerUpType
    {
        TripleShot,
        Speed,
        Shield
    }

    [SerializeField] private float _powerUpSpeed = 3f;
    [SerializeField] private float _duration = 5f;
    [SerializeField] private PowerUpType _type;
    [SerializeField] private AudioClip _powerUpClip;
    private readonly float _minYPosition = -6f;

    private void Update()
    {
        MovePowerUp();
    }

    private void MovePowerUp()
    {
        transform.Translate(_powerUpSpeed * Time.deltaTime * Vector3.down);
        if (transform.position.y < _minYPosition)
            Destroy(gameObject);
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
            default:
                break;
        }
    }
}
