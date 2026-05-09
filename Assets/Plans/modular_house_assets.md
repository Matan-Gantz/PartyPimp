# Project Overview
- Game Title: PartyPimp
- High-Level Concept: A stylized, isometric, top-down party simulation game where players manage an indoor party environment.
- Players: Single player (implied by typical simulation games)
- Inspiration / Reference Games: Overcooked, cozy mobile simulation games.
- Tone / Art Direction: Stylized semi-realistic cartoon, soft rounded geometry, chunky proportions, clean silhouettes.
- Target Platform: Standalone (MacOS/PC)
- Screen Orientation / Resolution: Landscape (Standard PC)
- Render Pipeline: Built-in

# Game Mechanics
## Core Gameplay Loop
- Players set up a house environment (Living Room, Kitchen, Bathroom) and manage party-related stations (Food, Drinks, Music).
- The environment must be modular to allow for dynamic placement and clear navigation.
## Controls and Input Methods
- Top-down isometric movement.
- Grid-based placement (1m x 1m).

# UI
- (Out of scope for this task)

# Key Asset & Context
- **Floor Materials**:
    - `Mat_Floor_LivingRoom`: Warm brown tiles, subtle grout.
    - `Mat_Floor_Kitchen`: Neutral gray tiles.
    - `Mat_Floor_Bathroom`: Darker gray/blue tiles.
- **Modular Wall Set**:
    - `Mesh_Wall_1x1`: 1m x 2.5m (approx height), chunky, painted brick texture, rounded edges.
    - `Mesh_Wall_2x1`: 2m wide version.
- **Door Prefab**:
    - `Mesh_Door_Frame`: Wooden frame, soft edges.
    - `Mesh_Door_Panel`: Simple wooden door, beveled, warm brown.
- **Pivot Policy**: Bottom-center for all modules to facilitate grid snapping.

# Implementation Steps
## Phase 1: Floor Materials
1. Generate stylized tile textures using `GenerateAsset` with a prompt focusing on "hand-painted stylized, clean tiles, subtle grout, warm brown/neutral gray".
2. Create `Material` assets in `Assets/Materials/Environment`.
3. Apply to 1x1 plane meshes for testing.

## Phase 2: Modular Wall Meshes
1. Generate a "stylized modular wall" mesh using `GenerateAsset`. Prompt: "Stylized interior wall segment, 1m wide, chunky proportions, rounded beveled edges, painted brick texture, clean silhouette, Overcooked style".
2. Ensure the pivot is set to the bottom center.
3. Create `Wall_1x1` and `Wall_2x1` prefabs in `Assets/Prefabs/Environment`.

## Phase 3: Door Prefab
1. Generate a door frame and door panel mesh. Prompt: "Stylized wooden door and frame, chunky rounded edges, simple bevel, warm brown wood, cozy simulation style".
2. Assemble into a `Door_Simple` prefab in `Assets/Prefabs/Environment`.
3. Set up appropriate colliders (BoxCollider).

## Phase 4: Verification
1. Place assets in the `Prototype` scene.
2. Verify they snap to the 1m grid defined in `GridManager`.
3. Check readability from the isometric camera.

# Verification & Testing
- **Visual Check**: Assets must match the "Overcooked" aesthetic (chunky, rounded, saturated colors).
- **Grid Check**: Drag assets into the scene; they should align perfectly when moved in 1-unit increments.
- **Navigation Check**: Ensure wall segments have colliders that don't block the grid cells unnecessarily.
