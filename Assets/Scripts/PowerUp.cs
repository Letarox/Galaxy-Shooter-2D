using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{   
    [SerializeField] private float _powerUpSpeed = 3f;    
    [SerializeField] private float _duration = 5f;
    [SerializeField] private int _powerUpID; //0 TripleShot, 1 Speed, 2 Shield
    private float _minYPosition = -6f;
    void Start()
    {
        
    }

    void Update()
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
                switch (_powerUpID)
                {
                    case 0:
                        player.ActivateTripleShotPowerUp(_duration);
                        break;
                    case 1:
                        player.ActivateSpeedPowerUp(_duration);
                        break;
                    case 2:
                        Debug.Log("Shield Collected.");
                        break;
                    default:
                        Debug.Log("Default value.");
                        break;
                }
            }
            Destroy(gameObject);
        }
    }
}
