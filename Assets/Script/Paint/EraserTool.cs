using UnityEngine;

public class EraserTool : IDrawTool
{
    private BrushDrawer drawer;
    private StrokeRepository repo;

    public EraserTool(BrushDrawer drawer, StrokeRepository repo)
    {
        this.drawer = drawer;
        this.repo = repo;
    }

    public void OnDown(Vector2 pos) => TryErase(pos);
    public void OnDrag(Vector2 pos) => TryErase(pos);
    public void OnUp(Vector2 pos) { }

    private void TryErase(Vector2 pos)
    {
        float radius = 0.05f;

        for (int i = repo.strokes.Count - 1; i >= 0; i--)
        {
            var stroke = repo.strokes[i];

            if (IsStrokeHit(stroke, pos, radius))
            {
                repo.ExecuteAction(new DeleteStrokeAction(stroke));
                drawer.RedrawAll();
                return;
            }
        }
    }

    private bool IsStrokeHit(DrawnStroke stroke, Vector2 pos, float r)
    {
        for (int i = 0; i < stroke.points.Length; i++)
        {
            if (Vector2.Distance(stroke.points[i], pos) < r)
                return true;
        }
        return false;
    }
}