using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BrushDrawer : MonoBehaviour
{
    [Header("UI")]
    public RawImage rawImage;

    [Header("Brush Settings")]
    public Color brushColor = Color.black;
    public int brushSize = 5;

    [Header("Tool")]
    public Tool currentTool = Tool.Brush;

    private Texture2D texture;

    private Vector2? lastBrushPos = null;
    private Vector2? lineStartPos = null;
    private List<Vector2> currentStrokePoints = new List<Vector2>();

    public List<DrawnStroke> strokes = new List<DrawnStroke>();

    void Start()
    {
        CreateTexture();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) OnMouseDown();
        if (Input.GetMouseButton(0)) OnMouseDrag();
        if (Input.GetMouseButtonUp(0)) OnMouseUp();
    }

    private void OnMouseDown()
    {
        Vector2 mousePos = GetMouseRelativePosition();

        if (currentTool == Tool.Brush)
        {
            lastBrushPos = mousePos;
            currentStrokePoints.Clear();
            currentStrokePoints.Add(mousePos);
            DrawCircle(RelativeToTexture(mousePos));
            texture.Apply();
        }
        else if (currentTool == Tool.Line)
        {
            lineStartPos = mousePos;
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePos = GetMouseRelativePosition();

        if (currentTool == Tool.Brush && lastBrushPos != null)
        {
            DrawLine(RelativeToTexture(lastBrushPos.Value), RelativeToTexture(mousePos));
            currentStrokePoints.Add(mousePos);
            lastBrushPos = mousePos;
            texture.Apply();
        }
    }

    private void OnMouseUp()
    {
        Vector2 mousePos = GetMouseRelativePosition();

        if (currentTool == Tool.Brush)
        {
            SaveCurrentBrushStroke();
        }
        else if (currentTool == Tool.Line && lineStartPos != null)
        {
            SaveLineStroke(mousePos);
        }
    }

    // ===================== SAVE =====================

    private void SaveCurrentBrushStroke()
    {
        if (currentStrokePoints.Count == 0) return;

        strokes.Add(new DrawnStroke
        {
            tool = Tool.Brush,
            color = brushColor,
            size = brushSize,
            points = currentStrokePoints.ToArray()
        });

        currentStrokePoints.Clear();
        lastBrushPos = null;
    }

    private void SaveLineStroke(Vector2 endPos)
    {
        strokes.Add(new DrawnStroke
        {
            tool = Tool.Line,
            color = brushColor,
            size = brushSize,
            points = new Vector2[] { lineStartPos.Value, endPos }
        });

        DrawLine(RelativeToTexture(lineStartPos.Value), RelativeToTexture(endPos));
        texture.Apply();
        lineStartPos = null;
    }

    // ===================== DRAWING =====================

    private void DrawCircle(Vector2 pos)
    {
        DrawCircleCustom(pos, brushColor, brushSize);
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        DrawLineCustom(from, to, brushColor, brushSize);
    }

    private void DrawCircleCustom(Vector2 pos, Color color, int size)
    {
        int cx = (int)pos.x;
        int cy = (int)pos.y;

        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (x * x + y * y <= size * size)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private void DrawLineCustom(Vector2 from, Vector2 to, Color color, int size)
    {
        int x0 = (int)from.x;
        int y0 = (int)from.y;
        int x1 = (int)to.x;
        int y1 = (int)to.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircleCustom(new Vector2(x0, y0), color, size);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    // ===================== TEXTURE & UTILS =====================

    private void CreateTexture()
    {
        texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        ClearCanvas();
        rawImage.texture = texture;
    }

    private Vector2 GetMouseRelativePosition()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            Input.mousePosition,
            null,
            out localPoint
        );

        float relativeX = localPoint.x / rawImage.rectTransform.rect.width;
        float relativeY = localPoint.y / rawImage.rectTransform.rect.height;

        return new Vector2(relativeX, relativeY); // -0.5..0.5 от центра
    }

    private Vector2 RelativeToTexture(Vector2 rel)
    {
        float px = (rel.x + 0.5f) * texture.width;
        float py = (rel.y + 0.5f) * texture.height;
        return new Vector2(px, py);
    }

    public void ClearCanvas()
    {
        Color[] fillColor = new Color[texture.width * texture.height];
        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = Color.white;

        texture.SetPixels(fillColor);
        texture.Apply();

        strokes.Clear();
    }

    private void ClearCanvasWithoutReset()
    {
        Color[] fillColor = new Color[texture.width * texture.height];
        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = Color.white;

        texture.SetPixels(fillColor);
    }

    // ===================== JSON SAVE / LOAD =====================

    public void SaveToJson()
    {
        StrokeListWrapper wrapper = new StrokeListWrapper();
        wrapper.strokes = strokes;

        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.persistentDataPath, "drawing.json");

        File.WriteAllText(path, json);
        Debug.Log("Saved to: " + path);
    }

    public void LoadFromJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "drawing.json");
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);

        strokes = wrapper.strokes;

        RedrawAll();
    }

    public void RedrawAll()
    {
        ClearCanvasWithoutReset();

        foreach (var stroke in strokes)
        {
            if (stroke.tool == Tool.Brush)
            {
                for (int i = 1; i < stroke.points.Length; i++)
                {
                    DrawLine(RelativeToTexture(stroke.points[i - 1]), RelativeToTexture(stroke.points[i]));
                }
            }
            else if (stroke.tool == Tool.Line)
            {
                DrawLine(RelativeToTexture(stroke.points[0]), RelativeToTexture(stroke.points[1]));
            }
        }

        texture.Apply();
    }

    // ===================== UI =====================

    public void SetToolBrush() { currentTool = Tool.Brush; }
    public void SetToolLine() { currentTool = Tool.Line; }
}
