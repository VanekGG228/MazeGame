using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    private string dbPath;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        dbPath = "URI=file:" + Application.persistentDataPath + "/game.db";
        Debug.Log("[DB] Path: " + dbPath);
        CreateTables();
    }

    void CreateTables()
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT,
            age INTEGER,
            createdAt TEXT
        );

        CREATE TABLE IF NOT EXISTS Sessions (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            userId INTEGER,
            levelId INTEGER,
            startTime TEXT,
            endTime TEXT
        );

        CREATE TABLE IF NOT EXISTS Attempts (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            sessionId INTEGER,
            result TEXT,
            completionTime REAL,
            errorsCount INTEGER
        );

        CREATE TABLE IF NOT EXISTS TrajectoryPoints (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            attemptId INTEGER,
            posX REAL,
            posY REAL,
            posZ REAL,
            time REAL
        );

        CREATE TABLE IF NOT EXISTS Statistics (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            attemptId INTEGER,
            distance REAL,
            avgSpeed REAL,
            maxSpeed REAL,
            collisions INTEGER,
            duration REAL
        );
        ";
        cmd.ExecuteNonQuery();
        Debug.Log("[DB] Tables ensured");
    }

    // Создание и завершение сессий
    public int CreateSession(int userId, int levelId)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Sessions (userId, levelId, startTime)
            VALUES (@userId, @levelId, datetime('now'));
            SELECT last_insert_rowid();
        ";
        cmd.Parameters.AddWithValue("@userId", userId);
        cmd.Parameters.AddWithValue("@levelId", levelId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void EndSession(int sessionId)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"UPDATE Sessions SET endTime = datetime('now') WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", sessionId);
        cmd.ExecuteNonQuery();
    }

    // Работа с попытками
    public int InsertAttempt(int sessionId, string result, float completionTime, int errors)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Attempts (sessionId, result, completionTime, errorsCount)
            VALUES (@sessionId, @res, @time, @err);
            SELECT last_insert_rowid();
        ";
        cmd.Parameters.AddWithValue("@sessionId", sessionId);
        cmd.Parameters.AddWithValue("@res", result);
        cmd.Parameters.AddWithValue("@time", completionTime);
        cmd.Parameters.AddWithValue("@err", errors);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // Траектория привязана к попытке
    public void InsertTrajectoryPoint(int attemptId, Vector3 pos, float time)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO TrajectoryPoints (attemptId, posX, posY, posZ, time)
            VALUES (@attemptId, @x, @y, @z, @time);
        ";
        cmd.Parameters.AddWithValue("@attemptId", attemptId);
        cmd.Parameters.AddWithValue("@x", pos.x);
        cmd.Parameters.AddWithValue("@y", pos.y);
        cmd.Parameters.AddWithValue("@z", pos.z);
        cmd.Parameters.AddWithValue("@time", time);
        cmd.ExecuteNonQuery();
    }

    public List<Vector3> GetTrajectoryForAttempt(int attemptId)
    {
        List<Vector3> points = new List<Vector3>();
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT posX, posY, posZ
            FROM TrajectoryPoints
            WHERE attemptId = @id
            ORDER BY time ASC;
        ";
        cmd.Parameters.AddWithValue("@id", attemptId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            float x = reader.GetFloat(0);
            float y = reader.GetFloat(1);
            float z = reader.GetFloat(2);
            points.Add(new Vector3(x, y, z));
        }
        return points;
    }

    // Статистика по попытке
    public void InsertStatistics(int attemptId, float distance, float avgSpeed, float maxSpeed, int collisions, float duration)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Statistics (attemptId, distance, avgSpeed, maxSpeed, collisions, duration)
            VALUES (@attemptId, @dist, @avg, @max, @col, @dur);
        ";
        cmd.Parameters.AddWithValue("@attemptId", attemptId);
        cmd.Parameters.AddWithValue("@dist", distance);
        cmd.Parameters.AddWithValue("@avg", avgSpeed);
        cmd.Parameters.AddWithValue("@max", maxSpeed);
        cmd.Parameters.AddWithValue("@col", collisions);
        cmd.Parameters.AddWithValue("@dur", duration);
        cmd.ExecuteNonQuery();
    }

    public int GetLastAttemptId(int sessionId)
    {
        int lastId = -1;
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id FROM Attempts WHERE sessionId=@sid ORDER BY id DESC LIMIT 1;";
        cmd.Parameters.AddWithValue("@sid", sessionId);
        var result = cmd.ExecuteScalar();
        if (result != null) lastId = Convert.ToInt32(result);
        return lastId;
    }

    public void UpdateAttemptResult(int attemptId, string result, float completionTime, int errors)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        UPDATE Attempts
        SET result = @res,
            completionTime = @time,
            errorsCount = @err
        WHERE id = @id;
    ";
        cmd.Parameters.AddWithValue("@res", result);
        cmd.Parameters.AddWithValue("@time", completionTime);
        cmd.Parameters.AddWithValue("@err", errors);
        cmd.Parameters.AddWithValue("@id", attemptId);
        cmd.ExecuteNonQuery();
    }

    public List<AttemptData> GetAllAttemptsWithTrajectory()
    {
        List<AttemptData> attempts = new List<AttemptData>();

        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
        SELECT DISTINCT a.id, a.result, a.completionTime, a.errorsCount
        FROM Attempts a
        INNER JOIN TrajectoryPoints t ON a.id = t.attemptId
        ORDER BY a.id DESC;
    ";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            attempts.Add(new AttemptData
            {
                id = reader.GetInt32(0),
                result = reader.GetString(1),
                time = reader.GetFloat(2),
                errors = reader.GetInt32(3)
            });
        }

        return attempts;
    }
}