using UnityEngine;

public class CombinedCameraBehavior : MonoBehaviour
{
    public float targetTime = 0.2f; // Время на один шаг в секундах
    public float Smooth = 10; // Мягкость
    public float AmplitudeHeight = 0.1f; // Амлитуда покачивания вверх-вниз
    public float AmplitudeRot = 1.5f; // Амплитуда поворота
    public float tiltAmount = 15f; // Кут нахилу камери
    public float maxTiltAngle = 30f; // Максимальний кут нахилу

    private float Progress; // Прогресс
    private int PassedStep = 1; // Шаг
    private float DefCamPos = 0; // Изначальная позиция камеры
    private float DefCamRotX = 0; // Изначальний поворот камери по X
    private float DefCamRotY = 0; // Изначальний поворот камери по Y
    private Transform MyTransform; // трансформ
    private Transform cameraTransform;
    private Vector3 initialRotation;
    private float horizontalInput;

    private void Start()
    {
        MyTransform = transform; // Трансформ вашого об'єкта
        
        DefCamPos = MyTransform.localPosition.y; // Изначальна позиція камери
        DefCamRotX = MyTransform.localEulerAngles.x; // Изначальний поворот камери по X
        DefCamRotY = MyTransform.localEulerAngles.y; // Изначальний поворот камери по Y

        cameraTransform = Camera.main.transform; // Отримуємо трансформ камери
        initialRotation = cameraTransform.localRotation.eulerAngles; // Зберігаємо початкові угли повороту камери
    }

    private void Update()
    {
        float Pssd = Passed(); // Наш прогресс
        horizontalInput = Input.GetAxis("Horizontal");

        // Позиція в Vector3, к якій ми стримуємося
        Vector3 CamPos = new Vector3(MyTransform.localPosition.x, Pssd * AmplitudeHeight + DefCamPos, MyTransform.localPosition.z);
        // Інтерполяція позиції (сглажування)
        MyTransform.localPosition = Vector3.Lerp(MyTransform.localPosition, CamPos, Time.deltaTime * Smooth);

        // Поворот камери
        if (Mathf.Abs(horizontalInput) == 1 && Mathf.Abs(Input.GetAxis("Vertical")) == 0)
        {
            Pssd = 0; // Тільки якщо ми не йдемо в бок
        }
        float targetTiltX = Pssd * AmplitudeRot + DefCamRotX;
        float targetTiltY = Pssd * AmplitudeRot + DefCamRotY;
        
        // Омежуємо максимальний кут нахилу
        targetTiltX = Mathf.Clamp(targetTiltX, -maxTiltAngle, maxTiltAngle);
        targetTiltY = Mathf.Clamp(targetTiltY, -maxTiltAngle, maxTiltAngle);

        // Плавно змінюємо кут нахилу камери
        Quaternion targetRotation = Quaternion.Euler(initialRotation.x + targetTiltX, initialRotation.y + targetTiltY, initialRotation.z);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRotation, Smooth * Time.deltaTime);
    }

    private float Passed()
    {
        // Якщо ми взагалі нікуди не рухаємося (право, ліво, вперед, назад)
        // То повертаємо нуль
        if (Mathf.Abs(Input.GetAxis("Horizontal")) == 0 && Mathf.Abs(Input.GetAxis("Vertical")) == 0)
        {
            PassedStep = 1; // Скидаємо крок
            return (Progress = 0); // Прогрес сводимо до нуля і повертаємо його
        }

        // Перемножуємо прогрес на крок (PassedStep)
        // Якщо step = 1, то тоді значення не змінюється.
        // А якщо step = -1, то тоді значення формули стає від'ємним і ми починаємо віднімати з Progress
        Progress += (Time.deltaTime * (1f / targetTime)) * PassedStep;
        if (Mathf.Abs(Progress) >= 1)
        {
            PassedStep *= -1; // Інвертуємо крок
        }

        // Повертаємо прогрес, він у нас ширяється від -1 до 1
        return Progress;
    }
}
