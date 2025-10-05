using Raylib_cs;
using System.Numerics;

namespace SocietySim;

enum FeatureType { Rock, Tree, GrassPatch, DirtPatch }

class TerrainFeature
{
    private Vector2 position;
    private FeatureType type;
    private float size;
    private Random random = new();

    public TerrainFeature(Vector2 position, FeatureType type, float size)
    {
        this.position = position;
        this.type = type;
        this.size = size;
    }

    public void Render()
    {
        switch (type)
        {
            case FeatureType.Rock:
                RenderRock();
                break;
            case FeatureType.Tree:
                RenderTree();
                break;
            case FeatureType.GrassPatch:
                RenderGrassPatch();
                break;
            case FeatureType.DirtPatch:
                RenderDirtPatch();
                break;
        }
    }

    private void RenderRock()
    {
        // Shadow
        Raylib.DrawEllipse((int)(position.X + 2), (int)(position.Y + 2), size * 1.2f, size * 0.6f, new Color(0, 0, 0, 40));

        // Base rock (gray)
        Color rockBase = new Color(120, 120, 130, 255);
        Raylib.DrawCircleV(position, size, rockBase);

        // Highlight
        Color rockHighlight = new Color(150, 150, 160, 255);
        Raylib.DrawCircleV(position + new Vector2(-size * 0.2f, -size * 0.2f), size * 0.6f, rockHighlight);

        // Dark spots
        Color rockDark = new Color(90, 90, 95, 255);
        Raylib.DrawCircleV(position + new Vector2(size * 0.3f, size * 0.1f), size * 0.3f, rockDark);
    }

    private void RenderTree()
    {
        // Shadow
        Raylib.DrawEllipse((int)(position.X + 3), (int)(position.Y + size * 1.5f + 2), size * 1.5f, size * 0.4f, new Color(0, 0, 0, 50));

        // Trunk (brown)
        Color trunkColor = new Color(101, 67, 33, 255);
        Raylib.DrawRectangle(
            (int)(position.X - size * 0.15f),
            (int)(position.Y),
            (int)(size * 0.3f),
            (int)(size * 1.5f),
            trunkColor
        );

        // Foliage (multiple circles for bushy look)
        Color foliageBase = new Color(34, 139, 34, 255); // Forest green
        Color foliageLight = new Color(50, 160, 50, 255);

        Raylib.DrawCircleV(position + new Vector2(-size * 0.4f, -size * 0.3f), size * 0.6f, foliageBase);
        Raylib.DrawCircleV(position + new Vector2(size * 0.4f, -size * 0.2f), size * 0.6f, foliageBase);
        Raylib.DrawCircleV(position + new Vector2(0, -size * 0.7f), size * 0.7f, foliageLight);
        Raylib.DrawCircleV(position + new Vector2(0, -size * 0.1f), size * 0.8f, foliageBase);
    }

    private void RenderGrassPatch()
    {
        // Slightly darker green grass patch
        Color grassDark = new Color(60, 120, 60, 100);
        Raylib.DrawCircleV(position, size, grassDark);
    }

    private void RenderDirtPatch()
    {
        // Brown dirt patch
        Color dirtColor = new Color(139, 107, 47, 120);
        Raylib.DrawCircleV(position, size, dirtColor);
    }

    public Vector2 Position => position;
}
