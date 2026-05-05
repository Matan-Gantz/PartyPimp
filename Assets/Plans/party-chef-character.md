# Project Overview
- Game Title: Party Chef Prototype
- High-Level Concept: A stylized 3D character based on the "Party Chef" concept art, ready for a player-controlled environment.
- Players: Single player (Player Character)
- Inspiration / Reference Games: Animal Crossing, My Time at Portia (for the character style).
- Tone / Art Direction: Stylized, clean, "Vinyl Toy" aesthetic.
- Target Platform: PC / Standalone
- Render Pipeline: Built-in (based on project settings)

# Game Mechanics
## Core Gameplay Loop
The player controls the "Party Chef" character to navigate a social environment, interacting with food and objects.
## Controls and Input Methods
- WASD for movement.
- Space for jumping (if applicable).
- Standard Character Controller logic.

# UI
- Minimalist HUD (not part of this asset generation task).

# Key Asset & Context
- **Reference Image**: `Assets/PartyChef_Concept.png` (InstanceID: 56982).
- **Extracted Views**: Sliced images for Front, Side, and Back views from the concept art.
- **3D Model**: `Assets/Models/PartyChef.glb` (Generated via Tripo P1 Multi-view).
- **Rigged Prefab**: `Assets/Prefabs/PartyChef_Player.prefab` with Animator and Collision.

# Implementation Steps
## Phase 1: Image Processing
1. **Slice Turnaround Views**: Use a script to extract the Front, Side, and Back views from the "TURNAROUND" section of the concept art.
2. **Remove Backgrounds**: Ensure the extracted sprites have clean backgrounds for the 3D generator.

## Phase 2: 3D Asset Generation
1. **Generate Mesh**: Use `model3d-tripo-p1-multiview` with the three extracted views to create a high-fidelity 3D model with PBR textures.
2. **Rigging**: Use `model3d-tripo-rigging-v1` to automatically rig the character for humanoid movement.
3. **Generate Animations**: Generate Idle, Walk, and Run animation clips.

## Phase 3: Unity Integration & Wiring
1. **Import Assets**: Move the generated mesh, textures, and animations into the project.
2. **Create Prefab**: Assemble the `PartyChef_Player` prefab.
3. **Setup Physics**: Add a `CapsuleCollider` and `Rigidbody` (or `CharacterController`).
4. **Setup Animator**: Create an `Animator Controller` and wire the Idle/Walk/Run states.
5. **Collider Fitting**: Use the `auto-wire-asset` logic to ensure the collider matches the character's bounds.

# Verification & Testing
1. **Visual Check**: Inspect the model in the Scene view for texture quality and mesh integrity.
2. **Animation Test**: Play the Idle, Walk, and Run animations in the Inspector.
3. **Physics Test**: Drop the character into a test scene to verify it interacts with the ground and doesn't "float" or "clip".
