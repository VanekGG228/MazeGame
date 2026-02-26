using UnityEngine;
using System.IO;
using System.Collections.Generic; 


public class MazeBuilder : MonoBehaviour
{
    [Header("JSON & Walls")]
    public TextAsset jsonFile;
    public float wallHeight = 2f;
    public Material wallMaterial;

    [Header("Plane Settings")]
    public Vector2 planeSize = new Vector2(50f, 50f);
    public Material planeMaterial; 

    [Header("Wall Settings")]
    public float minWallThickness = 0.1f;

    private GameObject plane;

    void Start()
    {
        CreatePlane();
        BuildMaze();
    }

    void CreatePlane()
    {
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(planeSize.x / 10f, 1f, planeSize.y / 10f);
        plane.name = "MazePlane";

        MeshRenderer pmr = plane.GetComponent<MeshRenderer>();
        pmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; 
        pmr.receiveShadows = true;

        if (planeMaterial != null)
            pmr.material = planeMaterial;
    }

    void CreateWallSegment(Vector2 p1, Vector2 p2, float thickness)
    {
        Vector3 start = new Vector3(p1.x * planeSize.x, 0, p1.y * planeSize.y);
        Vector3 end = new Vector3(p2.x * planeSize.x, 0, p2.y * planeSize.y);

        Vector3 dir = end - start;
        float length = dir.magnitude;

        if (length < 0.001f) return;

        Vector3 center = (start + end) / 2f;

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = center + Vector3.up * (wallHeight / 2f);

        if (dir != Vector3.zero)
            wall.transform.rotation = Quaternion.LookRotation(dir);
        else
            wall.transform.rotation = Quaternion.identity;

        float wallThickness = Mathf.Max(thickness * 0.1f, minWallThickness);
        wall.transform.localScale = new Vector3(wallThickness, wallHeight, length);

        MeshRenderer mr = wall.GetComponent<MeshRenderer>();
        if (wallMaterial != null)
            mr.material = wallMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;

        Rigidbody rb = wall.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        wall.transform.parent = transform;
    }

    void BuildMaze()
    {
        if (jsonFile == null)
        {
            Debug.LogWarning("JSON file not assigned!");
            return;
        }

        var wrapper = JsonUtility.FromJson<StrokeListWrapper>(jsonFile.text);

        foreach (var stroke in wrapper.strokes)
        {
            Vector2[] simplifiedPoints = SimplifyStroke(stroke.points, 0.005f);

            for (int i = 1; i < simplifiedPoints.Length; i++)
            {
                CreateWallSegment(simplifiedPoints[i - 1], simplifiedPoints[i], stroke.size);
            }
        }
    }

    public static Vector2[] SimplifyStroke(Vector2[] points, float threshold = 0.005f)
    {
        if (points == null || points.Length == 0)
            return new Vector2[0];

        List<Vector2> simplified = new List<Vector2>();
        simplified.Add(points[0]);

        Vector2 lastPoint = points[0];

        for (int i = 1; i < points.Length; i++)
        {
            if ((points[i] - lastPoint).magnitude >= threshold)
            {
                simplified.Add(points[i]);
                lastPoint = points[i];
            }
        }

        return simplified.ToArray();
    }
}