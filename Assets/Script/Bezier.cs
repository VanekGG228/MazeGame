using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedWallSimple : MonoBehaviour
{
    public float height = 3f;
    public float thickness = 0.5f;
    public int segments = 20;

    // Контрольные точки Bezier для скобки
    public Vector3 p0 = new Vector3(0, 0, 0);
    public Vector3 p1 = new Vector3(0, 0, 3);
    public Vector3 p2 = new Vector3(2, 0, 3);
    public Vector3 p3 = new Vector3(2, 0, 0);

    void Start()
    {
        Mesh mesh = new Mesh();

        // 1. Вычисляем точки вдоль кривой
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            points[i] = BezierPoint(t, p0, p1, p2, p3);
        }

        // 2. Создаём вершины с толщиной
        Vector3[] vertices = new Vector3[(segments + 1) * 4];
        for (int i = 0; i <= segments; i++)
        {
            Vector3 point = points[i];
            Vector3 prev = (i == 0) ? points[i] : points[i - 1];
            Vector3 next = (i == segments) ? points[i] : points[i + 1];

            Vector3 dir = (next - prev).normalized;
            Vector3 normal = Vector3.Cross(dir, Vector3.up);

            Vector3 left = point - normal * thickness / 2;
            Vector3 right = point + normal * thickness / 2;

            vertices[i * 4 + 0] = left;                  // низ слева
            vertices[i * 4 + 1] = right;                 // низ справа
            vertices[i * 4 + 2] = left + Vector3.up * height;   // верх слева
            vertices[i * 4 + 3] = right + Vector3.up * height;  // верх справа
        }

        // 3. Создаём треугольники
        int[] triangles = new int[segments * 12];
        for (int i = 0; i < segments; i++)
        {
            int i0 = i * 4;
            int i1 = i0 + 4;

            // передняя грань
            triangles[i * 12 + 0] = i0 + 2;
            triangles[i * 12 + 1] = i0 + 0;
            triangles[i * 12 + 2] = i1 + 0;

            triangles[i * 12 + 3] = i1 + 2;
            triangles[i * 12 + 4] = i0 + 2;
            triangles[i * 12 + 5] = i1 + 0;

            // верхняя грань
            triangles[i * 12 + 6] = i0 + 2;
            triangles[i * 12 + 7] = i1 + 2;
            triangles[i * 12 + 8] = i1 + 3;

            triangles[i * 12 + 9] = i0 + 2;
            triangles[i * 12 + 10] = i1 + 3;
            triangles[i * 12 + 11] = i0 + 3;
        }

        // 4. Применяем массивы к мешу
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // 5. Назначаем меш фильтру и коллайдеру
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        // 6. Добавьте материал через MeshRenderer в инспекторе
    }

    Vector3 BezierPoint(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * a;
        p += 3 * uu * t * b;
        p += 3 * u * tt * c;
        p += ttt * d;
        return p;
    }
}

