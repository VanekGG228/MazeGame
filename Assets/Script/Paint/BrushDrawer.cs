using UnityEngine;
using UnityEngine.UI;

public class BrushDrawer : MonoBehaviour
{
    [Header("UI")]
    public RawImage rawImage;

    [Header("Canvas Shape UI")]
    public Image canvasImage;
    public Sprite squareSprite;
    public Sprite circleSprite;

    [Header("Brush")]
    public Color brushColor = Color.black;
    public int brushSize = 5;

    private Texture2D texture;
    internal TextureDrawer drawer;

    public StrokeRepository repository = new();

    private IDrawTool currentTool;

    private int canvasShape = 0;

    void Start()
    {
        texture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        drawer = new TextureDrawer(texture);

        ClearCanvas();
        rawImage.texture = texture;

        SetCanvasSquare(); 
        SetBrushTool();
    }

    bool IsPointerOverCanvas()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            rawImage.rectTransform,
            Input.mousePosition
        );
    }

    void Update()
    {
        Vector2 pos = GetMousePos();
        if (!IsPointerOverCanvas() || !IsInsideCanvas(pos)) return;

        if (Input.GetMouseButtonDown(0)) currentTool?.OnDown(GetMousePos());
        if (Input.GetMouseButton(0)) currentTool?.OnDrag(GetMousePos());
        if (Input.GetMouseButtonUp(0)) currentTool?.OnUp(GetMousePos());
    }


    Vector2 GetMousePos()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint
        );

        return new Vector2(
            localPoint.x / rawImage.rectTransform.rect.width,
            localPoint.y / rawImage.rectTransform.rect.height
        );
    }

    public Vector2 ToTexture(Vector2 rel)
    {
        return new Vector2((rel.x + 0.5f) * texture.width, (rel.y + 0.5f) * texture.height);
    }


    public bool IsInsideCanvas(Vector2 relPos)
    {
        if (canvasShape == 0)
        {
            return relPos.x >= -0.5f && relPos.x <= 0.5f &&
                   relPos.y >= -0.5f && relPos.y <= 0.5f;
        }
        else 
        {
            return relPos.sqrMagnitude <= 0.25f;
        }
    }

    public void SetCanvasSquare()
    {
        canvasShape = 0;
        if (canvasImage != null) canvasImage.sprite = squareSprite;
    }

    public void SetCanvasCircle()
    {
        canvasShape = 1;
        if (canvasImage != null) canvasImage.sprite = circleSprite;
    }

    public void ClearCanvas()
    {
        drawer.Clear(Color.white);
        repository.Clear();
        drawer.Apply();
    }

    public void RedrawAll()
    {
        drawer.Clear(Color.white);

        foreach (var s in repository.strokes)
        {
            if (s.tool == Tool.Brush)
            {
                for (int i = 1; i < s.points.Length; i++)
                {
                    if (!IsInsideCanvas(s.points[i])) continue;

                    drawer.DrawLine(
                        ToTexture(s.points[i - 1]),
                        ToTexture(s.points[i]),
                        s.color,
                        s.size
                    );
                }
            }
            else if (s.tool == Tool.Line)
            {
                drawer.DrawLine(
                    ToTexture(s.points[0]),
                    ToTexture(s.points[1]),
                    s.color,
                    s.size
                );
            }
            else
            {
                drawer.DrawCircle(
                    ToTexture(s.points[0]),
                    s.color,
                    s.size * 2
                );
            }
        }

        drawer.Apply();
    }

    public void SetBrushTool() =>
        currentTool = new BrushTool(this, repository); 

    public void SetLineTool() =>
        currentTool = new LineTool(this, drawer, repository);

    public void SetFinishTool() =>
        currentTool = new PointTool(this, repository, Tool.Finish);

    public void SetFakeFinishTool() =>
        currentTool = new PointTool(this, repository, Tool.FakeFinish); 

    public void SetBallSpawnTool() =>
        currentTool = new PointTool(this, repository, Tool.BallSpawn);

    public void SetEraserTool()
    {
        currentTool = new EraserTool(this, repository);
    }


    public void Save()
    {
        SaveLoadService.Save(repository, canvasShape);
    }

    public void LoadBlankCanvas()
    {
        repository.strokes.Clear();

        canvasShape = 0; 

        SetCanvasSquare(); 

        RedrawAll();

        Debug.Log("Blank canvas loaded");
    }

    public void Load(int levelId)
    {
        var data = SaveLoadService.Load(levelId);
        if (data == null) return;

        repository.strokes = data.strokes;

        if (data.canvasShape == 0)
            SetCanvasSquare();
        else
            SetCanvasCircle();

        RedrawAll();
    }


    public void Undo()
    {
        if (repository.Undo())
            RedrawAll();
    }

    public void Redo()
    {
        if (repository.Redo())
            RedrawAll();
    }
}