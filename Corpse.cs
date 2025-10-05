using Raylib_cs;
using System.Numerics;

namespace SocietySim;

public class Corpse
{
    public Vector2 Position { get; private set; }
    public float RottenTime { get; private set; } = 0f;
    public string CauseDeath { get; private set; }
    public bool IsBuried { get; private set; } = false;

    private const float TimeToRot = 120f; // 2 minutes to fully rot
    private const float MoodRadius = 150f; // Affects mood within this radius

    public Corpse(Vector2 position, string cause)
    {
        Position = position;
        CauseDeath = cause;
    }

    public void Update(float deltaTime)
    {
        if (!IsBuried)
        {
            RottenTime += deltaTime;
        }
    }

    public float GetRotLevel()
    {
        return Math.Clamp(RottenTime / TimeToRot, 0f, 1f);
    }

    public float GetMoodPenalty()
    {
        // Worse penalty the more rotten
        return -0.3f * GetRotLevel();
    }

    public void Bury()
    {
        IsBuried = true;
    }

    public void Render()
    {
        if (IsBuried) return;

        // Draw corpse as dark gray/black body
        Color corpseColor = new Color(40, 30, 30, 255);

        // Add green tint as it rots
        float rotLevel = GetRotLevel();
        if (rotLevel > 0.3f)
        {
            corpseColor = new Color(40, (int)(30 + rotLevel * 60), 30, 255);
        }

        // Draw body (oval)
        Raylib.DrawEllipse((int)Position.X, (int)Position.Y, 12, 8, corpseColor);

        // Draw blood pool if recently dead
        if (RottenTime < 30f)
        {
            int bloodAlpha = (int)((1f - RottenTime / 30f) * 150);
            Raylib.DrawCircle((int)Position.X, (int)Position.Y, 15, new Color(120, 0, 0, bloodAlpha));
        }

        // Flies buzzing around rotten corpses
        if (rotLevel > 0.5f)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = (RottenTime * 3f + i * 2f) % (MathF.PI * 2);
                float radius = 20f;
                int fx = (int)(Position.X + MathF.Cos(angle) * radius);
                int fy = (int)(Position.Y + MathF.Sin(angle) * radius);
                Raylib.DrawCircle(fx, fy, 2, new Color(50, 50, 50, 200));
            }
        }
    }

    public bool AffectsMood(Vector2 agentPos)
    {
        return !IsBuried && Vector2.Distance(Position, agentPos) < MoodRadius;
    }
}
