using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class Program
{
    static void Main(string[] args)
    {
        // Get monitor size for borderless fullscreen
        int monitorWidth = Raylib.GetMonitorWidth(0);
        int monitorHeight = Raylib.GetMonitorHeight(0);

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.BorderlessWindowMode);
        Raylib.InitWindow(monitorWidth, monitorHeight, "Society Simulation");
        Raylib.SetTargetFPS(60);

        var world = new World(monitorWidth, monitorHeight);

        while (!Raylib.WindowShouldClose())
        {
            // Handle window resize
            if (Raylib.IsWindowResized())
            {
                int newWidth = Raylib.GetScreenWidth();
                int newHeight = Raylib.GetScreenHeight();
                world.Resize(newWidth, newHeight);
            }

            // Update
            world.Update(Raylib.GetFrameTime());

            // Draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            world.Render();

            Raylib.DrawFPS(10, 10);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
