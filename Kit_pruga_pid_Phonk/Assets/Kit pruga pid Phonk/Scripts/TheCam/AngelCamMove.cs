using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelCamMove : MonoBehaviour
{
    public float maxTiltAngle = 30.0f; // Максимальний кут нахилу камери.
    public float tiltSpeed = 5.0f; // Швидкість нахилу камери.

    private float currentTiltAngle = 0.0f;

    private void Update()
    {
        // Отримуємо вхід від гравця (наприклад, клавіші A та D або стрілки вліво та вправо).
        float horizontalInput = Input.GetAxis("Horizontal");

        // Обчислюємо кут нахилу залежно від вхідних даних.
        float targetTiltAngle = horizontalInput * maxTiltAngle;

        // За допомогою Lerp плавно змінюємо поточний кут нахилу до цільового кута нахилу.
        currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTiltAngle, tiltSpeed * Time.deltaTime);

        // Застосовуємо нахил до камери, обертаючи її.
        transform.rotation = Quaternion.Euler(0, 0, -currentTiltAngle);
    }
}
