using Raylib_cs;
using System.Numerics;

namespace SocietySim;

class Agent
{
    private static int nextId = 0;
    private int id;
    private float aliveTime = 0f;

    private Vector2 position;
    private Vector2 velocity;
    private World world;
    private Random random = new();

    // Agent state
    private float energy = 100f;
    private float maxEnergy = 100f;
    private float hydration = 100f;
    private float maxHydration = 100f;
    private float size = 10f; // Doubled size

    // LIFE CYCLE SYSTEM - REALISTIC LIFESPANS (check in day-to-day!)
    public enum LifeStage { Child, Adult, Elder }
    private LifeStage lifeStage = LifeStage.Adult;
    private float age = 0f; // In real-time seconds (not game days)
    private const float ChildhoodDuration = 172800f; // 2 DAYS (48 hours) as child
    private const float AdulthoodDuration = 604800f; // 1 WEEK (7 days) as adult
    private const float MaxLifespan = 1209600f; // 2 WEEKS total lifespan - follow their whole life story!
    private bool isDead = false;
    public string deathCause = ""; // Track how they died (for corpse system)

    // FAMILY SYSTEM
    private Agent? parent1 = null;
    private Agent? parent2 = null;
    private List<Agent> children = new List<Agent>();

    // PERSONALITY TRAITS (0-1 scale)
    private float generosity = 0.5f; // Willing to share vs selfish
    private float bravery = 0.5f; // Risk-taking vs cautious
    private float sociability = 0.5f; // Seeks company vs solitary

    // EMOTIONAL STATE
    private float happiness = 0.7f; // Current mood (0-1)
    private float stress = 0.2f; // Current stress level (0-1)

    // SLEEP SYSTEM
    private bool isSleeping = false;
    private float sleepiness = 0f; // 0-1, increases at night

    // Specialization - what this agent prefers to gather
    public enum Specialization { Generalist, FoodGatherer, WaterGatherer }
    private Specialization specialization;

    // Inventory for trading
    private float storedFood = 0f;
    private float storedWater = 0f;
    private const float MaxStorage = 100f;

    // Social relationships - tracks how much this agent trusts others
    private Dictionary<Agent, float> relationships = new();
    private const float InitialTrust = 0.5f;
    private const float TrustDecay = 0.01f;
    private const float HelpReward = 0.3f;

    // Home and territory
    private Home? home = null;
    private bool wantsHome = false;
    private float homelessTime = 0f;

    // Animation
    private float walkCycle = 0f;
    private float facingAngle = 0f;
    private float particleTimer = 0f;

    // Knowledge/Learning system - agents observe and copy successful neighbors
    private float learningTimer = 0f;
    private const float LearningInterval = 15f; // Check neighbors every 15 seconds
    private float wellBeing = 0.5f; // Tracks how well this agent is doing (0-1)

    // Visual feedback - track current target
    private Vector2? currentTarget = null;
    private string currentAction = "";

    // Gathering animation system
    private float gatheringTimer = 0f;
    private const float GatheringDuration = 1.5f; // 1.5 seconds to gather
    private bool isGathering = false;

    public Agent(Vector2 position, World world, Specialization? spec = null)
    {
        this.id = nextId++;
        this.position = position;
        this.world = world;

        // Random initial velocity
        this.velocity = new Vector2(
            (float)(random.NextDouble() - 0.5) * 100f,
            (float)(random.NextDouble() - 0.5) * 100f
        );

        // Assign specialization randomly if not specified
        if (spec.HasValue)
        {
            specialization = spec.Value;
        }
        else
        {
            var roll = random.NextDouble();
            if (roll < 0.4) specialization = Specialization.FoodGatherer;
            else if (roll < 0.8) specialization = Specialization.WaterGatherer;
            else specialization = Specialization.Generalist;
        }

        // Generate random personality traits (normal distribution around 0.5)
        generosity = (float)(random.NextDouble() * 0.6 + 0.2); // 0.2 to 0.8
        bravery = (float)(random.NextDouble() * 0.6 + 0.2);
        sociability = (float)(random.NextDouble() * 0.6 + 0.2);

        // Random starting age variation
        age = (float)(random.NextDouble() * ChildhoodDuration); // Start somewhere in childhood
    }

