# SocietySim - Detailed Implementation Plan

## 🎯 Current Status (Completed in this session)

### ✅ Foundational Systems
1. **Aging & Life Cycle** - Agents progress through Child → Adult → Elder → Death
2. **Death & Mortality** - Agents die from old age (8min) or starvation
3. **Personality Traits** - Generosity, bravery, sociability (genetically inherited)
4. **Emotional States** - Happiness and stress tied to well-being
5. **Genetic Reproduction** - Offspring inherit parent traits with mutations
6. **Day/Night Cycle** - 2-minute cycles with dynamic lighting and sleep behavior
7. **Realistic Survival** - 2-3 minute survival times (5x slower consumption)

### ✅ Visual & Polish
- Removed destination lines for cleaner look
- Day/night ambient lighting overlay
- Sleep behavior at night near homes
- Time display (hour, day, year)

### 🚧 Partially Implemented (Files Created)
- **Corpse.cs** - Dead body system with rot, blood pools, flies, mood effects
- **Wood.cs** - Wood resource with regeneration and harvest mechanics

---

## 🔥 PRIORITY IMPLEMENTATIONS (Next Session)

### 1. **Corpse & Death System** (30 min)
**What's needed:**
- Integrate Corpse.cs into World.cs
- Create corpse when agent dies (store position, cause of death)
- Render corpses with blood pools
- Add fly particles around rotten corpses
- Mood penalty for agents near corpses
- Contaminate nearby food sources

**Files to modify:**
- `World.cs` - Add corpses list, update/render loop
- `Agent.cs` - Create corpse on death in Update()
- `Agent.cs` - Check nearby corpses for mood penalty

### 2. **Wood Gathering & Building Requirements** (45 min)
**What's needed:**
- Agents need wood to build homes
- Add "Chopping Wood" animation (3-5 sec gather time)
- Visual: Agent swinging axe motion, wood chips particles
- Homes require 20 wood to build
- Store wood in agent inventory
- Building animation: hammering, placing planks visually

**New specialization:** `WoodChopper`

**Files to modify:**
- `Agent.cs` - Add woodInventory, chopping behavior, building with wood
- `World.cs` - Integrate wood sources, update/render
- `Home.cs` - Add construction state (foundation → walls → roof → complete)

### 3. **Visual Action Indicators** (30 min)
**Replace text with animations:**
- **Gathering food:** Bend down animation, berry picking motion
- **Drinking water:** Kneel animation, cupped hands
- **Chopping wood:** Axe swing, wood chips flying
- **Building:** Hammering motion, planks appearing
- **Sleeping:** Lying down, Zzz particles
- **Eating:** Hand to mouth motion

**Implementation:**
- Add `animationState` and `animationTimer` to Agent
- Create simple sprite rotations/positions for actions
- Particle effects for each action type

---

## 🏗️ MAJOR FEATURES (Future Sessions)

### 4. **Town Planning & Zoning** (60-90 min)
**Problems to solve:**
- Random house placement = messy towns
- No roads or structure
- Resources scattered randomly

**Solution - Town Center System:**
1. **Elect a "Planner"** (highest intelligence agent)
2. Planner designates zones:
   - **Residential Zone** - Grid of house plots (organized rows)
   - **Resource Zone** - Near forests, water, berries
   - **Cemetery Zone** - Dedicated burial area
   - **Market Zone** - Trading area (future)

3. **Building System:**
   - Planner places foundation markers (ghost buildings)
   - Agents bring wood to marked locations
   - Collaborative building (multiple agents work together)
   - Buildings take 30-60 seconds with multiple workers

4. **Road System:**
   - Planner marks road paths between zones
   - Agents walking on paths compact them (dirt → gravel → stone)
   - Roads boost movement speed

**New Files:**
- `TownPlanner.cs` - AI for zone placement
- `BuildingProject.cs` - Track construction progress
- `Zone.cs` - Define zone types and boundaries

### 5. **Pregnancy & Childbirth System** (60 min)
**Current Problem:** Reproduction is instant and asexual

**New System:**
1. **Courtship (15-30 sec)**
   - Two compatible adults meet
   - Hearts particle effect
   - Both must be healthy (>70% well-being)

2. **Pregnancy (90 seconds)**
   - Visual: Belly grows over time
   - Mother moves 50% slower
   - Needs 50% more food/water
   - Must stay near home

