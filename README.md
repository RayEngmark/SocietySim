# SocietySim

A society simulation built with C# and Raylib, featuring autonomous agents, resource management, emergent economies, and social behaviors.

## Features

### Agent Behavior
- **Autonomous Agents**: Agents make decisions based on needs (energy, hydration)
- **Specializations**: Food gatherers, water gatherers, and generalists
- **Visual Feedback**: Color-coded status rings show agent well-being
  - Green: Thriving (>70% well-being)
  - Yellow: Struggling (40-70% well-being)
  - Red: Critical (<40% well-being) with pulsing ring
- **Action Display**: See what each agent is doing in real-time
- **Gathering Animations**: Agents pause and animate when collecting resources

### Resource Systems
- **Regenerating Resources**: Food and water regenerate over time
- **Harvest Pressure**: Resources take longer to regenerate when heavily used
- **Home Production**: Homes produce resources based on owner specialization
- **Storage & Trading**: Agents store excess resources at home

### Social Systems
- **Relationship Building**: Trust develops between agents through cooperation
- **Knowledge Propagation**: Agents learn successful strategies from thriving neighbors
- **Home Ownership**: Agents can claim and build homes

### Visual Polish
- **Particle Effects**: Bursts when gathering food (green) or water (blue)
- **Destination Lines**: White lines show where agents are heading
- **Gathering Progress Bar**: Orange bar shows gathering completion
- **Detailed HUD**: Population, resources, specialization breakdown, average well-being
- **Terrain**: Procedurally generated terrain with trees, rocks, and vegetation

### World Features
- **Emergent Paths**: Paths form naturally where agents walk frequently
- **Large World**: 8000x5600 explorable area
- **Camera Controls**: Zoom with mouse scroll, pan by moving near edges
- **Windowed Fullscreen**: Runs in borderless fullscreen mode

## Controls

- **Mouse Scroll**: Zoom in/out
- **Move mouse to screen edges**: Pan camera
- **ESC**: Close application

## Requirements

- .NET 8.0 SDK
- Raylib-cs (included via NuGet)

## Building & Running

```bash
dotnet restore
dotnet run
```

### Life Cycle & Genetics
- **Aging System**: Agents progress through Child (60s) → Adult (5min) → Elder → Death (8min total)
- **Genetic Inheritance**: Offspring inherit personality traits from parents with mutations
- **Death & Mortality**: Agents die from old age or starvation (both resources depleted)
- **Family Relationships**: Parents and children tracked

### Day/Night Cycle
- **Time System**: 2-minute day/night cycles with year tracking
- **Dynamic Lighting**: Dawn, day, dusk, night with ambient overlay
- **Sleep Behavior**: Agents rest at night near their homes
- **Realistic Survival**: 2-3 minute survival times without resources

## 📋 Implementation Plan

**See [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) for detailed roadmap**

### Priority Features (Next Session):
1. ✅ Corpse system with blood, rot, mood effects
2. ✅ Wood gathering with chopping animations
3. 🔲 Visual action indicators (replace text)
4. 🔲 Town planning with organized zones
5. 🔲 Pregnancy & realistic childbirth
6. 🔲 Leadership & orders system
7. 🔲 Cemetery & burial mechanics

### Stretch Goals:
- Seasons, disease, warfare
- Religion, tech trees, trade routes
- Advanced AI and emergent storytelling

## Project Structure

### Core Systems
- `Agent.cs` - Agent behavior, life cycle, personality, genetics
- `World.cs` - World simulation, camera, time system
- `Home.cs` - Home buildings with resource production
- `Corpse.cs` - Dead body system with rot and mood effects
- `Wood.cs` - Wood resource for building

### Resources
- `Food.cs` - Food resource with regeneration
- `Water.cs` - Water resource with regeneration

### Visuals
- `Particle.cs` - Visual particle effects
- `TerrainFeature.cs` - Environmental decorations
- `Program.cs` - Entry point and game loop

## License

MIT
