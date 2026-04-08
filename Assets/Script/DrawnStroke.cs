using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DrawnStroke
{
    public Tool tool;
    public Color color;
    public int size;
    public Vector2[] points;
}

[System.Serializable]
public class StrokeListWrapper
{
    public List<DrawnStroke> strokes;
    public int canvasShape;
}