using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Renderer))]
public class Painter : MonoBehaviour
{
    public enum Tool { Brush, Line, Eraser }

    [Header("Texture")]
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public Color backgroundColor = Color.white;

    [Header("Brush")]
    public Color brushColor = Color.black;
    [Range(1, 200)] public int brushSize = 8;

    [Header("Tool & UI (assign in Inspector)")]
    public Tool activeTool = Tool.Brush;
    public Button brushButton;
    public Button lineButton;
    public Button eraserButton;
    public Slider sizeSlider;
    public Text sizeLabel;           // optional: to show size number
    public Button saveButton;        // optional: save to PNG

    // internals
    private Texture2D tex;
    private Renderer rend;
    private Vector2? lastPixel = null;   // for continuous stroke
    private Vector2 lineStartUV;
    private bool isMouseDown = false;

    void Start()
    {
        rend = GetComponent<Renderer>();

        tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        ClearTexture(backgroundColor);
        rend.material.mainTexture = tex;

        // hook UI if assigned
        if (brushButton != null) brushButton.onClick.AddListener(() => SetTool(Tool.Brush));
        if (lineButton != null) lineButton.onClick.AddListener(() => SetTool(Tool.Line));
        if (eraserButton != null) eraserButton.onClick.AddListener(() => SetTool(Tool.Eraser));
        if (sizeSlider != null)
        {
            sizeSlider.minValue = 1;
            sizeSlider.maxValue = 200;
            sizeSlider.value = brushSize;
            sizeSlider.onValueChanged.AddListener((v) => { brushSize = Mathf.RoundToInt(v); UpdateSizeLabel(); });
        }
        UpdateSizeLabel();
        if (saveButton != null) saveButton.onClick.AddListener(SavePNG);
    }

    void UpdateSizeLabel()
    {
        if (sizeLabel != null) sizeLabel.text = brushSize.ToString();
    }

    void Update()
    {
        // mouse handling
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            if (TryGetTextureCoord(out Vector2 uv))
            {
                if (activeTool == Tool.Line)
                {
                    lineStartUV = uv; // set start, draw on mouse up
                }
                else
                {
                    Vector2 pixel = UVToPixel(uv);
                    DrawAt(pixel);
                    lastPixel = pixel;
                }
            }
        }
        else if (Input.GetMouseButton(0) && isMouseDown)
        {
            if (activeTool == Tool.Brush || activeTool == Tool.Eraser)
            {
                if (TryGetTextureCoord(out Vector2 uv))
                {
                    Vector2 pixel = UVToPixel(uv);
                    if (lastPixel.HasValue)
                        DrawLinePixels(lastPixel.Value, pixel);
                    else
                        DrawAt(pixel);

                    lastPixel = pixel;
                }
            }
            // Line tool doesn't draw until mouse up
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isMouseDown)
            {
                if (TryGetTextureCoord(out Vector2 uv))
                {
                    if (activeTool == Tool.Line)
                    {
                        Vector2 lineEndPixel = UVToPixel(uv);
                        Vector2 lineStartPixel = UVToPixel(lineStartUV);
                        DrawLinePixels(lineStartPixel, lineEndPixel);
                    }
                    else
                    {
                        // ensure last point stamped
                        if (TryGetTextureCoord(out Vector2 uv2))
                        {
                            DrawAt(UVToPixel(uv2));
                        }
                    }
                }
            }

            isMouseDown = false;
            lastPixel = null;
            tex.Apply();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearTexture(backgroundColor);
            tex.Apply();
        }

        if (Input.GetKeyDown(KeyCode.B)) SetTool(Tool.Brush);
        if (Input.GetKeyDown(KeyCode.L)) SetTool(Tool.Line);
        if (Input.GetKeyDown(KeyCode.E)) SetTool(Tool.Eraser);
    }

    void SetTool(Tool t)
    {
        activeTool = t;
    }

    bool TryGetTextureCoord(out Vector2 uv)
    {
        uv = Vector2.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            uv = hit.textureCoord;
            return true;
        }
        return false;
    }

    Vector2 UVToPixel(Vector2 uv)
    {
        int x = Mathf.RoundToInt(uv.x * (textureWidth - 1));
        int y = Mathf.RoundToInt(uv.y * (textureHeight - 1));
        return new Vector2(x, y);
    }

    void DrawAt(Vector2 pixel)
    {
        int cx = Mathf.RoundToInt(pixel.x);
        int cy = Mathf.RoundToInt(pixel.y);
        Color col = activeTool == Tool.Eraser ? backgroundColor : brushColor;
        DrawFilledCircle(cx, cy, brushSize, col);
    }

    void DrawLinePixels(Vector2 p0, Vector2 p1)
    {
        float dist = Vector2.Distance(p0, p1);
        int steps = Mathf.CeilToInt(dist);
        if (steps == 0) steps = 1;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 p = Vector2.Lerp(p0, p1, t);
            DrawAt(p);
        }
    }

    void DrawFilledCircle(int cx, int cy, int radius, Color col)
    {
        int r = Mathf.Max(0, radius);
        int sqrR = r * r;
        int x0 = Mathf.Clamp(cx - r, 0, textureWidth - 1);
        int x1 = Mathf.Clamp(cx + r, 0, textureWidth - 1);
        int y0 = Mathf.Clamp(cy - r, 0, textureHeight - 1);
        int y1 = Mathf.Clamp(cy + r, 0, textureHeight - 1);

        for (int x = x0; x <= x1; x++)
        {
            int dx = x - cx;
            int dx2 = dx * dx;
            for (int y = y0; y <= y1; y++)
            {
                int dy = y - cy;
                if (dx2 + dy * dy <= sqrR)
                {
                    tex.SetPixel(x, y, col);
                }
            }
        }
    }

    void ClearTexture(Color color)
    {
        Color[] fill = new Color[textureWidth * textureHeight];
        for (int i = 0; i < fill.Length; i++) fill[i] = color;
        tex.SetPixels(fill);
        tex.Apply();
    }

    public void SavePNG()
    {
        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, "paint_result.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log("Saved paint to: " + path);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
