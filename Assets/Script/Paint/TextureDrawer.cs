using UnityEngine;

public class TextureDrawer
{
    private Texture2D texture;

    public TextureDrawer(Texture2D tex)
    {
        texture = tex;
    }

    public void Clear(Color color)
    {
        Color[] fill = new Color[texture.width * texture.height];
        for (int i = 0; i < fill.Length; i++) fill[i] = color;
        texture.SetPixels(fill);
    }

    public void Apply()
    {
        texture.Apply();
    }

    public void DrawCircle(Vector2 pos, Color color, int size)
    {
        int cx = (int)pos.x;
        int cy = (int)pos.y;

        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (x * x + y * y <= size * size)
                {
                    int px = cx + x;
                    int py = cy + y;

                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        texture.SetPixel(px, py, color);
                }
            }
        }
    }

    public void DrawLine(Vector2 from, Vector2 to, Color color, int size)
    {
        int x0 = (int)from.x;
        int y0 = (int)from.y;
        int x1 = (int)to.x;
        int y1 = (int)to.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            DrawCircle(new Vector2(x0, y0), color, size);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}