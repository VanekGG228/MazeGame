using UnityEngine;
using TMPro;

public class IMUCalibration : MonoBehaviour
{
    public SerialReader serial;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI instructionText;

    public float calibrationTime = 3f;
    private float elapsedTime = 0f;
    private bool isCalibrating = false;

    private float sumX = 0f;
    private float sumZ = 0f;
    private int sampleCount = 0;

    void Start()
    {
        if (serial == null)
            serial = FindFirstObjectByType<SerialReader>();

        timerText.text = calibrationTime.ToString("F1");
        if (instructionText != null)
            instructionText.text = "Держите устройство ровно";

        StartCalibration();
    }

    void StartCalibration()
    {
        elapsedTime = 0f;
        sumX = 0f;
        sumZ = 0f;
        sampleCount = 0;
        isCalibrating = true;
    }

    void Update()
    {
        if (!isCalibrating || serial == null) return;

        float[] imu = serial.GetIMU();
        if (imu.Length < 12) return;

        float ax = (imu[0] + imu[6]) / 2f;
        float ay = (imu[1] + imu[7]) / 2f;
        float az = (imu[2] + imu[8]) / 2f;

        float accAngleX = Mathf.Atan2(ay, az) * Mathf.Rad2Deg;
        float accAngleZ = Mathf.Atan2(-ax, az) * Mathf.Rad2Deg;

        sumX += accAngleX;
        sumZ += accAngleZ;
        sampleCount++;

        elapsedTime += Time.deltaTime;
        timerText.text = Mathf.Max(0f, calibrationTime - elapsedTime).ToString("F1");

        if (elapsedTime >= calibrationTime)
            FinishCalibration();
    }

    void FinishCalibration()
    {
        isCalibrating = false;

        if (sampleCount == 0)
        {
            Debug.LogWarning("No IMU samples collected during calibration!");
            return;
        }

        float offsetX = sumX / sampleCount;
        float offsetZ = sumZ / sampleCount;

        PlayerPrefs.SetFloat("IMU_OffsetX", offsetX);
        PlayerPrefs.SetFloat("IMU_OffsetZ", offsetZ);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}