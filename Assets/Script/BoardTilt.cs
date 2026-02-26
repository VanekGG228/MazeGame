//using UnityEngine;

//public class BoardTiltIMU : MonoBehaviour
//{
//    public SerialReader serial; 
//    public float maxTilt = 20f;  
//    public float smooth = 6f;    
//    public float alpha = 0.93f;

//    private float angleX = 0f; 
//    private float angleZ = 0f; 

//    private float lastTime;

//    void Start()
//    {
//        lastTime = Time.time;
//    }

//    void Update()
//    {
//        float[] imu = serial.GetIMU();
//        float ax = imu[0]; 
//        float ay = imu[1]; 
//        float az = imu[2]; 

//        float gx = imu[3]; 
//        float gy = imu[4]; 

//        float dt = Time.time - lastTime;
//        lastTime = Time.time;

//        float accAngleX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
//        float accAngleZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

//        angleX += gx * dt;
//        angleZ += gy * dt;

//        angleX = alpha * angleX + (1f - alpha) * accAngleX;
//        angleZ = alpha * angleZ + (1f - alpha) * accAngleZ;

//        float tiltX = Mathf.Clamp(angleX, -1f, 1f) * maxTilt;
//        float tiltZ = Mathf.Clamp(angleZ, -1f, 1f) * maxTilt;

//        Quaternion target = Quaternion.Euler(-tiltX, 0f, -tiltZ);
//        transform.rotation = Quaternion.Lerp(transform.rotation, target, smooth * Time.deltaTime);
//    }
//}


using UnityEngine;

public class BoardTiltKeyboard : MonoBehaviour
{
    public float maxTilt = 20f;   // максимальный наклон
    public float smooth = 6f;     // сглаживание

    private float angleX;
    private float angleZ;

    void Update()
    {
        float inputX = Input.GetAxis("Vertical");   // W/S
        float inputZ = Input.GetAxis("Horizontal"); // A/D

        angleX = inputX * maxTilt;
        angleZ = inputZ * maxTilt;

        Quaternion target = Quaternion.Euler(-angleX, 0f, -angleZ);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            target,
            smooth * Time.deltaTime
        );
    }
}
