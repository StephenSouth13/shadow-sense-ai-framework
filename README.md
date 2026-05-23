# Invincible: Xeno Swarm - High-Speed Vertical Combat & Swarm AI Framework

## 1. Project Overview
<b>Invincible: Xeno Swarm</b> is a professional-grade Unity framework designed for developers building high-octane, 3D vertical action games. Inspired by the kinetic energy of "Invincible," this package provides a complete ecosystem for omni-directional flight combat, advanced spatial AI, and procedural sandbox generation.

Whether you are building a superhero epic, a sci-fi dogfighter, or a swarm-based survival game, this framework delivers the performance and modularity needed for a commercial release.

---

## 2. Work Report: Technical Component Breakdown

### <b>Core AI Systems</b>
*   <b>RobotBrain FSM (Ground AI)</b>: A robust Finite State Machine (Patrol, Chase, Investigate) integrated with Unity's NavMesh. Includes built-in Animator bridging for "Monster07" style entities with smooth crossfades between locomotion and combat.
*   <b>A* 3D Pathfinding & Waypoint Network</b>: A custom-built, non-NavMesh pathfinding solution for open-air navigation. It allows flying entities to calculate optimal routes through a 3D grid of floating nodes.
*   <b>SteeringAgent (Flight Physics)</b>: A high-performance steering engine providing organic mid-air movement. Features include:
    *   <b>Whisker-based Obstacle Avoidance</b>: Procedural dodging of skyscrapers.
    *   <b>Separation</b>: Flocking algorithms to maintain swarm formation.
    *   <b>Wander</b>: Perlin-noise based organic jitter for cinematic hovering.
*   <b>ViltrumiteFlightController</b>: The "Apex Brain" for flying interceptors, seamlessly blending global A* navigation with local steering behaviors for relentless player pursuit.

### <b>Player & Combat</b>
*   <b>PlayerCombatController</b>: A specialized 3D flight controller utilizing the <b>New Input System</b>. Supports omni-directional movement, vertical ascent/descent, and high-speed boosting.
*   <b>IDamageable Interface</b>: A fully decoupled combat architecture. Allows the player and AI to interact through a standardized damage event system, making it easy to add new enemy types or destructible environments.

### <b>Infrastructure & Tools</b>
*   <b>SectorSandboxGenerator</b>: An automated level design tool. With a single click, it generates 200x200 arenas with procedural buildings (obstacles), bakes NavMeshes, establishes waypoint networks, and spawns the dynamic entities.
*   <b>AudioManager (Singleton)</b>: A persistent, cross-scene audio engine supporting smooth BGM crossfading and 3D spatial sound effects.
*   <b>MainMenuController</b>: Production-ready UI logic for asynchronous scene loading and application management.

---

## 3. Step-by-Step Setup Instructions

### <b>1. Initial Arena Generation</b>
1.  Open the scene `Sector_Zero_Arena` (or a fresh scene).
2.  Locate the `SectorSandboxGenerator` component in the Inspector.
3.  Click the <b>"Generate Sector"</b> context menu button (or run from the inspector). This will build the buildings, ground, and waypoint networks.

### <b>2. Baking Navigation</b>
1.  Select the <b>City_Ground</b> object inside the `--- DYNAMIC_ARENA ---` folder.
2.  Ensure a `NavMeshSurface` component is attached.
3.  Click <b>Bake</b> in the AI Navigation window to ensure Ground AI (Xeno-Stalkers) can navigate around the generated buildings.

### <b>3. Setting up Persistence</b>
1.  Locate the <b>[SYSTEM_PERSISTENT]</b> prefab/GameObject.
2.  In the `AudioManager` component, assign your desired `AudioClips` for BGM and SFX.
3.  This object will persist across all scene loads, ensuring uninterrupted music and sound management.

### <b>4. Build Settings</b>
1.  Go to <b>File > Build Settings</b>.
2.  Add your Main Menu scene and name it exactly `MainMenu_Scene`.
3.  Add the generated combat arena and name it exactly `Gameplay_Scene`.
4.  The `MainMenuController` will now correctly load the combat sector asynchronously.

---

## 4. Architecture Highlights
*   <b>Decoupled Logic</b>: Using the `IDamageable` interface ensures that the Player doesn't need to know "what" it is hitting, only that it can be damaged.
*   <b>Optimized Spatial Queries</b>: AI detection and steering use `Physics.OverlapSphere` and optimized raycasts to ensure high frame rates even with dozens of active agents.
*   <b>Commercial Coding Standards</b>: All scripts follow industry-standard PascalCase naming, are heavily commented, and utilize `[SerializeField]` and `[Header]` attributes for a clean Inspector experience.
*   <b>New Input System Ready</b>: Fully integrated with the latest Unity Input System for modern controller and keyboard/mouse support.

---
<b>Version</b>: 1.0.0  
<b>Unity Compatibility</b>: Unity 6.0.x and higher  
<b>Dependencies</b>: AI Navigation Package, TextMeshPro, New Input System.