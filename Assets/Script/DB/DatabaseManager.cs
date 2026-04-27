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

            CREATE TABLE IF NOT EXISTS Levels (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT,
                difficulty TEXT,
                path TEXT
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
            );";
        cmd.ExecuteNonQuery();
        Debug.Log("[DB] Tables ensured");
    }

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

    public void UpdateLevelPath(int levelId, string path)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
        UPDATE Levels
        SET path = @path
        WHERE id = @id;
    ";

        cmd.Parameters.AddWithValue("@path", path);
        cmd.Parameters.AddWithValue("@id", levelId);

        cmd.ExecuteNonQuery();
    }

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


    public List<LevelDataRow> GetAllLevels()
    {
        List<LevelDataRow> levels = new List<LevelDataRow>();

        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = "SELECT id, name, difficulty, path FROM Levels ORDER BY id DESC;";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            levels.Add(new LevelDataRow
            {
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                difficulty = reader.GetString(2),
                path = reader.GetString(3)
            });
        }

        return levels;
    }

    public List<Vector2> GetTrajectoryForAttempt(int attemptId)
    {
        List<Vector2> points = new List<Vector2>();

        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        SELECT posX, posZ
        FROM TrajectoryPoints
        WHERE attemptId = @id
        ORDER BY time ASC;
    ";

        cmd.Parameters.AddWithValue("@id", attemptId);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            float x = reader.GetFloat(0);
            float z = reader.GetFloat(1);

            points.Add(new Vector2(x, z));
        }

        return points;
    }

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
            INNER JOIN Statistics s ON a.id = s.attemptId
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

    public StatisticsData GetStatisticsForAttempt(int attemptId)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        SELECT distance, avgSpeed, maxSpeed, collisions, duration
        FROM Statistics
        WHERE attemptId = @attemptId
        LIMIT 1;
    ";

        cmd.Parameters.AddWithValue("@attemptId", attemptId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new StatisticsData
            {
                distance = reader.GetFloat(0),
                avgSpeed = reader.GetFloat(1),
                maxSpeed = reader.GetFloat(2),
                collisions = reader.GetInt32(3),
                duration = reader.GetFloat(4)
            };
        }

        return null;
    }

    public int InsertLevel(string name, string difficulty, string path)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();
        using var cmd = connection.CreateCommand();

        cmd.CommandText = @"
        INSERT INTO Levels (name, difficulty, path)
        VALUES (@name, @difficulty, @path);
        SELECT last_insert_rowid();
    ";

        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@difficulty", difficulty);
        cmd.Parameters.AddWithValue("@path", path);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }


    public int GetSessionLevelIdFromAttempt(int attemptId)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        SELECT s.levelId
        FROM Sessions s
        JOIN Attempts a ON a.sessionId = s.id
        WHERE a.id = @id;
    ";

        cmd.Parameters.AddWithValue("@id", attemptId);

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : -1;
    }

    public LevelDataRow GetLevelById(int levelId)
    {
        using var connection = new SqliteConnection(dbPath);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
        SELECT id, name, difficulty, path
        FROM Levels
        WHERE id = @id;
    ";

        cmd.Parameters.AddWithValue("@id", levelId);

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new LevelDataRow
            {
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                difficulty = reader.GetString(2),
                path = reader.GetString(3)
            };
        }

        return null;
    }
}