# Project Overview
- Game Title: Party Pimp (Inferred from path)
- High-Level Concept: Managing guests at a party, fulfilling their needs, and now attracting them to music stations.
- Players: Single player (managing the party).
- Render Pipeline: Built-in.
- Input System: New Input System.

# Game Mechanics
## Core Gameplay Loop
The player manages a party. Guests wander around and have needs (Thirst, Hunger, Toilet). The player provides items or directed stations to fulfill these needs. The Music Station is a new element that creates a crowd by attracting guests who don't have urgent needs.

## Music Station Logic
- **Attraction**: The station detects guests within a `attractionRadius`.
- **Filtering**: It selects guests who are not currently "dancing" and don't have urgent needs (level > 90%).
- **Capping**: It limits the number of attracted guests to `maxGuests`.
- **Influence**: It provides a soft movement influence towards the station.

# Key Asset & Context
- `GuestAI.cs`: Main guest logic. Needs `Dancing` state and assignment logic.
- `MusicStation.cs`: New script inheriting from `Station`.

# Implementation Steps

## Step 1: Foundation (Current Task)
1. **Modify `GuestAI.cs`**:
   - Add `Dancing` to `GuestState` enum.
   - Add `public void AssignToMusicStation(MusicStation station)` (stores reference, sets state).
   - Add `public bool IsInUrgentNeed()` (checks if any need > 90%).
   - Add `public MusicStation CurrentMusicStation { get; private set; }`.
2. **Create `MusicStation.cs`**:
   - Variables: `attractionRadius`, `maxGuests`, `attractionStrength`.
   - List to track `attractedGuests`.
   - `Update()` logic:
     - Detect guests with `Physics.OverlapSphere`.
     - Remove guests who left the radius or became urgent.
     - Add new guests if space available and criteria met.
     - Call `guest.AssignToMusicStation(this)`.

## Step 2: Guest Behavior (Next Phase)
- Implement movement logic in `GuestAI.UpdateState` for `GuestState.Dancing`.
- Implement soft attraction blending in `GuestAI.Update`.
- Handle exit conditions (too far, urgent need).

# Verification & Testing
1. **Selection Test**: Add `Debug.Log` in `MusicStation` when a guest is added/removed. Verify the count never exceeds `maxGuests`.
2. **Radius Test**: Use `OnDrawGizmos` to visualize the attraction radius.
3. **Filter Test**: Increase a guest's need manually in the inspector and verify they are removed from the `MusicStation` list.
