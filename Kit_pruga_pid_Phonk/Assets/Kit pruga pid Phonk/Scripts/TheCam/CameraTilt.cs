using UnityEngine;

public class CameraTilt : MonoBehaviour
{
    public float tiltAmount = 15f; // Кут нахилу камери
    public float smoothSpeed = 5f; // Плавність нахилу
    public float maxTiltAngle = 30f; // Максимальний кут нахилу

    private Transform cameraTransform;
    private Vector3 initialRotation;
    private float horizontalInput;

    private void Start()
    {
        
        cameraTransform = Camera.main.transform; // Отримуємо трансформ камери
        initialRotation = cameraTransform.localRotation.eulerAngles; // Зберігаємо початкові угли повороту камери
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
    }

    private void LateUpdate()
    {
        // Обчислюємо бажаний кут нахилу на основі вхідних даних гравця зі зміненим напрямком
        float targetTiltAngle = -horizontalInput * tiltAmount;

        // Омежуємо максимальний кут нахилу
        targetTiltAngle = Mathf.Clamp(targetTiltAngle, -maxTiltAngle, maxTiltAngle);

        // Плавно змінюємо кут нахилу камери
        Quaternion targetRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, targetTiltAngle);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}
1