using System.Collections.Generic;
using UnityEngine;

public interface IDrawTool
{
    void OnDown(Vector2 pos);
    void OnDrag(Vector2 pos);
    void OnUp(Vector2 pos);
}

