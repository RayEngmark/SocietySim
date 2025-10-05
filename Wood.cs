using Raylib_cs;
using System.Numerics;

namespace SocietySim;

public class Wood
{
    public Vector2 Position { get; private set; }
    public bool IsConsumed { get; private set; } = false;
    public float RegenerationTimer { get; private set; } = 0f;
    public int HarvestCount { get; private set; } = 0; // Track how many times harvested

    private const float BaseRegenerationTime = 45f; // 45 seconds base
    private const float MaxRegenerationTime = 180f; // 3 minutes max
    private const int WoodAmount = 5; // How much wood you get per harvest

    public Wood(Vector2 position)
    {
        Position = position;
    }

    public void Update(float deltaTime)
    {
        if (IsConsumed)
        {
            RegenerationTimer += deltaTime;

            // Regeneration time increases with harvest pressure
            float requiredTime = Math.Min(BaseRegenerationTime + (HarvestCount * 10f), MaxRegenerationTime);

            if (RegenerationTimer >= requiredTime)
            {
                IsConsumed = false;
                RegenerationTimer = 0f;
            }
        }
    }

    public int Harvest()
    {
        if (!IsConsumed)
        {
            IsConsumed = true;
            HarvestCount++;
            return WoodAmount;
        }
        return 0;
    }

    public void Render()
    {
        if (IsConsumed)
        {
            // Draw tree stump
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, 8, new Color(101, 67, 33, 255));
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, 6, new Color(139, 90, 43, 255));
        }
        else
        {
            // Draw full tree
            // Trunk
            Raylib.DrawRectangle((int)Position.X - 4, (int)Position.Y - 10, 8, 20, new Color(101, 67, 33, 255));

            // Canopy (3 circles)
            Raylib.DrawCircle((int)Position.X, (int)Position.Y - 20, 15, new Color(34, 139, 34, 255));
            Raylib.DrawCircle((int)Position.X - 10, (int)Position.Y - 15, 12, new Color(34, 139, 34, 255));
            Raylib.DrawCircle((int)Position.X + 10, (int)Position.Y - 15, 12, new Color(34, 139, 34, 255));
        }
    }
}
