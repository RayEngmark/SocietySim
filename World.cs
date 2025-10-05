using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class World
{
    private List<Agent> agents = new();
    private List<Food> foodSources = new();
    private List<Water> waterSources = new();
    private List<Home> homes = new();
    private List<Corpse> corpses = new();
    private List<Wood> woodSources = new();
    private List<Particle> particles = new();
    private List<TerrainFeature> terrainFeatures = new();
    private Random random = new();

    // TIME SYSTEM - Day/Night cycle and seasons
    private float worldTime = 0f; // Time in seconds
    private const float DayLength = 120f; // 2 minute days
    private const float YearLength = DayLength * 12f; // 12 days per year
    private int currentDay = 1;
    private int currentYear = 1;

    // Path tracking - stores intensity of foot traffic at tile coordinates
    private Dictionary<(int, int), float> pathIntensity = new();
    // Potential roads - tiles with enough traffic but not yet connected
    private HashSet<(int, int)> potentialRoads = new();
    // Permanent roads - tiles that have become established paths (connected)
    private HashSet<(int, int)> permanentRoads = new();
    // Track agent visits per tile for road formation (agent ID -> last visit time)
    private Dictionary<(int, int), Dictionary<int, float>> tileVisitors = new();
    private int width;
    private int height;
    // Old spawning system removed - resources now regenerate!
    private const float maxEnergy = 100f;
    private Color[,] terrainColors = new Color[0,0];


    // World dimensions - fixed large playable area
    private const int WorldWidth = 8000;
    private const int WorldHeight = 5600;

    // Camera
    private float cameraX = 0;
    private float cameraY = 0;
    private float cameraZoom = 1.0f;
    private float targetZoom = 1.0f;

    public World(int width, int height)
    {
        this.width = width;
        this.height = height;

        // Generate terrain
        GenerateTerrain();

        // Spawn some initial agents in center
        for (int i = 0; i < 30; i++)
        {
            agents.Add(new Agent(
                GetCenterWorldPosition(),
                this
            ));
        }

        // Spawn food in natural clusters (berry groves)
        SpawnFoodClusters(100); // 100 groves of 5-10 berries each

        // Spawn water in clusters (ponds)
        SpawnWaterClusters(80); // 80 ponds with 3-8 collection points each

        // Spawn wood sources in forest clusters
        SpawnWoodSources();

        // Spawn decorative terrain features
        SpawnTerrainFeatures();
    }

    private Vector2 GetRandomWorldPosition()
    {
        // Spawn anywhere in the world with some margin from edges
        int margin = 200;
        float centerX = width / 2f;
        float centerY = height / 2f;

        float worldLeft = centerX - WorldWidth / 2f + margin;
        float worldRight = centerX + WorldWidth / 2f - margin;
        float worldTop = centerY - WorldHeight / 2f + margin;
        float worldBottom = centerY + WorldHeight / 2f - margin;

        return new Vector2(
            (float)(random.NextDouble() * (worldRight - worldLeft) + worldLeft),
            (float)(random.NextDouble() * (worldBottom - worldTop) + worldTop)
        );
    }

    private Vector2 GetCenterWorldPosition()
    {
        // Spawn in center with some random offset
        float centerX = width / 2f;
        float centerY = height / 2f;

        float offsetRange = 200f;
        return new Vector2(
            centerX + (float)(random.NextDouble() - 0.5) * offsetRange,
            centerY + (float)(random.NextDouble() - 0.5) * offsetRange
        );
    }

    private void SpawnFoodClusters(int clusterCount)
    {
        for (int i = 0; i < clusterCount; i++)
        {
            // Pick random center for cluster
            Vector2 clusterCenter = GetRandomWorldPosition();

            // Spawn 5-10 food items in cluster
            int itemsInCluster = random.Next(5, 11);
            for (int j = 0; j < itemsInCluster; j++)
            {
                // Offset from cluster center (20-60 units)
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = (float)(random.NextDouble() * 40 + 20);
                Vector2 offset = new Vector2(
                    MathF.Cos(angle) * distance,
                    MathF.Sin(angle) * distance
                );
                foodSources.Add(new Food(clusterCenter + offset));
            }
        }
    }

    private void SpawnWaterClusters(int clusterCount)
    {
        for (int i = 0; i < clusterCount; i++)
        {
            // Pick random center for cluster (pond)
            Vector2 clusterCenter = GetRandomWorldPosition();

            // Spawn 3-8 water collection points in cluster
            int itemsInCluster = random.Next(3, 9);
            for (int j = 0; j < itemsInCluster; j++)
            {
                // Offset from cluster center (10-40 units for tighter ponds)
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = (float)(random.NextDouble() * 30 + 10);
                Vector2 offset = new Vector2(
                    MathF.Cos(angle) * distance,
                    MathF.Sin(angle) * distance
                );
                waterSources.Add(new Water(clusterCenter + offset));
            }
        }
    }

    private void SpawnWoodSources()
    {
        // Spawn wood in small forest clusters
        for (int i = 0; i < 15; i++)
        {
            Vector2 forestCenter = GetRandomWorldPosition();
            int treesInForest = random.Next(5, 12);

            for (int j = 0; j < treesInForest; j++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = (float)(random.NextDouble() * 60 + 20);
                Vector2 offset = new Vector2(
                    MathF.Cos(angle) * distance,
                    MathF.Sin(angle) * distance
                );
                woodSources.Add(new Wood(forestCenter + offset));
            }
        }
    }

    private void SpawnTerrainFeatures()
    {
        // Spawn rocks (scattered)
        for (int i = 0; i < 150; i++)
        {
            Vector2 pos = GetRandomWorldPosition();
            float size = (float)(random.NextDouble() * 8 + 6); // 6-14 units
            terrainFeatures.Add(new TerrainFeature(pos, FeatureType.Rock, size));
        }

        // Spawn trees in clusters (forests)
        int forestCount = 40;
        for (int i = 0; i < forestCount; i++)
        {
            Vector2 forestCenter = GetRandomWorldPosition();
            int treeCount = random.Next(5, 15);

            for (int j = 0; j < treeCount; j++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = (float)(random.NextDouble() * 80 + 20);
                Vector2 offset = new Vector2(
                    MathF.Cos(angle) * distance,
                    MathF.Sin(angle) * distance
                );
                float size = (float)(random.NextDouble() * 10 + 15); // 15-25 units
                terrainFeatures.Add(new TerrainFeature(forestCenter + offset, FeatureType.Tree, size));
            }
        }

        // Spawn ground variation patches
        for (int i = 0; i < 200; i++)
        {
            Vector2 pos = GetRandomWorldPosition();
            float size = (float)(random.NextDouble() * 30 + 20); // 20-50 units
            FeatureType type = random.NextDouble() > 0.5 ? FeatureType.GrassPatch : FeatureType.DirtPatch;
            terrainFeatures.Add(new TerrainFeature(pos, type, size));
        }
    }

    public void Update(float deltaTime)
    {
        // Handle camera controls
        float moveSpeed = 500f / cameraZoom;

        if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))
            cameraY -= moveSpeed * deltaTime;
        if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))
            cameraY += moveSpeed * deltaTime;
        if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
            cameraX -= moveSpeed * deltaTime;
        if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
            cameraX += moveSpeed * deltaTime;

        // Zoom with mouse wheel
        float wheelMove = Raylib.GetMouseWheelMove();
        if (wheelMove != 0)
        {
            targetZoom += wheelMove * 0.1f;
            targetZoom = Math.Clamp(targetZoom, 0.2f, 3.0f);
        }

        // Smooth zoom interpolation
        cameraZoom += (targetZoom - cameraZoom) * 10f * deltaTime;

        // TIME SYSTEM UPDATE - Track world time and days
        worldTime += deltaTime;
        currentDay = (int)(worldTime / DayLength) + 1;
        currentYear = (int)(worldTime / YearLength) + 1;

        // DEATH HANDLING - Create corpses and remove dead agents
        foreach (var deadAgent in agents.Where(a => a.IsDead).ToList())
        {
            // Create corpse at agent's position
            corpses.Add(new Corpse(deadAgent.Position, deadAgent.DeathCause));
        }
        agents.RemoveAll(a => a.IsDead);

        foreach (var agent in agents.ToList()) // Use ToList to avoid modification during iteration
        {
            agent.Update(deltaTime);

            // Social interactions - trading and sharing
            foreach (var other in agents)
            {
                if (other != agent)
                {
                    float distance = Vector2.Distance(agent.Position, other.Position);
                    if (distance < 50f) // Within interaction range
                    {
                        // Try trading if agent has excess resources
                        if (agent.CanTrade(out _, out _) && agent.WillHelp(other))
                        {
                            agent.Trade(other);
                        }

                        // Legacy energy sharing (backup)
                        if (agent.Energy > maxEnergy * 0.6f && other.Energy < maxEnergy * 0.4f)
                        {
                            if (agent.WillHelp(other))
                            {
                                agent.ShareEnergyWith(other, 15f);
                            }
                        }

                        break; // Only interact with one agent per frame
                    }
                }
            }

            // Handle reproduction (slow and controlled)
            if (agent.CanReproduce && agents.Count < 100) // Lower population cap
            {
                // Random chance to reproduce (not every frame when able)
                if (random.NextDouble() < 0.002) // ~0.2% chance per frame when ready
                {
                    // Birth position near parent
                    Vector2 birthPos = agent.Position + new Vector2(
                        (float)(random.NextDouble() - 0.5) * 20,
                        (float)(random.NextDouble() - 0.5) * 20
                    );

                    // Create offspring with genetic inheritance (asexual for now)
                    var offspring = agent.CreateOffspring(null, birthPos, this);
                    agents.Add(offspring);

                    // Reproduction costs resources
                    agent.ConsumeForReproduction();
                }
            }

            // Home claiming - agents prefer clustering near other homes AND resources
            if (agent.WantsHome)
            {
                // Check if location has enough distance from other homes
                const float MinHomeDistance = 80f; // Houses must be at least 80 pixels apart
                bool tooCloseToHomes = homes.Any(h => Vector2.Distance(agent.Position, h.Position) < MinHomeDistance);

                if (!tooCloseToHomes)
                {
                    // Evaluate current location quality
                    int nearbyFood = foodSources.Count(f => !f.IsConsumed && Vector2.Distance(agent.Position, f.Position) < 150f);
                    int nearbyWater = waterSources.Count(w => !w.IsConsumed && Vector2.Distance(agent.Position, w.Position) < 150f);
                    int nearbyHomes = homes.Count(h => Vector2.Distance(agent.Position, h.Position) < 200f);

                    // Calculate location score (resources + clustering bonus)
                    int resourceScore = nearbyFood + nearbyWater;
                    int clusteringBonus = nearbyHomes * 2; // Being near other homes is valuable
                    int totalScore = resourceScore + clusteringBonus;

                    // Claim home if score is good (resources OR near other homes)
                    if (totalScore > 5 || (nearbyHomes > 0 && totalScore > 3))
                    {
                        var newHome = new Home(agent.Position, agent);
                        homes.Add(newHome);
                        agent.SetHome(newHome);
                    }
                }
            }
        }

        // Remove dead agents
        agents.RemoveAll(a => a.IsDead);

        // Update resources with regeneration system
        foreach (var food in foodSources)
        {
            food.Update(deltaTime);
        }

        foreach (var water in waterSources)
        {
            water.Update(deltaTime);
        }

        // Remove old periodic spawning - resources now regenerate instead
        // Resources are no longer removed when consumed - they regenerate over time!

        // Update homes with production system
        foreach (var home in homes)
        {
            home.Update(deltaTime);
        }

        // Update corpses (rot over time)
        foreach (var corpse in corpses)
        {
            corpse.Update(deltaTime);
        }

        // Update wood sources
        foreach (var wood in woodSources)
        {
            wood.Update(deltaTime);
        }

        // Update particles
        foreach (var particle in particles.ToList())
        {
            particle.Update(deltaTime);
        }
        particles.RemoveAll(p => p.IsDead);

        // Decay paths over time
        var keysToUpdate = pathIntensity.Keys.ToList();
        foreach (var key in keysToUpdate)
        {
            pathIntensity[key] *= 0.999f; // Slow decay
            if (pathIntensity[key] < 0.01f)
            {
                pathIntensity.Remove(key);
            }
        }

    }

    public void SpawnParticle(Vector2 position, Vector2 velocity, Color color, float lifetime, float size)
    {
        particles.Add(new Particle(position, velocity, color, lifetime, size));
    }

    public void RecordFootstep(Vector2 position, int agentId, float currentTime)
    {
        int tileSize = 32;
        int tileX = (int)(position.X / tileSize);
        int tileY = (int)(position.Y / tileSize);
        var key = (tileX, tileY);

        // Skip if already a permanent road
        if (permanentRoads.Contains(key))
        {
            return;
        }

        // Track visitor for this tile
        if (!tileVisitors.ContainsKey(key))
        {
            tileVisitors[key] = new Dictionary<int, float>();
        }

        tileVisitors[key][agentId] = currentTime;

        // Clean up old visits (older than 10 seconds)
        var oldVisitors = tileVisitors[key].Where(v => currentTime - v.Value > 10f).Select(v => v.Key).ToList();
        foreach (var oldId in oldVisitors)
        {
            tileVisitors[key].Remove(oldId);
        }

        // Check if enough unique agents have visited recently (2+ agents within 10 seconds)
        if (tileVisitors[key].Count >= 2)
        {
            // Promote to potential road
            potentialRoads.Add(key);
            tileVisitors.Remove(key); // Don't need to track anymore
            pathIntensity.Remove(key); // Remove from decaying paths

            // Check if this potential road should become permanent (has connected neighbors)
            CheckAndPromoteToPermanentRoad(key);
        }
        else
        {
            // Regular path tracking for non-roads
            if (!pathIntensity.ContainsKey(key))
            {
                pathIntensity[key] = 0f;
            }
            pathIntensity[key] = Math.Min(pathIntensity[key] + 0.05f, 1f);
        }
    }

    private void CheckAndPromoteToPermanentRoad((int, int) tile)
    {
        // Check 8 adjacent tiles (including diagonals)
        var neighbors = new[]
        {
            (tile.Item1 - 1, tile.Item2), (tile.Item1 + 1, tile.Item2), // Left, Right
            (tile.Item1, tile.Item2 - 1), (tile.Item1, tile.Item2 + 1), // Up, Down
            (tile.Item1 - 1, tile.Item2 - 1), (tile.Item1 + 1, tile.Item2 - 1), // Diagonals
            (tile.Item1 - 1, tile.Item2 + 1), (tile.Item1 + 1, tile.Item2 + 1)
        };

        // Count how many neighbors are potential or permanent roads
        int connectedNeighbors = neighbors.Count(n =>
            potentialRoads.Contains(n) || permanentRoads.Contains(n)
        );

        // Need at least 2 connected neighbors to become permanent
        if (connectedNeighbors >= 2)
        {
            permanentRoads.Add(tile);
            potentialRoads.Remove(tile);

            // Recursively check neighbors that are potential roads
            foreach (var neighbor in neighbors)
            {
                if (potentialRoads.Contains(neighbor))
                {
                    CheckAndPromoteToPermanentRoad(neighbor);
                }
            }
        }
    }

    private void GenerateTerrain()
    {
        int tileSize = 32;
        // Make terrain large enough to cover the entire world
        int tilesX = (WorldWidth / tileSize) + 10;
        int tilesY = (WorldHeight / tileSize) + 10;
        terrainColors = new Color[tilesX, tilesY];

        // Create varied grass colors - simple and clean
        Color baseGrass = new Color(85, 107, 47, 255); // Dark olive green

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                // Add some variation to make it look natural
                int variation = random.Next(-5, 5);
                terrainColors[x, y] = new Color(
                    Math.Clamp(baseGrass.R + variation, 0, 255),
                    Math.Clamp(baseGrass.G + variation, 0, 255),
                    Math.Clamp(baseGrass.B + variation, 0, 255),
                    255
                );
            }
        }
    }

    private void RenderTerrain()
    {
        int tileSize = 32;
        int tilesX = terrainColors.GetLength(0);
        int tilesY = terrainColors.GetLength(1);

        // Calculate world bounds centered on viewport
        float centerX = width / 2f;
        float centerY = height / 2f;
        int startTileX = (int)((centerX - WorldWidth / 2f) / tileSize);
        int startTileY = (int)((centerY - WorldHeight / 2f) / tileSize);

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                int pixelX = (startTileX + x) * tileSize;
                int pixelY = (startTileY + y) * tileSize;

                // Draw grass terrain
                Raylib.DrawRectangle(pixelX, pixelY, tileSize, tileSize, terrainColors[x, y]);

                // Draw paths/footprints where agents walk frequently
                int worldTileX = startTileX + x;
                int worldTileY = startTileY + y;
                var key = (worldTileX, worldTileY);

                // Check for permanent roads first (fully opaque, connected paths)
                if (permanentRoads.Contains(key))
                {
                    Color roadColor = new Color(101, 67, 33, 200); // Solid connected road
                    Raylib.DrawRectangle(pixelX, pixelY, tileSize, tileSize, roadColor);
                }
                // Potential roads (medium opacity, awaiting connection)
                else if (potentialRoads.Contains(key))
                {
                    Color potentialColor = new Color(101, 67, 33, 120); // Semi-solid, not yet connected
                    Raylib.DrawRectangle(pixelX, pixelY, tileSize, tileSize, potentialColor);
                }
                // Then check for temporary paths (light transparency)
                else if (pathIntensity.ContainsKey(key))
                {
                    float intensity = pathIntensity[key];
                    Color pathColor = new Color(101, 67, 33, (int)(intensity * 60)); // Lighter
                    Raylib.DrawRectangle(pixelX, pixelY, tileSize, tileSize, pathColor);
                }
            }
        }
    }


    public void Render()
    {
        // Begin camera transformation
        Raylib.BeginMode2D(new Camera2D
        {
            Target = new Vector2(cameraX, cameraY),
            Offset = new Vector2(width / 2f, height / 2f),
            Rotation = 0f,
            Zoom = cameraZoom
        });

        // Draw terrain first
        RenderTerrain();

        // Draw terrain features (rocks, trees, patches)
        foreach (var feature in terrainFeatures)
        {
            feature.Render();
        }

        // Draw resources (so agents are on top)
        foreach (var food in foodSources)
        {
            food.Render();
        }

        foreach (var water in waterSources)
        {
            water.Render();
        }

        // Draw homes
        foreach (var home in homes)
        {
            home.Render();
        }

        // Draw corpses (on the ground, under agents)
        foreach (var corpse in corpses)
        {
            corpse.Render();
        }

        // Draw wood sources
        foreach (var wood in woodSources)
        {
            wood.Render();
        }

        // Draw relationship lines between agents (only when zoomed in)
        if (cameraZoom > 1.5f)
        {
            foreach (var agent in agents)
            {
                foreach (var relationship in agent.GetRelationships())
                {
                    var other = relationship.Key;
                    var trust = relationship.Value;

                    // Only draw very strong relationships (trust > 0.8)
                    if (trust > 0.8f && agents.Contains(other))
                    {
                        // Color based on trust level - cyan for strong bonds
                        int alpha = (int)(trust * 100);
                        Color lineColor = new Color(0, 200, 255, alpha);
                        Raylib.DrawLineV(agent.Position, other.Position, lineColor);
                    }
                }
            }
        }

        foreach (var agent in agents)
        {
            agent.Render();
        }

        // Draw particles
        foreach (var particle in particles)
        {
            particle.Render();
        }

        // End camera transformation
        Raylib.EndMode2D();

        // Draw semi-transparent background for UI
        Raylib.DrawRectangle(5, 35, 250, 220, new Color(0, 0, 0, 180));
        Raylib.DrawRectangleLines(5, 35, 250, 220, new Color(255, 255, 255, 100));

        // Draw UI with stats (not affected by camera)
        Raylib.DrawText($"Population: {agents.Count}", 15, 45, 22, Color.White);

        // Calculate average wellbeing
        float avgWellbeing = agents.Count > 0 ? agents.Average(a => a.WellBeing) : 0f;
        Color wellbeingColor = avgWellbeing > 0.7f ? Color.Green : avgWellbeing > 0.4f ? Color.Yellow : Color.Red;
        Raylib.DrawText($"Avg Well-being: {avgWellbeing:P0}", 15, 70, 16, wellbeingColor);

        Raylib.DrawText($"Food: {foodSources.Count}", 15, 95, 18, new Color(150, 255, 150, 255));
        Raylib.DrawText($"Water: {waterSources.Count}", 15, 115, 18, new Color(150, 200, 255, 255));

        // Count specialists
        int foodGatherers = agents.Count(a => a.AgentSpecialization == Agent.Specialization.FoodGatherer);
        int waterGatherers = agents.Count(a => a.AgentSpecialization == Agent.Specialization.WaterGatherer);
        int generalists = agents.Count(a => a.AgentSpecialization == Agent.Specialization.Generalist);

        Raylib.DrawText("Specialists:", 15, 145, 16, new Color(200, 200, 200, 255));
        Raylib.DrawText($"  Food: {foodGatherers}", 15, 165, 16, new Color(100, 200, 100, 255));
        Raylib.DrawText($"  Water: {waterGatherers}", 15, 185, 16, new Color(100, 150, 255, 255));
        Raylib.DrawText($"  General: {generalists}", 15, 205, 16, new Color(200, 200, 100, 255));
        Raylib.DrawText($"Homes: {homes.Count}", 15, 225, 18, new Color(255, 200, 150, 255));

        // Day/Night overlay (drawn AFTER everything else in world space, BEFORE UI text)
        Color ambientLight = GetAmbientLight();
        if (ambientLight.A > 0)
        {
            // Draw fullscreen overlay for day/night
            Raylib.DrawRectangle(0, 0, width, height, ambientLight);
        }

        // Time of day display
        float timeOfDay = GetTimeOfDay();
        int hour = (int)(timeOfDay * 24f);
        int minute = (int)((timeOfDay * 24f - hour) * 60f);
        string timeStr = $"{hour:D2}:{minute:D2}";
        string dayPhase = IsNightTime() ? "Night" : "Day";
        Raylib.DrawText($"Time: {timeStr} ({dayPhase})", 15, 250, 16, Color.White);
        Raylib.DrawText($"Day {currentDay}, Year {currentYear}", 15, 270, 14, new Color(200, 200, 200, 255));

        // Camera info
        Raylib.DrawText($"Zoom: {cameraZoom:F1}x (scroll)", 15, 295, 12, new Color(150, 150, 150, 255));
    }

    public Food? FindNearestFood(Vector2 position)
    {
        Food? nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var food in foodSources)
        {
            if (!food.IsConsumed)
            {
                float dist = Vector2.Distance(position, food.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = food;
                }
            }
        }

        return nearest;
    }

    public Water? FindNearestWater(Vector2 position)
    {
        Water? nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var water in waterSources)
        {
            if (!water.IsConsumed)
            {
                float dist = Vector2.Distance(position, water.Position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = water;
                }
            }
        }

        return nearest;
    }

    public void Resize(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;
        GenerateTerrain();
    }

    public bool CheckWorldBoundary(ref Vector2 position)
    {
        float centerX = width / 2f;
        float centerY = height / 2f;

        float worldLeft = centerX - WorldWidth / 2f;
        float worldRight = centerX + WorldWidth / 2f;
        float worldTop = centerY - WorldHeight / 2f;
        float worldBottom = centerY + WorldHeight / 2f;

        bool changed = false;

        // Clamp position to world bounds
        if (position.X < worldLeft)
        {
            position.X = worldLeft;
            changed = true;
        }
        else if (position.X > worldRight)
        {
            position.X = worldRight;
            changed = true;
        }

        if (position.Y < worldTop)
        {
            position.Y = worldTop;
            changed = true;
        }
        else if (position.Y > worldBottom)
        {
            position.Y = worldBottom;
            changed = true;
        }

        return changed;
    }

    public void LearnFromNeighbors(Agent learner)
    {
        // Find nearby agents and learn from the most successful one
        var nearbyAgents = agents
            .Where(a => a != learner && Vector2.Distance(learner.Position, a.Position) < 150f)
            .OrderByDescending(a => a.WellBeing)
            .Take(3) // Look at top 3 neighbors
            .ToList();

        if (nearbyAgents.Count == 0) return;

        Agent bestNeighbor = nearbyAgents[0];

        // Only learn from neighbors doing better than you
        if (bestNeighbor.WellBeing > learner.WellBeing + 0.15f)
        {
            // Small chance to copy their specialization (cultural diffusion!)
            if (random.NextDouble() < 0.1) // 10% chance
            {
                learner.AdoptSpecialization(bestNeighbor.AgentSpecialization);
            }
        }
    }

    public float GetPathSpeedBoost(Vector2 position)
    {
        int tileSize = 32;
        float centerX = width / 2f;
        float centerY = height / 2f;
        int startTileX = (int)((centerX - WorldWidth / 2f) / tileSize);
        int startTileY = (int)((centerY - WorldHeight / 2f) / tileSize);

        int tileX = (int)(position.X / tileSize) - startTileX;
        int tileY = (int)(position.Y / tileSize) - startTileY;

        var key = (tileX + startTileX, tileY + startTileY);

        // Permanent roads give maximum speed boost (1.5x)
        if (permanentRoads.Contains(key))
        {
            return 1.5f;
        }

        // Temporary paths give variable speed boost
        if (pathIntensity.ContainsKey(key))
        {
            float intensity = pathIntensity[key];
            // Well-traveled paths (>0.5 intensity) give up to 1.5x speed
            if (intensity > 0.5f)
            {
                return 1.0f + (intensity * 0.5f);
            }
        }

        return 1.0f; // No boost
    }

    public int Width => width;
    public int Height => height;

    // DAY/NIGHT CYCLE HELPERS
    public float GetTimeOfDay()
    {
        // Returns 0-1 where 0 is midnight, 0.5 is noon
        return (worldTime % DayLength) / DayLength;
    }

    public bool IsNightTime()
    {
        float timeOfDay = GetTimeOfDay();
        // Night is from 20:00 (0.833) to 06:00 (0.25)
        return timeOfDay < 0.25f || timeOfDay > 0.833f;
    }

    public Color GetAmbientLight()
    {
        float timeOfDay = GetTimeOfDay();

        // Dawn: 0.20 - 0.30 (5am-7am)
        if (timeOfDay >= 0.20f && timeOfDay < 0.30f)
        {
            float t = (timeOfDay - 0.20f) / 0.10f;
            return LerpColor(new Color(30, 30, 60, 180), new Color(255, 255, 255, 0), t);
        }
        // Day: 0.30 - 0.75 (7am-6pm)
        else if (timeOfDay >= 0.30f && timeOfDay < 0.75f)
        {
            return new Color(255, 255, 255, 0); // Full brightness
        }
        // Dusk: 0.75 - 0.85 (6pm-8pm)
        else if (timeOfDay >= 0.75f && timeOfDay < 0.85f)
        {
            float t = (timeOfDay - 0.75f) / 0.10f;
            return LerpColor(new Color(255, 255, 255, 0), new Color(30, 30, 60, 180), t);
        }
        // Night: 0.85 - 0.20 next day
        else
        {
            return new Color(30, 30, 60, 180); // Dark blue overlay
        }
    }

    private Color LerpColor(Color a, Color b, float t)
    {
        return new Color(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t),
            (int)(a.A + (b.A - a.A) * t)
        );
    }

    // Get mood penalty from nearby corpses
    public float GetCorpseMoodPenalty(Vector2 position)
    {
        float totalPenalty = 0f;
        foreach (var corpse in corpses)
        {
            float distance = Vector2.Distance(position, corpse.Position);
            if (distance < 150f) // Mood radius from Corpse.cs
            {
                totalPenalty += corpse.GetMoodPenalty();
            }
        }
        return totalPenalty;
    }
}
