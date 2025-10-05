using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class Particle
{
    private Vector2 position;
    private Vector2 velocity;
    private float lifetime;
    private float maxLifetime;
    private Color color;
    private float size;

    public Particle(Vector2 position, Vector2 velocity, Color color, float lifetime, float size = 2f)
    {
        this.position = position;
        this.velocity = velocity;
        this.color = color;
        this.lifetime = lifetime;
        this.maxLifetime = lifetime;
        this.size = size;
    }

    public void Update(float deltaTime)
    {
        position += velocity * deltaTime;
        lifetime -= deltaTime;

        // Slow down over time
        velocity *= 0.95f;
    }

    public void Render()
    {
        float alpha = lifetime / maxLifetime;
        Color renderColor = new Color(
            color.R,
            color.G,
            color.B,
            (int)(color.A * alpha)
        );

        float renderSize = size * (0.5f + alpha * 0.5f);
        Raylib.DrawCircleV(position, renderSize, renderColor);
    }

    public bool IsDead => lifetime <= 0;
}
