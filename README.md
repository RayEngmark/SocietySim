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

## Project Structure

- `Agent.cs` - Agent behavior, needs, and decision-making
- `World.cs` - World simulation, camera, rendering
- `Home.cs` - Home buildings with resource production
- `Food.cs` - Food resource with regeneration
- `Water.cs` - Water resource with regeneration
- `Particle.cs` - Visual particle effects
- `TerrainFeature.cs` - Environmental decorations
- `Program.cs` - Entry point and game loop

## License

MIT
