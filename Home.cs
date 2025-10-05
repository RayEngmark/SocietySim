using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class Home
{
    private Vector2 position;
    private Agent? owner;
    private float size = 50f; // Much bigger - actual buildings!

    // Resource storage at home
    private float storedFood = 0f;
    private float storedWater = 0f;
    private const float MaxStorage = 200f;

    // Production system - homes produce resources over time based on owner specialization
    private float productionTimer = 0f;
    private const float ProductionInterval = 10f; // Produce every 10 seconds
    private const float ProductionAmount = 15f; // Amount produced each interval

    public Home(Vector2 position, Agent owner)
    {
        this.position = position;
        this.owner = owner;
    }

    public void Update(float deltaTime)
    {
        if (owner == null) return;

        productionTimer += deltaTime;

        if (productionTimer >= ProductionInterval)
        {
            productionTimer = 0f;

            // Produce resources based on specialization
            switch (owner.AgentSpecialization)
            {
                case Agent.Specialization.FoodGatherer:
                    // Farmers grow food at home
                    if (storedFood < MaxStorage)
                    {
                        storedFood = Math.Min(storedFood + ProductionAmount, MaxStorage);
                    }
                    break;

                case Agent.Specialization.WaterGatherer:
                    // Well diggers collect water at home
                    if (storedWater < MaxStorage)
                    {
                        storedWater = Math.Min(storedWater + ProductionAmount, MaxStorage);
                    }
                    break;

                case Agent.Specialization.Generalist:
                    // Generalists produce both, but slower
                    if (storedFood < MaxStorage)
                    {
                        storedFood = Math.Min(storedFood + ProductionAmount * 0.5f, MaxStorage);
                    }
                    if (storedWater < MaxStorage)
                    {
                        storedWater = Math.Min(storedWater + ProductionAmount * 0.5f, MaxStorage);
                    }
                    break;
            }
        }
    }

    public bool CanStore(float food, float water)
    {
        return storedFood + food <= MaxStorage && storedWater + water <= MaxStorage;
    }

    public void StoreResources(float food, float water)
    {
        storedFood = Math.Min(storedFood + food, MaxStorage);
        storedWater = Math.Min(storedWater + water, MaxStorage);
    }

    public void WithdrawResources(out float food, out float water, float requestedFood, float requestedWater)
    {
        food = Math.Min(requestedFood, storedFood);
        water = Math.Min(requestedWater, storedWater);
        storedFood -= food;
        storedWater -= water;
    }

    public void Render()
    {
        if (owner == null) return;

        // Large shadow underneath
        Raylib.DrawEllipse((int)(position.X + 4), (int)(position.Y + size * 0.6f), size * 0.8f, size * 0.3f, new Color(0, 0, 0, 60));

        // Determine base colors by specialization
        Color wallColor, roofColor, accentColor;
        switch (owner.AgentSpecialization)
        {
            case Agent.Specialization.FoodGatherer:
                wallColor = new Color(139, 90, 43, 255); // Wooden hut
                roofColor = new Color(101, 67, 33, 255); // Dark brown thatch
                accentColor = new Color(100, 200, 100, 255); // Green accent
                break;
            case Agent.Specialization.WaterGatherer:
                wallColor = new Color(120, 120, 130, 255); // Stone cottage
                roofColor = new Color(70, 90, 110, 255); // Dark gray tiles
                accentColor = new Color(100, 150, 255, 255); // Blue accent
                break;
            default:
                wallColor = new Color(180, 140, 100, 255); // Clay dwelling
                roofColor = new Color(160, 82, 45, 255); // Reddish thatch
                accentColor = new Color(200, 200, 100, 255); // Yellow accent
                break;
        }

        // Foundation/base (darker)
        Color foundationColor = new Color(
            (int)(wallColor.R * 0.7f),
            (int)(wallColor.G * 0.7f),
            (int)(wallColor.B * 0.7f),
            255
        );
        Raylib.DrawRectangleV(
            position - new Vector2(size * 0.5f, size * 0.5f) + new Vector2(0, size * 0.7f),
            new Vector2(size, size * 0.15f),
            foundationColor
        );

        // Main wall structure (rounded corners for natural look)
        Vector2 wallPos = position - new Vector2(size * 0.45f, size * 0.45f);
        float wallSize = size * 0.9f;
        Raylib.DrawRectangleRounded(
            new Rectangle(wallPos.X, wallPos.Y, wallSize, wallSize * 0.8f),
            0.1f,
            8,
            wallColor
        );

        // Wall texture (vertical planks/stones)
        for (int i = 0; i < 5; i++)
        {
            float x = wallPos.X + (wallSize / 5f) * i;
            Raylib.DrawLineEx(
                new Vector2(x, wallPos.Y),
                new Vector2(x, wallPos.Y + wallSize * 0.8f),
                1.5f,
                new Color(0, 0, 0, 30)
            );
        }

        // Large thatched roof (overlapping)
        Vector2 roofTop = position + new Vector2(0, -size * 0.8f);
        Vector2 roofLeft = position + new Vector2(-size * 0.6f, -size * 0.3f);
        Vector2 roofRight = position + new Vector2(size * 0.6f, -size * 0.3f);

        // Roof back layer (darker)
        Color darkRoof = new Color(
            (int)(roofColor.R * 0.8f),
            (int)(roofColor.G * 0.8f),
            (int)(roofColor.B * 0.8f),
            255
        );
        Raylib.DrawTriangle(roofTop, roofRight, roofLeft, darkRoof);

        // Roof front layer
        Vector2 roofFrontTop = roofTop + new Vector2(0, 3);
        Raylib.DrawTriangle(roofFrontTop, roofRight, roofLeft, roofColor);

        // Roof thatch lines for texture
        for (int i = 0; i < 6; i++)
        {
            float ratio = i / 6f;
            Vector2 leftPoint = Vector2.Lerp(roofLeft, roofFrontTop, ratio);
            Vector2 rightPoint = Vector2.Lerp(roofRight, roofFrontTop, ratio);
            Raylib.DrawLineEx(leftPoint, rightPoint, 1.5f, new Color(0, 0, 0, 40));
        }

        // Wooden door (larger and more detailed)
        Color doorColor = new Color(80, 50, 30, 255);
        Vector2 doorPos = position + new Vector2(-size * 0.12f, size * 0.15f);
        float doorWidth = size * 0.24f;
        float doorHeight = size * 0.35f;
        Raylib.DrawRectangleRounded(
            new Rectangle(doorPos.X, doorPos.Y, doorWidth, doorHeight),
            0.15f,
            8,
            doorColor
        );

        // Door planks
        Raylib.DrawLineEx(
            doorPos + new Vector2(doorWidth / 2, 0),
            doorPos + new Vector2(doorWidth / 2, doorHeight),
            2f,
            new Color(60, 40, 20, 255)
        );

        // Windows (with frames)
        Color windowFrame = new Color(60, 40, 20, 255);
        Color windowGlass = new Color(180, 220, 255, 160);

        // Left window
        Vector2 leftWin = position + new Vector2(-size * 0.3f, -size * 0.05f);
        Raylib.DrawRectangle((int)leftWin.X - 1, (int)leftWin.Y - 1, (int)(size * 0.15f) + 2, (int)(size * 0.15f) + 2, windowFrame);
        Raylib.DrawRectangle((int)leftWin.X, (int)leftWin.Y, (int)(size * 0.15f), (int)(size * 0.15f), windowGlass);
        Raylib.DrawLine((int)(leftWin.X + size * 0.075f), (int)leftWin.Y, (int)(leftWin.X + size * 0.075f), (int)(leftWin.Y + size * 0.15f), windowFrame);

        // Right window
        Vector2 rightWin = position + new Vector2(size * 0.15f, -size * 0.05f);
        Raylib.DrawRectangle((int)rightWin.X - 1, (int)rightWin.Y - 1, (int)(size * 0.15f) + 2, (int)(size * 0.15f) + 2, windowFrame);
        Raylib.DrawRectangle((int)rightWin.X, (int)rightWin.Y, (int)(size * 0.15f), (int)(size * 0.15f), windowGlass);
        Raylib.DrawLine((int)(rightWin.X + size * 0.075f), (int)rightWin.Y, (int)(rightWin.X + size * 0.075f), (int)(rightWin.Y + size * 0.15f), windowFrame);

        // Chimney (small stack on roof)
        Vector2 chimneyPos = position + new Vector2(size * 0.25f, -size * 0.6f);
        Raylib.DrawRectangle((int)chimneyPos.X, (int)chimneyPos.Y, (int)(size * 0.12f), (int)(size * 0.25f), new Color(100, 60, 60, 255));

        // Smoke (if active)
        for (int i = 0; i < 3; i++)
        {
            float offset = i * 8f;
            Raylib.DrawCircleV(chimneyPos + new Vector2(size * 0.06f, -offset - 10), 3 + i, new Color(200, 200, 200, 100 - i * 30));
        }

        // Storage indicators (larger bars below house)
        float barWidth = size * 0.8f;
        float barHeight = 4f;
        Vector2 storagePos = position + new Vector2(-barWidth / 2, size * 0.5f);

        // Food bar
        float foodRatio = storedFood / MaxStorage;
        Raylib.DrawRectangleV(storagePos, new Vector2(barWidth, barHeight), new Color(40, 40, 40, 200));
        Raylib.DrawRectangleV(storagePos, new Vector2(barWidth * foodRatio, barHeight), accentColor);

        // Water bar
        Vector2 waterBarPos = storagePos + new Vector2(0, barHeight + 3);
        float waterRatio = storedWater / MaxStorage;
        Raylib.DrawRectangleV(waterBarPos, new Vector2(barWidth, barHeight), new Color(40, 40, 40, 200));
        byte waterAlpha = 200;
        Color waterBarColor = new Color(accentColor.R, accentColor.G, accentColor.B, waterAlpha);
        Raylib.DrawRectangleV(waterBarPos, new Vector2(barWidth * waterRatio, barHeight), waterBarColor);
    }

    public Vector2 Position => position;
    public Agent? Owner => owner;
    public bool HasOwner => owner != null;
    public float StoredFood => storedFood;
    public float StoredWater => storedWater;
}
