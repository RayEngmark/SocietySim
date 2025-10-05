using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class Food
{
    private Vector2 position;
    private float energyValue = 50f;
    private float size = 7f; // Bigger
    private bool consumed = false;

    // Regeneration system
    private float regenerationTimer = 0f;
    private const float RegenerationTime = 30f; // 30 seconds to regrow
    private float harvestPressure = 0f; // Tracks how heavily this area is harvested
    private const float PressureDecay = 0.1f; // Pressure decreases over time

    public Food(Vector2 position)
    {
        this.position = position;
    }

    public void Consume()
    {
        consumed = true;
        regenerationTimer = 0f;
        harvestPressure += 1f; // Each harvest increases pressure
    }

    public void Update(float deltaTime)
    {
        // Regenerate if consumed
        if (consumed)
        {
            regenerationTimer += deltaTime;

            // Regeneration time increases with harvest pressure (overharvesting slows regrowth)
            float effectiveRegenTime = RegenerationTime * (1f + harvestPressure * 0.5f);

            if (regenerationTimer >= effectiveRegenTime)
            {
                consumed = false;
                regenerationTimer = 0f;
            }
        }

        // Harvest pressure naturally decays over time (land recovers if left alone)
        harvestPressure = Math.Max(0f, harvestPressure - PressureDecay * deltaTime);
    }

    public void Render()
    {
        // Always show the bush, but appearance changes based on state
        float regenProgress = consumed ? (regenerationTimer / (RegenerationTime * (1f + harvestPressure * 0.5f))) : 1f;

        // Shadow
        Raylib.DrawEllipse((int)position.X + 1, (int)position.Y + 1, size * 1.2f, size * 0.5f, new Color(0, 0, 0, 50));

        // Bush leaves - color changes based on health/harvest pressure
        // High pressure = brownish, low pressure = vibrant green
        int greenValue = (int)(139 - harvestPressure * 30);
        Color bushColor = new Color(34, Math.Clamp(greenValue, 60, 139), 34, 255);

        float bushSize = size * (0.5f + regenProgress * 0.5f); // Smaller when consumed
        Raylib.DrawCircleV(position + new Vector2(-3, 0), bushSize * 0.6f, bushColor);
        Raylib.DrawCircleV(position + new Vector2(3, 0), bushSize * 0.6f, bushColor);
        Raylib.DrawCircleV(position + new Vector2(0, -2), bushSize * 0.7f, bushColor);
        Raylib.DrawCircleV(position, bushSize * 0.8f, bushColor);

        // Berries only appear when fully regenerated
        if (!consumed)
        {
            Color berryColor = new Color(178, 34, 34, 255); // Firebrick red
            Raylib.DrawCircleV(position + new Vector2(-2, -1), 1.5f, berryColor);
            Raylib.DrawCircleV(position + new Vector2(2, 0), 1.5f, berryColor);
            Raylib.DrawCircleV(position + new Vector2(0, 2), 1.5f, berryColor);
            Raylib.DrawCircleV(position + new Vector2(1, -2), 1.5f, berryColor);
        }
    }

    public Vector2 Position => position;
    public float EnergyValue => energyValue;
    public bool IsConsumed => consumed;
}
