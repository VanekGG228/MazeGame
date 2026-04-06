using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialReader : MonoBehaviour
{
    public string portName = "COM6";
    public int baudRate = 115200;

    private SerialPort port;
    private Thread readThread;
    private bool running = false;

    public float[] imu = new float[12];
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
            readThread.IsBackground = true; // важно!
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

                if (!string.IsNullOrEmpty(data))
                {
                    leftover += data;

                    int pos;
                    while ((pos = leftover.IndexOf('\n')) >= 0)
                    {
                        string line = leftover.Substring(0, pos);
                        leftover = leftover.Substring(pos + 1);

                        line = line.Trim();

                        ParseMessage(line);
                    }
                }
            }
            catch (Exception e)
            {
                // можно оставить, но лучше редко логировать
                // Debug.LogWarning($"[Serial] Read error: {e.Message}");
            }

            Thread.Sleep(2);
        }
    }

    void ParseMessage(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
            return;

        try
        {
            string[] parts = msg.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length != 12)
                return;

            float[] values = new float[12];

            for (int i = 0; i < 12; i++)
            {
                if (!float.TryParse(parts[i],
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out values[i]))
                {
                    return;
                }
            }
            Debug.Log("values: " + string.Join(", ", values));
            Debug.Log("imu: " + string.Join(", ", imu));
            lock (lockObj)
            {
                for (int i = 0; i < 12; i++)
                {
                    imu[i] = values[i];
                }
            }
        }
        catch
        {
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

        if (readThread != null && readThread.IsAlive)
            readThread.Join();

        if (port != null && port.IsOpen)
            port.Close();

        Debug.Log("[Serial] Port closed.");
    }
}