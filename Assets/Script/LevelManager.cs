using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject boardPrefab;    // Prefab доски с Platform, Ball и LevelContent
    public GameObject cubePrefab;     // Префаб куба для всех объектов

    [Header("Plane Settings")]
    public Vector3 planeScale = new Vector3(5f, 5f, 5f); // scale твоего Plane
    private Vector2 boardSize;       // размер X,Z в мире

    [Header("Line Settings")]
    public float cubeHeight = 0.5f;   // высота куба для линий
    [Header("Collider Settings")]
    [Range(0.1f, 1f)]
    public float colliderShrink = 0.8f; // на сколько уменьшить BoxCollider

    private GameObject currentBoard;
    private Transform levelContent;
    private Transform ball;

    void Start()
    {
        boardSize = new Vector2(10f * planeScale.x, 10f * planeScale.z);

        CreateBoard();
        LoadLevel("drawing.json");
    }

    void CreateBoard()
    {
        currentBoard = Instantiate(boardPrefab);

        levelContent = currentBoard.transform.Find("LevelContent");
        if (levelContent == null)
            Debug.LogError("LevelContent not found in BoardPrefab");

        ball = currentBoard.transform.Find("Ball");
        if (ball == null)
            Debug.LogError("Ball not found in BoardPrefab");
    }

    public void LoadLevel(string fileName)
    {
        ClearLevel();

        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);

        BuildLevel(wrapper);
        ResetBall();
    }

    void BuildLevel(StrokeListWrapper wrapper)
    {
        List<CombineInstance> brushCombine = new List<CombineInstance>();

        foreach (var stroke in wrapper.strokes)
        {
            if (stroke.tool == Tool.Brush)
            {
                AddBrushToCombineWithColliders(stroke, brushCombine);
            }
            else if (stroke.tool == Tool.Line)
            {
                BuildLineStroke(stroke);
            }
        }

        if (brushCombine.Count > 0)
        {
            GameObject combinedBrush = new GameObject("BrushCombined");
            combinedBrush.transform.parent = levelContent;
            MeshFilter mf = combinedBrush.AddComponent<MeshFilter>();
            MeshRenderer mr = combinedBrush.AddComponent<MeshRenderer>();

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(brushCombine.ToArray());
            mf.mesh = combinedMesh;
            mr.sharedMaterial = cubePrefab.GetComponent<MeshRenderer>().sharedMaterial;
        }
    }

    void AddBrushToCombineWithColliders(DrawnStroke stroke, List<CombineInstance> combineList)
    {
        HashSet<Vector2> used = new HashSet<Vector2>();

        foreach (var point in stroke.points)
        {
            if (used.Contains(point)) continue;
            used.Add(point);

            Vector3 pos = ConvertToWorld(point);

            // --- визуальный Mesh ---
            MeshFilter mf = cubePrefab.GetComponent<MeshFilter>();
            if (mf != null)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = mf.sharedMesh;
                ci.transform = Matrix4x4.TRS(pos, Quaternion.identity, cubePrefab.transform.localScale);
                combineList.Add(ci);
            }

            // --- физический коллайдер (уменьшенный) ---
            GameObject cubeCollider = new GameObject("CubeCollider");
            cubeCollider.transform.parent = levelContent;
            cubeCollider.transform.localPosition = pos;
            cubeCollider.transform.localRotation = Quaternion.identity;

            BoxCollider bc = cubeCollider.AddComponent<BoxCollider>();
            bc.size = cubePrefab.transform.localScale * colliderShrink; // уменьшаем коллайдер
        }
    }

    void BuildLineStroke(DrawnStroke stroke)
    {
        if (stroke.points.Length < 2) return;

        Vector3 startPos = ConvertToWorld(stroke.points[0]);
        Vector3 endPos = ConvertToWorld(stroke.points[1]);

        Vector3 center = (startPos + endPos) / 2f;
        Vector3 dir = endPos - startPos;
        float length = dir.magnitude;

        GameObject lineCube = Instantiate(cubePrefab, center, Quaternion.identity, levelContent);

        if (dir != Vector3.zero)
            lineCube.transform.rotation = Quaternion.LookRotation(dir);

        Vector3 scale = lineCube.transform.localScale;
        scale.z = length;
        scale.y = cubeHeight;
        lineCube.transform.localScale = scale;

        BoxCollider bc = lineCube.GetComponent<BoxCollider>();
        if (bc == null) bc = lineCube.AddComponent<BoxCollider>();
        bc.size = new Vector3(scale.x, scale.y, scale.z);
    }

    Vector3 ConvertToWorld(Vector2 relative)
    {
        float x = relative.x * boardSize.x;
        float z = relative.y * boardSize.y;
        return new Vector3(x, 0f, z);
    }

    void ClearLevel()
    {
        foreach (Transform child in levelContent)
            Destroy(child.gameObject);
    }

    void ResetBall()
    {
        if (ball == null) return;

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ball.localPosition = new Vector3(0, 0.5f, 0);
    }
}