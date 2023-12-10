using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 25f;
    [SerializeField] private GameObject _explosionPrefab;
    private void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime * Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerLaser"))
        {
            SpawnManager.Instance.StartSpawning();
            _ = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            Destroy(gameObject, 0.25f);
        }
    }
}
