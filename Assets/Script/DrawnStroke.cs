using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DrawnStroke
{
    public Tool tool;
    public Color color;
    public int size;
    public Vector2[] points;
}

[Serializable]
public class StrokeListWrapper
{
    public List<DrawnStroke> strokes;
    public int canvasShape;
}

// ===== Undo/Redo Actions =====
public abstract class StrokeAction
{
    public abstract void Undo(StrokeRepository repo);
    public abstract void Redo(StrokeRepository repo);
}

public class BrushStrokeAction : StrokeAction
{
    private DrawnStroke stroke;
    public BrushStrokeAction(DrawnStroke s) { stroke = s; }

    public override void Undo(StrokeRepository repo) => repo.strokes.Remove(stroke);
    public override void Redo(StrokeRepository repo) => repo.strokes.Add(stroke);
}

public class BallSpawnAction : StrokeAction
{
    private DrawnStroke newStroke;
    private DrawnStroke oldStroke;

    public BallSpawnAction(DrawnStroke newS, DrawnStroke oldS)
    {
        newStroke = newS;
        oldStroke = oldS;
    }

    public override void Undo(StrokeRepository repo)
    {
        repo.strokes.Remove(newStroke);
        if (oldStroke != null) repo.strokes.Add(oldStroke);
    }

    public override void Redo(StrokeRepository repo)
    {
        if (oldStroke != null) repo.strokes.Remove(oldStroke);
        repo.strokes.Add(newStroke);
    }
}

// ===== Stroke Repository =====
public class StrokeRepository
{
    public List<DrawnStroke> strokes = new List<DrawnStroke>();
    private Stack<StrokeAction> undoStack = new Stack<StrokeAction>();
    private Stack<StrokeAction> redoStack = new Stack<StrokeAction>();

    public void ExecuteAction(StrokeAction action)
    {
        action.Redo(this);
        undoStack.Push(action);
        redoStack.Clear();
    }

    public bool Undo()
    {
        if (undoStack.Count == 0) return false;
        var action = undoStack.Pop();
        action.Undo(this);
        redoStack.Push(action);
        return true;
    }

    public bool Redo()
    {
        if (redoStack.Count == 0) return false;
        var action = redoStack.Pop();
        action.Redo(this);
        undoStack.Push(action);
        return true;
    }

    public void Clear()
    {
        strokes.Clear();
        undoStack.Clear();
        redoStack.Clear();
    }
}