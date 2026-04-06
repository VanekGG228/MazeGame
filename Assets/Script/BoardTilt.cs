using UnityEngine;

public class BoardTiltIMU : MonoBehaviour
{
    public SerialReader serial;
    public float maxTilt = 30f;   
    public float smooth = 2f;     
    public float alpha = 0.70f;   

    private float angleX = 0f;
    private float angleZ = 0f;

    private float lastTime;

    void Start()
    {
        lastTime = Time.time;

        if (serial == null)
            serial = FindFirstObjectByType<SerialReader>();
    }

    void Update()
    {
        if (serial == null)
        {
            Debug.LogWarning("[BoardTiltIMU] SerialReader not assigned!");
            return;
        }

        float[] imu = serial.GetIMU();
        Debug.Log(imu);

        if (imu.Length < 12)
        {
            Debug.LogWarning("[BoardTiltIMU] IMU data incomplete!");
            return;
        }

        float ax = (imu[0] + imu[6]) / 2f;
        float ay = (imu[1] + imu[7]) / 2f;
        float az = (imu[2] + imu[8]) / 2f;

        float gx = (imu[3] + imu[9]) / 2f;
        float gy = (imu[4] + imu[10]) / 2f;
        float gz = (imu[5] + imu[11]) / 2f;

        // --- DEBUG: проверка усредненных данных ---
        // Debug.Log($"[IMU AVG] Ax={ax:F2}, Ay={ay:F2}, Az={az:F2}, Gx={gx:F2}, Gy={gy:F2}, Gz={gz:F2}");

        float dt = Time.time - lastTime;
        lastTime = Time.time;

        float accAngleX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
        float accAngleZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        angleX += gx * dt;
        angleZ += gy * dt;

        angleX = alpha * angleX + (1f - alpha) * accAngleX;
        angleZ = alpha * angleZ + (1f - alpha) * accAngleZ;

        float tiltX = Mathf.Clamp(angleX, -maxTilt, maxTilt);
        float tiltZ = Mathf.Clamp(angleZ, -maxTilt, maxTilt);

        Quaternion target = Quaternion.Euler(-tiltX, 0f, -tiltZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, target, smooth * Time.deltaTime);
    }
}
//using UnityEngine;

//public class BoardTiltKeyboard : MonoBehaviour
//{
//    public float maxTilt = 20f;   // максимальный наклон
//    public float smooth = 6f;     // сглаживание

//    private float angleX;
//    private float angleZ;

//    void Update()
//    {
//        float inputX = Input.GetAxis("Vertical");   
//        float inputZ = Input.GetAxis("Horizontal"); 

//        angleX = inputX * maxTilt;
//        angleZ = inputZ * maxTilt;

//        Quaternion target = Quaternion.Euler(-angleX, 0f, -angleZ);
//        transform.rotation = Quaternion.Lerp(
//            transform.rotation,
//            target,
//            smooth * Time.deltaTime
//        );
//    }
//}
