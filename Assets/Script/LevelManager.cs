using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject squareBoardPrefab;
    public GameObject circleBoardPrefab;
    public GameObject ballPrefab;
    public GameObject cubePrefab;
    public GameObject finishPrefab;
    public GameObject deadZonePrefab;

    [Header("Plane")]
    public Vector3 planeScale = new Vector3(3, 3, 3);

    [Header("Brush")]
    [Range(0.1f, 1)] public float colliderShrink = 0.8f;

    [Header("Line")]
    public float cubeHeight = 0.5f;

    private GameObject currentBoard;
    private GameObject currentBall;
    private Transform levelContent;
    private Vector2 boardSize;
    private Vector3? spawnPoint;
    private int currentShape; // 0 = квадрат, 1 = круг

    void Start()
    {
        boardSize = new Vector2(10f * planeScale.x, 10f * planeScale.z);
        CreateLevel(LevelData.SelectedLevelPath);
    }

    public void CreateLevel(string levelFile)
    {

        string path;
#if UNITY_EDITOR
    path = Path.Combine(Application.streamingAssetsPath, levelFile);
    Debug.Log("[LevelManager] EDITOR path used");
#else
        path = Path.Combine(Application.persistentDataPath, levelFile);
        Debug.Log("[LevelManager] BUILD path used");
#endif

        Debug.Log("[LevelManager] Requested file: " + levelFile);
        Debug.Log("[LevelManager] Full path: " + path);
        if (!File.Exists(path))
        {
            Debug.LogError("Level file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);

        currentShape = wrapper.canvasShape;

        GameObject selectedPrefab = currentShape == 1
            ? circleBoardPrefab
            : squareBoardPrefab;

        if (currentBoard != null)
            Destroy(currentBoard);

        currentBoard = Instantiate(selectedPrefab);

        levelContent = currentBoard.transform.Find("LevelContent");
        if (levelContent == null)
        {
            levelContent = new GameObject("LevelContent").transform;
            levelContent.parent = currentBoard.transform;
        }

        BuildLevel(wrapper);
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

    void BuildLevel(StrokeListWrapper wrapper)
    {
        ClearLevel();

        List<CombineInstance> brushMeshes = new();
        spawnPoint = null;

        foreach (var stroke in wrapper.strokes)
        {
            switch (stroke.tool)
            {
                case Tool.Brush:
                    AddBrushStroke(stroke, brushMeshes);
                    break;

                case Tool.Line:
                    BuildLineStroke(stroke);
                    break;

                case Tool.Finish:
                    CreateTrigger(stroke.points[0], finishPrefab, "Finish");
                    break;

                case Tool.FakeFinish:
                    CreateTrigger(stroke.points[0], deadZonePrefab, "DeadZone");
                    break;

                case Tool.BallSpawn:
                    spawnPoint = ConvertToWorld(stroke.points[0]);
                    break;
            }
        }

        if (brushMeshes.Count > 0)
            CreateCombinedMesh(brushMeshes);
    }

    void ClearLevel()
    {
        foreach (Transform child in levelContent)
            Destroy(child.gameObject);
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

            // 👉 ограничение круга
            if (currentShape == 1 && !IsInsideCircle(pos))
                continue;

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

        if (currentShape == 1 && (!IsInsideCircle(start) || !IsInsideCircle(end)))
            return;

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

        if (currentShape == 1 && !IsInsideCircle(pos))
            return;

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, levelContent);
        obj.tag = tag;
    }

    Vector3 ConvertToWorld(Vector2 relative)
    {
        float x = relative.x * boardSize.x;
        float z = relative.y * boardSize.y;
        return new Vector3(x, 0, z);
    }

    bool IsInsideCircle(Vector3 pos)
    {
        float radius = boardSize.x / 2f;
        Vector2 p = new Vector2(pos.x, pos.z);
        return p.magnitude <= radius;
    }
}