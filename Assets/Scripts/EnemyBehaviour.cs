using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private float _enemySpeed = 4f;
    [SerializeField] private int _health = 3;
    private Vector3 _newPosition = new();
    private float _maxYPosition = 7.35f;
    private float _minYPosition = -5.5f;
    private float _maxXPosition = 9.2f;
    private float _minXPosition = -9.2f;

    private void Start()
    {
        
    }

    private void Update()
    {
        CalculateMovement();
    }

    private void CalculateMovement()
    {
        transform.Translate(Vector3.down * _enemySpeed * Time.deltaTime);

        if (transform.position.y < _minYPosition)
        {
            float randomXPosition = Random.Range(_minXPosition, _maxXPosition);
            _newPosition.Set(randomXPosition, _maxYPosition, 0);
            transform.position = _newPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLaser"))
        {
            Destroy(other.gameObject);
            TakeDamage();            
        }
        else if (other.CompareTag("Player"))
        {            
            Player p = other.GetComponent<Player>();
            if (p != null)
                p.TakeDamage();
            Destroy(this.gameObject);
        }
    }

    private void TakeDamage()
    {
        _health--;
        if (_health <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
