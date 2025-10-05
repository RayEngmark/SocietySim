using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class Water
{
    private Vector2 position;
    private float hydrationValue = 40f;
    private float size = 7f; // Bigger
    private bool consumed = false;

    // Regeneration system
    private float regenerationTimer = 0f;
    private const float RegenerationTime = 20f; // 20 seconds to refill (faster than food)
    private float harvestPressure = 0f; // Tracks how heavily this source is used
    private const float PressureDecay = 0.15f; // Water recovers slightly faster

    public Water(Vector2 position)
    {
        this.position = position;
    }

    public void Consume()
    {
        consumed = true;
        regenerationTimer = 0f;
        harvestPressure += 1f;
    }

    public void Update(float deltaTime)
    {
        // Regenerate if consumed
        if (consumed)
        {
            regenerationTimer += deltaTime;

            // Regeneration time increases with harvest pressure
            float effectiveRegenTime = RegenerationTime * (1f + harvestPressure * 0.3f);

            if (regenerationTimer >= effectiveRegenTime)
            {
                consumed = false;
                regenerationTimer = 0f;
            }
        }

        // Harvest pressure naturally decays over time
        harvestPressure = Math.Max(0f, harvestPressure - PressureDecay * deltaTime);
    }

    public void Render()
    {
        // Always show the water source, but appearance changes based on state
        float regenProgress = consumed ? (regenerationTimer / (RegenerationTime * (1f + harvestPressure * 0.3f))) : 1f;

        // Size and opacity change based on water level
        float currentSize = size * (0.4f + regenProgress * 0.6f);
        int alpha = (int)(255 * (0.5f + regenProgress * 0.5f));

        // Water color changes with pressure - muddier when overused
        int blueValue = (int)(150 - harvestPressure * 20);
        Color waterDark = new Color(30, 90, Math.Clamp(blueValue, 80, 150), (byte)alpha);
        Raylib.DrawEllipse((int)position.X, (int)position.Y, currentSize * 1.2f, currentSize * 0.9f, waterDark);

        if (!consumed)
        {
            // Inner lighter water (highlight)
            Color waterLight = new Color(70, 130, 200, 200);
            Raylib.DrawEllipse((int)position.X - 1, (int)position.Y - 1, currentSize * 0.8f, currentSize * 0.6f, waterLight);

            // Shine/reflection spot (white highlight)
            Color shine = new Color(200, 230, 255, 180);
            Raylib.DrawCircleV(position + new Vector2(-2, -2), currentSize * 0.25f, shine);
        }

        // Edge outline for definition
        Raylib.DrawEllipseLines((int)position.X, (int)position.Y, currentSize * 1.2f, currentSize * 0.9f, new Color(20, 60, 100, 150));
    }

    public Vector2 Position => position;
    public float HydrationValue => hydrationValue;
    public bool IsConsumed => consumed;
}
