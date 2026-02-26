using UnityEngine;

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
    public System.Collections.Generic.List<DrawnStroke> strokes;
}
