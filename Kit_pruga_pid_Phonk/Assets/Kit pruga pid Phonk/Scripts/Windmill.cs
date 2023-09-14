using UnityEngine;

public class Windmill : MonoBehaviour
{
    public float rotationSpeed = 100f; // Швидкість обертання
    public Transform stationaryElement; // Посилання на нерухомий елемент

    private Transform windmillTransform;

    private void Start() {
        windmillTransform = transform;
    }

    private void Update() {
        // Обертання навколо осі Y
        windmillTransform.Rotate(-Vector3.right, rotationSpeed * Time.deltaTime);
    }
}
