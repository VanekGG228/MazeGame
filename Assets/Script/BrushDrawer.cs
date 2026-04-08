using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class BrushDrawerEditor : MonoBehaviour
{
    [Header("UI")]
    public RawImage rawImage;

    [Header("Brush Settings")]
    public Color brushColor = Color.black;
    public int brushSize = 5;

    [Header("Mask")]
    public Image canvasImage; 
    public Sprite squareSprite;
    public Sprite circleSprite;

    [Header("Tool")]
    public Tool currentTool = Tool.Brush;

    [Header("Canvas Shape")]
    public int canvasShape = 0;

    private Texture2D texture;
    private Vector2? lastBrushPos = null;
    private Vector2? lineStartPos = null;
    private List<Vector2> currentStrokePoints = new List<Vector2>();
    public List<DrawnStroke> strokes = new List<DrawnStroke>();
    private Vector2? lastBallSpawn = null;

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
        if (!IsInsideImage(mousePos)) return;

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
        else
        {
            SavePointObject(mousePos, currentTool);
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePos = GetMouseRelativePosition();
        if (!IsInsideImage(mousePos)) return;

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
        if (!IsInsideImage(mousePos)) return;

        if (currentTool == Tool.Brush)
            SaveCurrentBrushStroke();
        else if (currentTool == Tool.Line && lineStartPos != null)
            SaveLineStroke(mousePos);
    }

    private bool IsInsideImage(Vector2 relPos)
    {
        if (canvasShape == 0)
            return relPos.x >= -0.5f && relPos.x <= 0.5f && relPos.y >= -0.5f && relPos.y <= 0.5f;

        return relPos.sqrMagnitude <= 0.25f;
    }

    private void SavePointObject(Vector2 pos, Tool tool)
    {
        if (tool == Tool.BallSpawn)
        {
            lastBallSpawn = pos;
            DrawPointObject(pos, tool);
        }
        else
        {
            strokes.Add(new DrawnStroke
            {
                tool = tool,
                color = brushColor,
                size = brushSize,
                points = new Vector2[] { pos }
            });
            DrawPointObject(pos, tool);
        }
        texture.Apply();
    }

    private void DrawPointObject(Vector2 relPos, Tool tool)
    {
        Vector2 texPos = RelativeToTexture(relPos);
        Color color = Color.green;

        switch (tool)
        {
            case Tool.Finish: color = Color.green; break;
            case Tool.FakeFinish: color = Color.red; break;
            case Tool.BallSpawn: color = Color.blue; break;
        }

        DrawCircleCustom(texPos, color, brushSize * 2);
    }

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

    private void DrawCircle(Vector2 pos) => DrawCircleCustom(pos, brushColor, brushSize);
    private void DrawLine(Vector2 from, Vector2 to) => DrawLineCustom(from, to, brushColor, brushSize);

    private void DrawCircleCustom(Vector2 pos, Color color, int size)
    {
        int cx = (int)pos.x, cy = (int)pos.y;
        for (int x = -size; x <= size; x++)
            for (int y = -size; y <= size; y++)
                if (x * x + y * y <= size * size)
                {
                    int px = cx + x, py = cy + y;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        texture.SetPixel(px, py, color);
                }
    }

    private void DrawLineCustom(Vector2 from, Vector2 to, Color color, int size)
    {
        int x0 = (int)from.x, y0 = (int)from.y;
        int x1 = (int)to.x, y1 = (int)to.y;
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
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

    private void CreateTexture()
    {
        texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        ClearCanvas();
        rawImage.texture = texture;
    }

    private Vector2 GetMouseRelativePosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint
        );
        return new Vector2(localPoint.x / rawImage.rectTransform.rect.width, localPoint.y / rawImage.rectTransform.rect.height);
    }

    private Vector2 RelativeToTexture(Vector2 rel)
    {
        return new Vector2((rel.x + 0.5f) * texture.width, (rel.y + 0.5f) * texture.height);
    }

    private void ClearCanvasWithoutReset()
    {
        Color[] fillColor = new Color[texture.width * texture.height];
        for (int i = 0; i < fillColor.Length; i++) fillColor[i] = Color.white;
        texture.SetPixels(fillColor);
    }

    public void ClearCanvas()
    {
        strokes.Clear();
        lastBallSpawn = null;
        Color[] fillColor = new Color[texture.width * texture.height];
        for (int i = 0; i < fillColor.Length; i++) fillColor[i] = Color.white;
        texture.SetPixels(fillColor);
        texture.Apply();
    }

    public void RedrawAll()
    {
        ClearCanvasWithoutReset();
        foreach (var stroke in strokes)
        {
            if (stroke.tool == Tool.Brush)
                for (int i = 1; i < stroke.points.Length; i++)
                    DrawLine(RelativeToTexture(stroke.points[i - 1]), RelativeToTexture(stroke.points[i]));
            else if (stroke.tool == Tool.Line)
                DrawLine(RelativeToTexture(stroke.points[0]), RelativeToTexture(stroke.points[1]));
            else if (stroke.tool == Tool.Finish || stroke.tool == Tool.FakeFinish)
                DrawPointObject(stroke.points[0], stroke.tool);
        }
        if (lastBallSpawn.HasValue)
            DrawPointObject(lastBallSpawn.Value, Tool.BallSpawn);
        texture.Apply();
    }

    // ===================== JSON SAVE / LOAD =====================

    public void SaveToDisk(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) fileName = "drawing1.json";
        List<DrawnStroke> strokesToSave = new List<DrawnStroke>(strokes);

        if (lastBallSpawn.HasValue)
        {
            strokesToSave.Add(new DrawnStroke
            {
                tool = Tool.BallSpawn,
                color = brushColor,
                size = brushSize,
                points = new Vector2[] { lastBallSpawn.Value }
            });
        }

        StrokeListWrapper wrapper = new StrokeListWrapper
        {
            strokes = strokesToSave,
            canvasShape = canvasShape
        };
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);
        Debug.Log("Saved: " + path);
    }

    public void LoadFromDisk(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) fileName = "drawing.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);

        strokes = new List<DrawnStroke>();
        lastBallSpawn = null;
        canvasShape = wrapper.canvasShape;

        foreach (var s in wrapper.strokes)
        {
            if (s.tool == Tool.BallSpawn)
                lastBallSpawn = s.points[0];
            else
                strokes.Add(s);
        }
        RedrawAll();
        Debug.Log("Loaded: " + path);
    }

    // ===================== UI Tool Set =====================
    public void SetToolBrush() => currentTool = Tool.Brush;
    public void SetToolLine() => currentTool = Tool.Line;
    public void SetToolFinish() => currentTool = Tool.Finish;
    public void SetToolFakeFinish() => currentTool = Tool.FakeFinish;
    public void SetToolBallSpawn() => currentTool = Tool.BallSpawn;



    public void SetCanvasSquare()
    {
        canvasShape = 0;
        canvasImage.sprite = squareSprite;
    }

    public void SetCanvasCircle()
    {
        canvasShape = 1;
        canvasImage.sprite = circleSprite;
    }
}