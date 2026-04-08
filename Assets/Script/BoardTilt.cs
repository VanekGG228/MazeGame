using UnityEngine;

public class BoardTiltIMU : MonoBehaviour
{
    public SerialReader serial;
    public float maxTilt = 30f;   // максимальный наклон
    public float smooth = 2f;     // сглаживание
    public float alpha = 0.7f;    // коэффициент комплементарного фильтра

    private float angleX = 0f;
    private float angleZ = 0f;

    private float offsetX = 0f;   // смещение после калибровки
    private float offsetZ = 0f;

    private float lastTime;

    void Start()
    {
        lastTime = Time.time;

        if (serial == null)
            serial = FindFirstObjectByType<SerialReader>();

        // Загружаем сохраненные смещения
        if (PlayerPrefs.HasKey("IMU_OffsetX")) offsetX = PlayerPrefs.GetFloat("IMU_OffsetX");
        if (PlayerPrefs.HasKey("IMU_OffsetZ")) offsetZ = PlayerPrefs.GetFloat("IMU_OffsetZ");

        Debug.Log($"Loaded offsets: X={offsetX}, Z={offsetZ}");
    }

    // Калибровка начальной позиции
    public void Calibrate()
    {
        if (serial == null)
            return;

        float[] imu = serial.GetIMU();
        if (imu.Length < 12)
            return;

        float ax = (imu[0] + imu[6]) / 2f;
        float ay = (imu[1] + imu[7]) / 2f;
        float az = (imu[2] + imu[8]) / 2f;

        // Считаем углы акселерометра в момент калибровки
        offsetX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
        offsetZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        // Сохраняем в PlayerPrefs
        PlayerPrefs.SetFloat("IMU_OffsetX", offsetX);
        PlayerPrefs.SetFloat("IMU_OffsetZ", offsetZ);
        PlayerPrefs.Save();

        Debug.Log($"Calibrated offsets: X={offsetX}, Z={offsetZ}");
    }

    void Update()
    {
        if (serial == null)
            return;

        float[] imu = serial.GetIMU();
        if (imu.Length < 12)
            return;

        float ax = (imu[0] + imu[6]) / 2f;
        float ay = (imu[1] + imu[7]) / 2f;
        float az = (imu[2] + imu[8]) / 2f;

        float gx = (imu[3] + imu[9]) / 2f;
        float gy = (imu[4] + imu[10]) / 2f;

        float dt = Time.time - lastTime;
        lastTime = Time.time;

        // Углы от акселерометра
        float accAngleX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
        float accAngleZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        // Интеграция гироскопа
        angleX += gx * dt;
        angleZ += gy * dt;

        // Комплементарный фильтр
        angleX = alpha * angleX + (1f - alpha) * accAngleX;
        angleZ = alpha * angleZ + (1f - alpha) * accAngleZ;

        // Применяем смещение — делаем углы относительными к стартовой позиции
        float tiltX = angleX - offsetX;
        float tiltZ = angleZ - offsetZ;

        // Ограничение максимального наклона
        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltZ = Mathf.Clamp(tiltZ, -maxTilt, maxTilt);

        // Поворот доски
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(-tiltX, 0f, -tiltZ),
            smooth * Time.deltaTime
        );
    }
}