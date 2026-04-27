using System.Collections.Generic;
using UnityEngine;

public class BrushTool : IDrawTool
{
    private BrushDrawer controller;
    private StrokeRepository repo;

    private List<Vector2> points = new();
    private Vector2? lastPos = null;

    public BrushTool(BrushDrawer controller, StrokeRepository repo)
    {
        this.controller = controller;
        this.repo = repo;
    }

    public void OnDown(Vector2 pos)
    {
        points.Clear();
        points.Add(pos);
        lastPos = pos;

        controller.drawer.DrawCircle(controller.ToTexture(pos), controller.brushColor, controller.brushSize);
        controller.drawer.Apply();
    }

    public void OnDrag(Vector2 pos)
    {
        if (lastPos == null) return;

        points.Add(pos);

        controller.drawer.DrawLine(controller.ToTexture(lastPos.Value), controller.ToTexture(pos), controller.brushColor, controller.brushSize);
        controller.drawer.Apply();

        lastPos = pos;
    }

    public void OnUp(Vector2 pos)
    {
        if (points.Count == 0) return;

        var stroke = new DrawnStroke
        {
            tool = Tool.Brush,
            color = controller.brushColor,
            size = controller.brushSize,
            points = points.ToArray()
        };

        repo.ExecuteAction(new BrushStrokeAction(stroke));
        lastPos = null;
    }
}