3. **Labor & Birth**
   - Mother goes to home when timer expires
   - Needs midwife helper (any nearby female adult)
   - **Skill check:**
     - High midwife skill: Safe birth, healthy baby
     - Medium skill: Baby weak (starts at 50% energy)
     - Low skill: Risk of death for mother (20% chance) or baby (30% chance)
   - Blood visuals during birth

4. **Newborn Care**
   - Baby can't move for 30 seconds
   - Parent must stay nearby
   - Visual: Small agent, needs carrying

**New Files:**
- `PregnancyComponent.cs` - Track pregnancy state
- `ChildbirthEvent.cs` - Handle birth complications

**Files to modify:**
- `Agent.cs` - Add pregnancy state, midwife skill, birth logic
- `World.cs` - Matchmaking logic for reproduction

### 6. **Leadership & Orders System** (45 min)
**Election System:**
1. Agents vote every 2 in-game days
2. Candidates: Adults with high social trust
3. Leader gets crown icon above head
4. **Bad leaders possible:** Low intelligence = poor decisions

**Leader Powers:**
1. **Assign roles** - Tell agents to gather specific resources
2. **Prioritize projects** - Focus on building, defense, etc.
3. **Declare emergencies** - All gather food during drought

**Leader Types:**
- **Benevolent:** Shares resources equally, builds slowly
- **Tyrant:** Hoards resources, forces labor
- **Incompetent:** Random decisions, chaos

**Files:**
- `Leader.cs` - Leader AI and decision making
- `Order.cs` - Command queue for agents

### 7. **Cemetery & Burial System** (30 min)
**Requirements:**
- Dedicated cemetery zone (planner designates)
- Agents assigned "Gravedigger" role
- Corpses older than 60 sec must be buried
- Burial process: Carry corpse → Dig grave → Place body → Place marker
- Graves are permanent (stone markers)
- Cemetery visit behavior (mourning, -10% happiness for 30 sec)

**Visuals:**
- Gravedigger shovel animation
- Cross/stone grave markers
- Flowers on recent graves

---

## 🎨 VISUAL POLISH REQUIREMENTS

### Animation States Needed:
```csharp
enum AnimationState {
    Idle,
    Walking,
    Gathering,      // Bending down
    Chopping,       // Axe swing
    Building,       // Hammering
    Carrying,       // Holding object
    Drinking,       // Kneeling
    Sleeping,       // Lying down
    GivingBirth,    // Special pose
    Digging,        // Shovel motion
    Mourning        // Sad pose
}
```

### Particle Systems Needed:
- ✅ Food gather (green burst) - EXISTS
- ✅ Water gather (blue burst) - EXISTS
- Wood chips (brown particles)
- Dust clouds (walking)
- Hearts (courtship)
- Blood (birth/death)
- Zzz (sleeping)
- Sweat drops (hard labor)
- Flies (corpses)

---

## 📊 BALANCE ADJUSTMENTS NEEDED

### Current Issues to Fix:
1. **Reproduction rate TOO FAST**
   - Change from 0.2% to 0.02% chance per frame
   - Add cooldown: 2 in-game days between births
   - Require partner (two adults nearby)

2. **Home building TOO EASY**
   - Add wood requirement: 20 wood per home
   - Add construction time: 45 seconds with 1 worker
   - Collaborative building: 2+ workers = faster

3. **Population control**
   - Death rate should balance birth rate
   - Target: Stable 30-50 agents naturally
   - Old age deaths should be common

---

## 🚀 STRETCH GOALS (Way Future)

### Advanced Systems:
- **Seasons** - Summer (food abundant), Winter (scarce)
- **Disease** - Contagion spread, quarantine zones
- **War** - Territory disputes, raiders
- **Religion** - Shrines, rituals, beliefs
- **Tech Tree** - Tools → Agriculture → Industry
- **Trade Routes** - Caravans between towns

---

## 📝 GIT COMMIT CHECKLIST

Before each session ends:
```bash
git add .
git commit -m "Detailed message"
git push
```

---

## 🎬 NEXT SESSION START

1. Load this file
2. Check what's completed
3. Start with highest priority incomplete item
4. Update this plan as you go

**Session Goal:** Complete top 3 priority items minimum

---

Last Updated: [Current Date]
Session Progress: 6/15 core systems complete
