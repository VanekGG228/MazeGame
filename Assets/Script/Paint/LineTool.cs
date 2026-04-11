using UnityEngine;

public class LineTool : IDrawTool
{
    private BrushDrawer controller;
    private TextureDrawer drawer;
    private StrokeRepository repo;

    private Vector2? startPos = null;

    public LineTool(BrushDrawer controller, TextureDrawer drawer, StrokeRepository repo)
    {
        this.controller = controller;
        this.drawer = drawer;
        this.repo = repo;
    }

    public void OnDown(Vector2 pos)
    {
        startPos = pos;
    }

    public void OnDrag(Vector2 pos)
    {
        if (startPos == null) return;

        // Перерисовываем всё, затем рисуем временную линию
        controller.RedrawAll();
        drawer.DrawLine(
            controller.ToTexture(startPos.Value),
            controller.ToTexture(pos),
            controller.brushColor,
            controller.brushSize
        );
        drawer.Apply();
    }

    public void OnUp(Vector2 pos)
    {
        if (startPos == null) return;

        var stroke = new DrawnStroke
        {
            tool = Tool.Line,
            color = controller.brushColor,
            size = controller.brushSize,
            points = new Vector2[] { startPos.Value, pos }
        };

        repo.ExecuteAction(new BrushStrokeAction(stroke));

        // Полная перерисовка
        controller.RedrawAll();
        startPos = null;
    }
}