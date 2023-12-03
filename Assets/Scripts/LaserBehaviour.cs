using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
    [SerializeField] private float _laserSpeed = 8f;
    private void Start()
    {
        Destroy(this.gameObject, 2f);
    }

    private void Update()
    {
        transform.Translate(Vector3.up * _laserSpeed * Time.deltaTime);
    }
}
