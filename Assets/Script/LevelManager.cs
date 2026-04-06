using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject boardPrefab;
    public GameObject ballPrefab;
    public GameObject cubePrefab;
    public GameObject finishPrefab;      // финиш
    public GameObject deadZonePrefab;    // ловушка

    [Header("Plane")]
    public Vector3 planeScale = new Vector3(5, 5, 5);

    [Header("Brush")]
    [Range(0.1f, 1)] public float colliderShrink = 0.8f;
    [Header("Line")] public float cubeHeight = 0.5f;

    private GameObject currentBoard;
    private GameObject currentBall;
    private Transform levelContent;
    private Vector2 boardSize;
    private Vector3? spawnPoint;

    void Start()
    {
        boardSize = new Vector2(10f * planeScale.x, 10f * planeScale.z);
        CreateLevel(LevelData.SelectedLevelFile);
    }

    public void CreateLevel(string levelFile)
    {
        if (currentBoard != null) Destroy(currentBoard);

        currentBoard = Instantiate(boardPrefab);
        levelContent = currentBoard.transform.Find("LevelContent");
        if (levelContent == null)
        {
            levelContent = new GameObject("LevelContent").transform;
            levelContent.parent = currentBoard.transform;
        }

        LoadLevel(levelFile);
        SpawnPlayer();
        ApplySpawn();
    }

    void SpawnPlayer()
    {
        if (currentBall == null)
            currentBall = Instantiate(ballPrefab);

        currentBall.transform.parent = currentBoard.transform;
    }

    void ApplySpawn()
    {
        if (spawnPoint.HasValue)
            currentBall.transform.position = spawnPoint.Value + Vector3.up * 0.5f;
        else
            currentBall.transform.position = currentBoard.transform.position + Vector3.up * 0.5f;

        EventManager.RaisePlayerSpawned(currentBall);
    }

    void LoadLevel(string fileName)
    {
        ClearLevel();
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path)) { Debug.LogError("Level file not found: " + path); return; }

        string json = File.ReadAllText(path);
        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);
        BuildLevel(wrapper);
    }

    void ClearLevel()
    {
        foreach (Transform child in levelContent) Destroy(child.gameObject);
    }

    void BuildLevel(StrokeListWrapper wrapper)
    {
        List<CombineInstance> brushMeshes = new();
        spawnPoint = null;

        foreach (var stroke in wrapper.strokes)
        {
            switch (stroke.tool)
            {
                case Tool.Brush: AddBrushStroke(stroke, brushMeshes); break;
                case Tool.Line: BuildLineStroke(stroke); break;
                case Tool.Finish: CreateTrigger(stroke.points[0], finishPrefab, "Finish"); break;
                case Tool.FakeFinish: CreateTrigger(stroke.points[0], deadZonePrefab, "DeadZone"); break;
                case Tool.BallSpawn: spawnPoint = ConvertToWorld(stroke.points[0]); break;
            }
        }

        if (brushMeshes.Count > 0) CreateCombinedMesh(brushMeshes);
    }

    void AddBrushStroke(DrawnStroke stroke, List<CombineInstance> combineList)
    {
        HashSet<Vector2> used = new();
        Mesh cubeMesh = cubePrefab.GetComponent<MeshFilter>().sharedMesh;
        Vector3 cubeScale = cubePrefab.transform.localScale;

        foreach (var point in stroke.points)
        {
            if (used.Contains(point)) continue;
            used.Add(point);

            Vector3 pos = ConvertToWorld(point);

            CombineInstance ci = new CombineInstance();
            ci.mesh = cubeMesh;
            ci.transform = Matrix4x4.TRS(pos, Quaternion.identity, cubeScale);
            combineList.Add(ci);

            GameObject col = new GameObject("Collider");
            col.transform.parent = levelContent;
            col.transform.localPosition = pos;
            BoxCollider bc = col.AddComponent<BoxCollider>();
            bc.size = cubeScale * colliderShrink;
        }
    }

    void CreateCombinedMesh(List<CombineInstance> meshes)
    {
        GameObject combined = new GameObject("CombinedBrushMesh");
        combined.transform.parent = levelContent;
        MeshFilter mf = combined.AddComponent<MeshFilter>();
        MeshRenderer mr = combined.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(meshes.ToArray(), true, true);
        mf.mesh = mesh;
        mr.sharedMaterial = cubePrefab.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void BuildLineStroke(DrawnStroke stroke)
    {
        if (stroke.points.Length < 2) return;

        Vector3 start = ConvertToWorld(stroke.points[0]);
        Vector3 end = ConvertToWorld(stroke.points[1]);
        Vector3 center = (start + end) / 2;
        Vector3 dir = end - start;
        float length = dir.magnitude;

        GameObject cube = Instantiate(cubePrefab, center, Quaternion.identity, levelContent);
        cube.transform.rotation = Quaternion.LookRotation(dir);
        Vector3 scale = cube.transform.localScale;
        scale.z = length;
        scale.y = cubeHeight;
        cube.transform.localScale = scale;
    }

    void CreateTrigger(Vector2 point, GameObject prefab, string tag)
    {
        Vector3 pos = ConvertToWorld(point);
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, levelContent);
        obj.tag = tag;
    }

    Vector3 ConvertToWorld(Vector2 relative)
    {
        float x = relative.x * boardSize.x;
        float z = relative.y * boardSize.y;
        return new Vector3(x, 0, z);
    }
}