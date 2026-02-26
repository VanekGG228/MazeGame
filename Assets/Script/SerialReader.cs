using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialReader : MonoBehaviour
{
    public string portName = "COM8";
    public int baudRate = 115200;

    private SerialPort port;
    private Thread readThread;
    private bool running = false;

    // Ax Ay Az Gx Gy Gz
    public float[] imu = new float[6];

    private readonly object lockObj = new object();

    void Start()
    {
        port = new SerialPort(portName, baudRate);
        port.ReadTimeout = 50;

        try
        {
            port.Open();
            running = true;

            readThread = new Thread(ReadLoop);
            readThread.Start();

            Debug.Log($"[Serial] Port {portName} opened at {baudRate} baud.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Serial] Cannot open port: {e.Message}");
        }
    }

    void ReadLoop()
    {
        string leftover = "";

        while (running)
        {
            try
            {
                string data = port.ReadExisting();
                if (data.Length > 0)
                {
                    leftover += data;

                    int pos;
                    while ((pos = leftover.IndexOf('\n')) >= 0)
                    {
                        string line = leftover.Substring(0, pos);
                        leftover = leftover.Substring(pos + 1);

                        //Debug.Log($"[Serial] Received: {line.Trim()}");

                        ParseMessage(line.Trim());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Serial] Read error: {e.Message}");
            }

            Thread.Sleep(2);
        }
    }


    void ParseMessage(string msg)
    {

        string[] parts = msg.Split(' ');
        if (parts.Length != 6)
        {
            Debug.LogWarning($"[Serial] Invalid message: {msg}");
            return;
        }

        try
        {
            float ax = float.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
            float ay = float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            float az = float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);

            float gx = float.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
            float gy = float.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
            float gz = float.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture);

            lock (lockObj)
            {
                imu[0] = ax;
                imu[1] = ay;
                imu[2] = az;
                imu[3] = gx;
                imu[4] = gy;
                imu[5] = gz;
            }

            // --- DEBUG ---
            //Debug.Log($"[Serial] Ax={ax:F2} Ay={ay:F2} Az={az:F2} | Gx={gx:F2} Gy={gy:F2} Gz={gz:F2}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Serial] Parse error: {msg} | {e.Message}");
        }
    }

    public float[] GetIMU()
    {
        lock (lockObj)
        {
            return (float[])imu.Clone();
        }
    }

    void OnDestroy()
    {
        running = false;
        if (readThread != null) readThread.Join();
        if (port != null && port.IsOpen) port.Close();
        Debug.Log("[Serial] Port closed.");
    }
}
