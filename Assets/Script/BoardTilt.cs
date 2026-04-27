using UnityEngine;

public class BoardTiltIMU : MonoBehaviour
{
    public SerialReader serial;
    public float maxTilt = 30f;   
    public float smooth = 2f;     
    public float alpha = 0.7f;    

    private float angleX = 0f;
    private float angleZ = 0f;

    private float offsetX = 0f;  
    private float offsetZ = 0f;

    private float lastTime;

    void Start()
    {
        lastTime = Time.time;

        if (serial == null)
            serial = FindFirstObjectByType<SerialReader>();

        float defaultValue = -999f;

        offsetX = PlayerPrefs.GetFloat("IMU_OffsetX", defaultValue);
        offsetZ = PlayerPrefs.GetFloat("IMU_OffsetZ", defaultValue);

        if (offsetX == defaultValue || offsetZ == defaultValue)
        {
            Debug.LogWarning("[IMU] No saved offsets found!");
        }
        else
        {
            Debug.Log($"[IMU] Loaded offsets: X={offsetX}, Z={offsetZ}");
        }
    }

    public void Calibrate()
    {
        if (serial == null)
            return;

        float[] imu = serial.GetIMU();
        if (imu.Length < 12)
            return;

        //float ax = (imu[0] + imu[6]) / 2f;
        //float ay = (imu[1] + imu[7]) / 2f;
        //float az = (imu[2] + imu[8]) / 2f;
        float ax = (imu[0] + imu[6]) ;
        float ay = (imu[1] + imu[7]);
        float az = (imu[2] + imu[8]);

        offsetX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
        offsetZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

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

        float ax = (imu[0] + imu[6]) ;
        float ay = (imu[1] + imu[7]) ;
        float az = (imu[2] + imu[8]);

        float gx = (imu[3] + imu[9]) ;
        float gy = (imu[4] + imu[10]);

        float dt = Time.time - lastTime;
        lastTime = Time.time;

        float accAngleX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
        float accAngleZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        angleX += gx * dt;
        angleZ += gy * dt;

        angleX = alpha * angleX + (1f - alpha) * accAngleX;
        angleZ = alpha * angleZ + (1f - alpha) * accAngleZ;

        float tiltX = angleX - offsetX;
        float tiltZ = angleZ - offsetZ;

        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltZ = Mathf.Clamp(tiltZ, -maxTilt, maxTilt);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(-tiltX, 0f, -tiltZ),
            smooth * Time.deltaTime
        );
    }
}