    public void Update(float deltaTime)
    {
        aliveTime += deltaTime;
        bool seeking = false;

        // SLEEP SYSTEM - Track sleepiness based on time of day
        bool isNight = world.IsNightTime();
        if (isNight)
        {
            sleepiness += deltaTime * 0.3f; // Get tired at night
        }
        else
        {
            sleepiness -= deltaTime * 0.2f; // Wake up during day
        }
        sleepiness = Math.Clamp(sleepiness, 0f, 1f);

        // Decide whether to sleep
        if (isNight && sleepiness > 0.6f && home != null && Vector2.Distance(position, home.Position) < 50f)
        {
            isSleeping = true;
            currentAction = "Sleeping";
        }
        else if (!isNight || sleepiness < 0.3f)
        {
            isSleeping = false;
        }

        // Skip most actions if sleeping
        if (isSleeping)
        {
            velocity *= 0.95f; // Slow down when sleeping
            // Still consume resources but slower
            energy -= 0.1f * deltaTime;
            hydration -= 0.05f * deltaTime;
            return; // Don't do anything else while sleeping
        }

        // Calculate well-being (how well is this agent doing?)
        wellBeing = ((energy / maxEnergy) + (hydration / maxHydration)) / 2f;
        if (home != null) wellBeing += 0.2f; // Having a home is good
        if (storedFood + storedWater > 50f) wellBeing += 0.1f; // Having resources is good
        wellBeing = Math.Clamp(wellBeing, 0f, 1f);

        // Learning from successful neighbors
        learningTimer += deltaTime;
        if (learningTimer >= LearningInterval)
        {
            learningTimer = 0f;
            world.LearnFromNeighbors(this);
        }

        // Track homelessness - want a home after being alive for a bit
        if (home == null)
        {
            homelessTime += deltaTime;
            if (homelessTime > 5f) // Want home after 5 seconds
            {
                wantsHome = true;
            }
        }

        // Return home if carrying lots of resources
        if (home != null && (storedFood > 40f || storedWater > 40f))
        {
            float distToHome = Vector2.Distance(position, home.Position);
            if (distToHome > 20f)
            {
                // Go home to store resources
                seeking = true;
                currentTarget = home.Position;
                currentAction = "→ Home";
                Vector2 toHome = home.Position - position;
                float steerStrength = distToHome > 50f ? 250f : 150f;
                Vector2 steerForce = Vector2.Normalize(toHome) * steerStrength;
                velocity += steerForce * deltaTime;
            }
            else
            {
                // Slow down when arriving home
                velocity *= 0.3f;
                currentTarget = null;
                currentAction = "Storing";
                // Store resources at home
                home.StoreResources(storedFood, storedWater);
                storedFood = 0f;
                storedWater = 0f;
            }
        }

        // Check if we can use home storage first (if low on resources and near home)
        if (home != null)
        {
            float distToHome = Vector2.Distance(position, home.Position);
            if (distToHome < 50f) // Near home
            {
                // Try to withdraw from home if low
                if (energy < maxEnergy * 0.5f && home.StoredFood > 0)
                {
                    home.WithdrawResources(out float withdrawnFood, out _, 30f, 0f);
                    energy = Math.Min(energy + withdrawnFood, maxEnergy);
                }
                if (hydration < maxHydration * 0.5f && home.StoredWater > 0)
                {
                    home.WithdrawResources(out _, out float withdrawnWater, 0f, 30f);
                    hydration = Math.Min(hydration + withdrawnWater, maxHydration);
                }
            }
        }

        // Determine what we need most urgently
        bool needFood = energy < maxEnergy * 0.7f || (specialization == Specialization.FoodGatherer && storedFood < MaxStorage);
        bool needWater = hydration < maxHydration * 0.7f || (specialization == Specialization.WaterGatherer && storedWater < MaxStorage);

        // Seek food if needed and it's our preference
        if (needFood && (specialization != Specialization.WaterGatherer || energy < 40f))
        {
            var nearestFood = world.FindNearestFood(position);
            if (nearestFood != null)
            {
                seeking = true;
                currentTarget = nearestFood.Position;
                currentAction = "→ Food";
                Vector2 toFood = nearestFood.Position - position;
                float distance = toFood.Length();

                // Collect food if close enough
                if (distance < 15f)
                {
                    // Stop moving to collect
                    velocity *= 0.3f;
                    currentAction = "Gathering";
                    currentTarget = null;

                    // Start gathering animation if not already gathering
                    if (!isGathering)
                    {
                        isGathering = true;
                        gatheringTimer = 0f;
                    }

                    // Update gathering timer
                    gatheringTimer += deltaTime;

                    // Only collect after animation completes
                    if (gatheringTimer >= GatheringDuration)
                    {
                        float energyGain = nearestFood.EnergyValue;
                        if (energy < maxEnergy)
                        {
                            float consumed = Math.Min(energyGain, maxEnergy - energy);
                            energy += consumed;
                            energyGain -= consumed;
                        }
                        // Store excess if specialist
                        if (energyGain > 0 && storedFood < MaxStorage)
                        {
                            storedFood = Math.Min(storedFood + energyGain, MaxStorage);
                        }
                        nearestFood.Consume();
                        isGathering = false;
                        gatheringTimer = 0f;

                        // Spawn particle burst for gathering food
                        Random particleRandom = new Random();
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = (float)(particleRandom.NextDouble() * Math.PI * 2);
                            float particleSpeed = 50f + (float)(particleRandom.NextDouble() * 50f);
                            Vector2 particleVel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * particleSpeed;
                            world.SpawnParticle(position, particleVel, new Color(100, 200, 100, 255), 0.8f, 3f);
                        }
                    }
                }
                else if (distance > 0)
                {
                    // Reset gathering state when moving away
                    isGathering = false;
                    gatheringTimer = 0f;

                    // Move towards food with stronger force when far, gentler when close
                    float steerStrength = distance > 50f ? 300f : 150f;
                    Vector2 steerForce = Vector2.Normalize(toFood) * steerStrength;
                    velocity += steerForce * deltaTime;
                }
            }
            else
            {
                currentTarget = null;
                currentAction = "";
            }
        }
        // Seek water if needed
        else if (needWater && (specialization != Specialization.FoodGatherer || hydration < 40f))
        {
            var nearestWater = world.FindNearestWater(position);
            if (nearestWater != null)
            {
                seeking = true;
                currentTarget = nearestWater.Position;
                currentAction = "→ Water";
                Vector2 toWater = nearestWater.Position - position;
                float distance = toWater.Length();

                // Collect water if close enough
                if (distance < 15f)
                {
                    // Stop moving to collect
                    velocity *= 0.3f;
                    currentAction = "Drinking";
                    currentTarget = null;

                    float hydrationGain = nearestWater.HydrationValue;
                    if (hydration < maxHydration)
                    {
                        float consumed = Math.Min(hydrationGain, maxHydration - hydration);
                        hydration += consumed;
                        hydrationGain -= consumed;
                    }
                    // Store excess if specialist
                    if (hydrationGain > 0 && storedWater < MaxStorage)
                    {
                        storedWater = Math.Min(storedWater + hydrationGain, MaxStorage);
                    }
                    nearestWater.Consume();

                    // Spawn particle burst for drinking water
                    Random particleRandom = new Random();
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (float)(particleRandom.NextDouble() * Math.PI * 2);
                        float particleSpeed = 50f + (float)(particleRandom.NextDouble() * 50f);
                        Vector2 particleVel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * particleSpeed;
                        world.SpawnParticle(position, particleVel, new Color(100, 150, 255, 255), 0.8f, 3f);
                    }
                }
                else if (distance > 0)
                {
                    // Move towards water with stronger force when far, gentler when close
                    float steerStrength = distance > 50f ? 300f : 150f;
                    Vector2 steerForce = Vector2.Normalize(toWater) * steerStrength;
                    velocity += steerForce * deltaTime;
                }
            }
            else
            {
                currentTarget = null;
                currentAction = "";
            }
        }

        if (!seeking)
        {
            // Slow down when not seeking anything
            velocity *= 0.95f;

            // Wander when satisfied (gentler)
            velocity += new Vector2(
                (float)(random.NextDouble() - 0.5) * 100f * deltaTime,
                (float)(random.NextDouble() - 0.5) * 100f * deltaTime
            );
        }

        // Check if on a well-traveled path and get speed boost
        float speedMultiplier = world.GetPathSpeedBoost(position);

        // Limit speed (with path bonus)
        // Human walking speed: ~1.4 m/s, world is 2400x1800 pixels
        // If we assume world = ~100m, then 2400px = 100m → 24px/m
        // 1.4 m/s × 24 px/m = 33.6 px/s (realistic walking)
        float maxSpeed = 35f * speedMultiplier;
        if (velocity.Length() > maxSpeed)
        {
            velocity = Vector2.Normalize(velocity) * maxSpeed;
        }

        // Update position
        position += velocity * deltaTime;

        // Update walking animation based on movement speed
        float speed = velocity.Length();
        if (speed > 10f)
        {
            walkCycle += deltaTime * speed * 0.05f;

            // Spawn dust particles occasionally when walking (less frequently)
            particleTimer += deltaTime;
            if (particleTimer > 0.5f) // Reduced from 0.2f
            {
                particleTimer = 0f;
                world.SpawnParticle(
                    position + new Vector2(0, size),
                    new Vector2(
                        (float)(random.NextDouble() - 0.5) * 20f,
                        (float)(random.NextDouble() - 0.5) * 20f
                    ),
                    new Color(139, 107, 47, 100), // Lighter dust
                    0.4f, // Shorter lifetime
                    1.5f // Smaller size
                );
            }

            // Record path for footprints
            world.RecordFootstep(position, id, aliveTime);
        }

        // Update facing direction based on velocity
        if (velocity.Length() > 5f)
        {
            facingAngle = MathF.Atan2(velocity.Y, velocity.X);
        }

        // Keep agents within world bounds - bounce back if hitting edge
        if (world.CheckWorldBoundary(ref position))
        {
            // Hit world boundary, reverse direction
            velocity *= -0.5f; // Bounce back with reduced speed
        }

        // Resources decrease over time (slower if near home/settlement)
        float consumptionRate = 1.0f;
        if (home != null && Vector2.Distance(position, home.Position) < 100f)
        {
            consumptionRate = 0.8f; // 20% slower consumption near home (shelter benefit)
        }

        // REALISTIC SLOWER CONSUMPTION - agents can survive 2-3 minutes without resources
        energy -= 0.4f * deltaTime * consumptionRate; // Was 2f - now 5x slower
        if (energy < 0) energy = 0;

        hydration -= 0.3f * deltaTime * consumptionRate; // Was 1.5f - now 5x slower
        if (hydration < 0) hydration = 0;

        // AGING SYSTEM - Age and life stage progression
        age += deltaTime;

        // Update life stage based on age
        if (age < ChildhoodDuration)
        {
            lifeStage = LifeStage.Child;
        }
        else if (age < ChildhoodDuration + AdulthoodDuration)
        {
            lifeStage = LifeStage.Adult;
        }
        else
        {
            lifeStage = LifeStage.Elder;
        }

        // Death from old age
        if (age >= MaxLifespan)
        {
            isDead = true;
            deathCause = "Old Age";
            return;
        }

        // Death from starvation (both resources depleted for extended time)
        if (energy <= 0 && hydration <= 0)
        {
            isDead = true;
            deathCause = "Starvation";
            return;
        }

        // EMOTIONAL SYSTEM - Update emotions based on well-being and environment
        float corpseMoodPenalty = world.GetCorpseMoodPenalty(position);
        float targetHappiness = wellBeing + corpseMoodPenalty; // Happy when needs met, sad near corpses
        targetHappiness = Math.Clamp(targetHappiness, 0f, 1f);
        happiness += (targetHappiness - happiness) * deltaTime * 0.5f; // Smooth transition

        // Stress increases when needs are low or near corpses
        float targetStress = (1f - wellBeing) - corpseMoodPenalty; // Corpses cause stress
        targetStress = Math.Clamp(targetStress, 0f, 1f);
        stress += (targetStress - stress) * deltaTime * 0.3f;
    }

    public void Render()
    {
        // VISUAL FEEDBACK: Well-being status ring
        Color statusColor;
        if (wellBeing > 0.7f)
            statusColor = new Color(0, 255, 0, 150); // Green - thriving
        else if (wellBeing > 0.4f)
            statusColor = new Color(255, 255, 0, 150); // Yellow - okay
        else
            statusColor = new Color(255, 0, 0, 200); // Red - struggling

        // Pulsing ring based on urgency
        float pulseSize = wellBeing < 0.4f ? MathF.Sin(aliveTime * 4f) * 2f : 0f;
        Raylib.DrawCircleLines((int)position.X, (int)position.Y, size + 8f + pulseSize, statusColor);

        // Destination lines removed for cleaner visuals

        // VISUAL FEEDBACK: Draw action text
        if (!string.IsNullOrEmpty(currentAction))
        {
            Raylib.DrawText(currentAction, (int)(position.X - 20), (int)(position.Y - size - 35), 10, Color.White);
        }

        // Color based on specialization with health tint
        Color baseColor;
        switch (specialization)
        {
            case Specialization.FoodGatherer:
                baseColor = new Color(100, 200, 100, 255); // Greenish
                break;
            case Specialization.WaterGatherer:
                baseColor = new Color(100, 150, 255, 255); // Blueish
                break;
            default:
                baseColor = new Color(200, 200, 100, 255); // Yellowish
                break;
        }

        // Darken color if low on resources
        float healthRatio = Math.Min(energy / maxEnergy, hydration / maxHydration);
        Color agentColor = new Color(
            (int)(baseColor.R * (0.3f + healthRatio * 0.7f)),
            (int)(baseColor.G * (0.3f + healthRatio * 0.7f)),
            (int)(baseColor.B * (0.3f + healthRatio * 0.7f)),
            255
        );

        // Bobbing animation when walking
        float bob = MathF.Sin(walkCycle * 6f) * 1.5f;
        Vector2 renderPos = position + new Vector2(0, bob);

        // Draw shadow
        Color shadowColor = new Color(0, 0, 0, 60);
        Raylib.DrawEllipse((int)(position.X + 2), (int)(position.Y + size + 2), (int)(size * 0.8f), (int)(size * 0.3f), shadowColor);

        // Determine facing direction (flip sprite if moving left)
        float facing = facingAngle;
        bool facingLeft = MathF.Cos(facingAngle) < 0;
        float flipMult = facingLeft ? -1f : 1f;

        // Draw simple humanoid sprite
        // Head
        Raylib.DrawCircleV(renderPos + new Vector2(0, -size * 0.3f), size * 0.4f, agentColor);

        // Eyes to show direction
        Color eyeColor = new Color(50, 50, 50, 255);
        float eyeOffset = flipMult * 2f;
        Raylib.DrawCircleV(renderPos + new Vector2(eyeOffset, -size * 0.35f), 1.2f, eyeColor);

        // Body (torso)
        Color bodyColor = new Color(
            (int)(agentColor.R * 0.8f),
            (int)(agentColor.G * 0.8f),
            (int)(agentColor.B * 0.8f),
            255
        );
        Raylib.DrawRectangle(
            (int)(renderPos.X - size * 0.3f),
            (int)(renderPos.Y + size * 0.1f),
            (int)(size * 0.6f),
            (int)(size * 0.8f),
            bodyColor
        );

        // Arms (simple lines) - animate with walking or gathering
        Color limbColor = new Color(
            (int)(agentColor.R * 0.7f),
            (int)(agentColor.G * 0.7f),
            (int)(agentColor.B * 0.7f),
            255
        );

        float armSwing;
        if (isGathering)
        {
            // Gathering animation - arms move down in a reaching motion
            float gatherProgress = gatheringTimer / GatheringDuration;
            armSwing = MathF.Sin(gatherProgress * MathF.PI * 2) * 5f; // Bigger arm movement
        }
        else
        {
            armSwing = MathF.Sin(walkCycle * 6f) * 3f;
        }

        // Left arm
        Raylib.DrawLineEx(
            renderPos + new Vector2(-size * 0.3f * flipMult, size * 0.2f),
            renderPos + new Vector2(-size * 0.6f * flipMult, size * 0.5f + armSwing),
            2f,
            limbColor
        );
        // Right arm
        Raylib.DrawLineEx(
            renderPos + new Vector2(size * 0.3f * flipMult, size * 0.2f),
            renderPos + new Vector2(size * 0.6f * flipMult, size * 0.5f - armSwing),
            2f,
            limbColor
        );

        // Legs (simple lines) - animate with walking
        float legSwing = MathF.Sin(walkCycle * 6f + MathF.PI) * 4f;
        // Left leg
        Raylib.DrawLineEx(
            renderPos + new Vector2(-size * 0.1f, size * 0.9f),
            renderPos + new Vector2(-size * 0.3f, size * 1.3f + legSwing),
            2.5f,
            limbColor
        );
        // Right leg
        Raylib.DrawLineEx(
            renderPos + new Vector2(size * 0.1f, size * 0.9f),
            renderPos + new Vector2(size * 0.3f, size * 1.3f - legSwing),
            2.5f,
            limbColor
        );

        // Draw resource bars above agent
        float barWidth = 40f;
        float barHeight = 5f;
        Vector2 barPos = position + new Vector2(-barWidth / 2, -size - 20);

        // Energy bar (green)
        Raylib.DrawRectangleV(barPos, new Vector2(barWidth * (energy / maxEnergy), barHeight), Color.Green);
        Raylib.DrawRectangleLinesEx(
            new Rectangle(barPos.X, barPos.Y, barWidth, barHeight),
            1f,
            new Color(255, 255, 255, 100)
        );

        // Hydration bar (blue)
        Vector2 waterBarPos = barPos + new Vector2(0, barHeight + 1);
        Raylib.DrawRectangleV(waterBarPos, new Vector2(barWidth * (hydration / maxHydration), barHeight), Color.SkyBlue);
        Raylib.DrawRectangleLinesEx(
            new Rectangle(waterBarPos.X, waterBarPos.Y, barWidth, barHeight),
            1f,
            new Color(255, 255, 255, 100)
        );

        // Gathering progress bar (show when gathering)
        if (isGathering)
        {
            Vector2 gatherBarPos = barPos + new Vector2(0, (barHeight + 1) * 2);
            float gatherProgress = gatheringTimer / GatheringDuration;
            Raylib.DrawRectangleV(gatherBarPos, new Vector2(barWidth * gatherProgress, barHeight), Color.Orange);
            Raylib.DrawRectangleLinesEx(
                new Rectangle(gatherBarPos.X, gatherBarPos.Y, barWidth, barHeight),
                1f,
                new Color(255, 255, 255, 150)
            );
        }
    }

    public void ReceiveHelp(Agent helper, float energyAmount)
    {
        energy = Math.Min(energy + energyAmount, maxEnergy);

        // Increase trust toward the helper
        if (!relationships.ContainsKey(helper))
        {
            relationships[helper] = InitialTrust;
        }
        relationships[helper] = Math.Min(relationships[helper] + HelpReward, 1.0f);
    }

    public bool WillHelp(Agent other)
    {
        if (!relationships.ContainsKey(other))
        {
            relationships[other] = InitialTrust;
        }

        // More likely to help if we trust them
        return random.NextDouble() < relationships[other];
    }

    public void ShareEnergyWith(Agent other, float amount)
    {
        if (energy > amount)
        {
            energy -= amount;
            other.ReceiveHelp(this, amount);
        }
    }

    public Dictionary<Agent, float> GetRelationships() => relationships;

    public bool NeedsResource(out bool needsFood, out bool needsWater)
    {
        needsFood = energy < maxEnergy * 0.5f;
        needsWater = hydration < maxHydration * 0.5f;
        return needsFood || needsWater;
    }

    public bool CanTrade(out float availableFood, out float availableWater)
    {
        availableFood = storedFood;
        availableWater = storedWater;
        return storedFood > 20f || storedWater > 20f;
    }

    public void Trade(Agent other)
    {
        // Simple barter - give what they need if we have excess
        if (other.NeedsResource(out bool otherNeedsFood, out bool otherNeedsWater))
        {
            if (otherNeedsFood && storedFood > 20f)
            {
                float tradeAmount = Math.Min(30f, storedFood);
                storedFood -= tradeAmount;
                other.energy = Math.Min(other.energy + tradeAmount, other.maxEnergy);

                // Build trust through successful trade
                if (!relationships.ContainsKey(other))
                    relationships[other] = InitialTrust;
                relationships[other] = Math.Min(relationships[other] + HelpReward * 0.5f, 1.0f);
            }

            if (otherNeedsWater && storedWater > 20f)
            {
                float tradeAmount = Math.Min(30f, storedWater);
                storedWater -= tradeAmount;
                other.hydration = Math.Min(other.hydration + tradeAmount, other.maxHydration);

                // Build trust through successful trade
                if (!relationships.ContainsKey(other))
                    relationships[other] = InitialTrust;
                relationships[other] = Math.Min(relationships[other] + HelpReward * 0.5f, 1.0f);
            }
        }
    }

    public void SetHome(Home newHome)
    {
        home = newHome;
        wantsHome = false;
    }

    public Vector2 Position => position;
    public float Energy => energy;
    public float Hydration => hydration;
    public Home? AgentHome => home;
    public bool WantsHome => wantsHome;

    public void ConsumeForReproduction()
    {
        // Reproduction costs significant resources
        energy -= 30f;
        hydration -= 30f;
    }

    public Agent CreateOffspring(Agent? partner, Vector2 birthPosition, World world)
    {
        var offspring = new Agent(birthPosition, world);

        // GENETIC INHERITANCE - Inherit traits from parents
        if (partner != null)
        {
            // Mix personality traits (50% from each parent + small mutation)
            offspring.generosity = (this.generosity + partner.generosity) / 2f + (float)(random.NextDouble() - 0.5) * 0.1f;
            offspring.bravery = (this.bravery + partner.bravery) / 2f + (float)(random.NextDouble() - 0.5) * 0.1f;
            offspring.sociability = (this.sociability + partner.sociability) / 2f + (float)(random.NextDouble() - 0.5) * 0.1f;

            // Clamp to valid range
            offspring.generosity = Math.Clamp(offspring.generosity, 0f, 1f);
            offspring.bravery = Math.Clamp(offspring.bravery, 0f, 1f);
            offspring.sociability = Math.Clamp(offspring.sociability, 0f, 1f);

            // Set family relationships
            offspring.parent1 = this;
            offspring.parent2 = partner;
            this.children.Add(offspring);
            partner.children.Add(offspring);
        }
        else
        {
            // Asexual reproduction - inherit from single parent with mutation
            offspring.generosity = this.generosity + (float)(random.NextDouble() - 0.5) * 0.15f;
            offspring.bravery = this.bravery + (float)(random.NextDouble() - 0.5) * 0.15f;
            offspring.sociability = this.sociability + (float)(random.NextDouble() - 0.5) * 0.15f;

            offspring.generosity = Math.Clamp(offspring.generosity, 0f, 1f);
            offspring.bravery = Math.Clamp(offspring.bravery, 0f, 1f);
            offspring.sociability = Math.Clamp(offspring.sociability, 0f, 1f);

            offspring.parent1 = this;
            this.children.Add(offspring);
        }

        // Start as newborn (age 0)
        offspring.age = 0f;
        offspring.lifeStage = LifeStage.Child;

        return offspring;
    }

    public void AdoptSpecialization(Specialization newSpec)
    {
        // Agent learns a new specialization from successful neighbors
        specialization = newSpec;
    }

    public bool IsDead => isDead;
    public string DeathCause => deathCause;
    public bool CanReproduce => energy > maxEnergy * 0.85f && hydration > maxHydration * 0.85f;
    public Specialization AgentSpecialization => specialization;
    public float WellBeing => wellBeing;
}

