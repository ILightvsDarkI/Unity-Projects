using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShookShake : MonoBehaviour
{
    public float power = 0.7f;
    public float duration = 1.0f;
    public Transform camera;
    public float slowDownAmount;
    public bool shouldShake = false;

     Vector3 StartPosition;
     float initiaDuration;

    void Start () {
        camera = Camera.main.transform;
        StartPosition = camera.localPosition;
        initiaDuration = duration;
    }

    void Update () {
        if (shouldShake)
        {
            if (duration > 0)
            {
                camera.localPosition = StartPosition + Random.insideUnitSphere * power;
                duration -= Time.deltaTime * slowDownAmount;
            }
            else
            {
                shouldShake = false;
                duration = initiaDuration;
                camera.localPosition =  StartPosition;
            }

        }
    }
}
