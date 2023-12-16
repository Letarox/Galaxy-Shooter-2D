using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float _duration = 1f;
    [SerializeField] private AnimationCurve _animationCurve;
    public IEnumerator CameraShakeRoutine()
    {
        Vector3 startPosition = transform.position;

        float elapsedTime = 0f;

        while(elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;

            float strength = _animationCurve.Evaluate(elapsedTime / _duration);

            transform.position = startPosition + Random.insideUnitSphere * strength;

            yield return null;
        }

        transform.position = startPosition;
    }
}
