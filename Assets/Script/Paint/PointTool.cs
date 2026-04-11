using UnityEngine;

public class PointTool : IDrawTool
{
    private BrushDrawer controller;
    private StrokeRepository repo;
    private Tool type;

    public PointTool(BrushDrawer controller, StrokeRepository repo, Tool type)
    {
        this.controller = controller;
        this.repo = repo;
        this.type = type;
    }

    public void OnDown(Vector2 pos)
    {
        if (!controller.IsInsideCanvas(pos)) return; // проверка границ

        Color color = type switch
        {
            Tool.Finish => Color.green,
            Tool.FakeFinish => Color.red,
            Tool.BallSpawn => Color.blue,
            _ => Color.black
        };

        if (type == Tool.BallSpawn)
        {
            var newStroke = new DrawnStroke
            {
                tool = Tool.BallSpawn,
                color = color,
                size = controller.brushSize,
                points = new Vector2[] { pos }
            };

            repo.ExecuteAction(new BrushStrokeAction(newStroke)); // используем обычное действие
        }
        else
        {
            var stroke = new DrawnStroke
            {
                tool = type,
                color = color,
                size = controller.brushSize,
                points = new Vector2[] { pos }
            };

            repo.ExecuteAction(new BrushStrokeAction(stroke));
        }

        // Рисуем точку сразу
        controller.drawer.DrawCircle(controller.ToTexture(pos), color, controller.brushSize * 2);
        controller.drawer.Apply();
    }

    public void OnDrag(Vector2 pos) { }
    public void OnUp(Vector2 pos) { }
}