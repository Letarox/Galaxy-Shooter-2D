using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
    [SerializeField] private float _laserSpeed = 8f;
    private void Start()
    {
        if(transform.parent != null)
        {
            Destroy(transform.parent.gameObject, 2f);
            Destroy(gameObject, 2f);
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    private void Update()
    {
        transform.Translate(_laserSpeed * Time.deltaTime * Vector3.up);
    }
}
