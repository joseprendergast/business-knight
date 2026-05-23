# Business Night

Temporary working title for a new cinematic point-and-click adventure framework.

This project is intentionally story-neutral. It does not carry forward characters, lore, dialogue, puzzles, or locations from earlier notes. The current rooms and text are placeholders whose job is to prove the technical foundation before a full GDD arrives.

## Open the Unity Project

1. Open Unity Hub.
2. Add the project folder: `/Users/jprendergast/Documents/Business Knight/BusinessNightUnity`
3. Open with Unity `6000.4.7f1` or newer Unity 6.
4. Import PowerQuest into the project when ready. The current framework compiles without PowerQuest and includes `BusinessNightPowerQuestBridge` so room changes can be routed through PowerQuest after import.

## Run the Game

1. Open `Assets/Game/Rooms/RoomTitle.unity`.
2. Press Play.
3. Choose `New Game`.
4. In the prototype room:
   - Left click the hotspot for contextual interaction.
   - Right click the hotspot to inspect.
   - Select the collected inventory item in the bottom strip, then left click the hotspot again to test item-use and room transition.

## Build WebGL

The project is set up for a static WebGL build.

In Unity:

1. Open `File > Build Profiles`.
2. Select `Web`.
3. Confirm these scenes are enabled:
   - `Assets/Game/Rooms/RoomTitle.unity`
   - `Assets/Game/Rooms/RoomPrototypeA.unity`
   - `Assets/Game/Rooms/RoomPrototypeB.unity`
   - `Assets/Game/Rooms/RoomPrototypeC.unity`
4. Build to `Builds/WebGL`.

The editor helper can also regenerate the framework from `Business Night > Build Placeholder Framework`.

## Project Structure

- `Assets/Game/Rooms`: PowerQuest-compatible room scenes and placeholder Unity scenes.
- `Assets/Game/Characters`: reserved for future character controllers and portraits.
- `Assets/Game/Inventory`: reserved for future item/evidence assets.
- `Assets/Game/UI`: reserved for reusable UI prefabs and art.
- `Assets/Game/Scripts`: core runtime systems.
- `Assets/Game/Audio`: room tone, ambience, stings, and UI sounds.
- `Assets/Game/Art`: placeholder pixel backgrounds and future room layers.
- `Assets/Game/Debug`: runtime debug tools.
- `Assets/Game/Atmosphere`: reusable atmosphere components.
- `Assets/StreamingAssets`: available for optional data-driven content.

## Core Systems

- `BusinessNightGlobals`: persistent flags, visited scenes, collected items, settings, and save capture.
- `BusinessNightSceneManager`: new game, continue, autosave, and room transitions.
- `BusinessNightUi`: title/menu layer, hotspot label, subtitles, fades, inventory strip, debug display.
- `BusinessNightInventory`: item catalog, collection, inspection data, and selected item state.
- `BusinessNightDialogue`: cinematic subtitle playback with one-time flags and typewriter support.
- `BusinessNightHotspot`: inspect, interact, use item, set flags, collect item, trigger subtitles, and change room.
- `BusinessNightSettings`: browser-safe audio unlock and volume/mute settings.
- `BusinessNightDebug`: scene jumps, item grant, reset progress, and UI debug toggle.
- `BusinessNightPowerQuestBridge`: optional reflection bridge so the framework can compile before PowerQuest is imported.

## Rooms

Each room is still a Unity scene because PowerQuest rooms should remain the core room unit.

Starter rooms:

- `RoomTitle`
- `RoomPrototypeA`
- `RoomPrototypeB`
- `RoomPrototypeC`

Each playable room has a `BusinessNightRoom` component with a `BusinessNightSceneDefinition`:

- scene id
- display name
- description
- background/art reference
- characters present
- ambient effects
- narrative beats
- required flags
- completion flag
- debug jump support

When the GDD arrives, rename these scenes and update the definitions instead of rewriting the framework.

## Hotspots

Use `BusinessNightHotspot` for neutral data-driven behavior:

- `displayName`: compact hover label.
- `inspectLine`: right-click inspection subtitle.
- `interactLine`: default contextual action.
- `requiredFlags`: locks interaction until flags exist.
- `setFlags`: flags set after interaction.
- `collectItemId`: item granted by the hotspot.
- `requiredItemId`: inventory item needed for item-use.
- `roomChangeSceneId`: scene to load after successful interaction.
- `dialogueBeat`: cinematic subtitle beat.

PowerQuest hotspot scripts can call these methods directly or mirror the same fields once PowerQuest rooms are authored.

## Flags

Flags live in `BusinessNightGlobals`.

Current placeholder flags:

- `m_gameStarted`
- `m_seenOpeningBeat`
- `m_collectedFirstItem`
- `m_unlockedSecondRoom`
- `m_talkedToFirstCharacter`
- `m_sceneOneComplete`

Use story, puzzle, and dialogue buckets to keep later GDD logic readable. Replace placeholder flags with final naming once real scenes and puzzles exist.

## Inventory and Case File

The inventory is deliberately neutral. It can become `Inventory`, `Case File`, `Evidence`, `Documents`, or another GDD-specific frame later.

Current support:

- item collection
- item selection
- item inspection text
- using selected item on a hotspot
- hidden/disabled catalog entries
- save/load persistence

Add new items to the `catalog` list on `BusinessNightInventory` or migrate the catalog to ScriptableObjects when the item list becomes large.

## Save and Load

Save data includes:

- current room
- current chapter
- visited scenes
- story flags
- puzzle flags
- dialogue flags
- collected items
- settings

The save file is written to Unity's `Application.persistentDataPath`, which maps correctly for WebGL browser storage.

## Debug Tools

During development:

- `F1`: toggle debug panel.
- `1`: jump to title.
- `2`: jump to prototype room A.
- `3`: jump to prototype room B.
- `4`: jump to prototype room C.
- `G`: grant prototype item.
- `R`: reset save and progress.

These can be hidden or compiled out for production later.

## Replacing Placeholder Content With the GDD

When the real GDD arrives:

1. Rename rooms from `RoomPrototype*` to real location names.
2. Replace placeholder backgrounds with layered room art.
3. Replace placeholder hotspot text with final inspect, interact, talk, and item-use beats.
4. Replace placeholder flags with final story/puzzle/dialogue flags.
5. Move larger item and dialogue catalogs into ScriptableObjects or PowerQuest-native data.
6. Route room changes through PowerQuest using `BusinessNightPowerQuestBridge`.
7. Keep the interaction model contextual unless the GDD explicitly calls for a visible verb bar.

## Publishing

For GitHub Pages or similar static hosting:

1. Build WebGL to `Builds/WebGL`.
2. Publish the contents of that folder.
3. Keep compression disabled unless the host is configured to serve Unity's compressed files with the correct headers.
4. Do not rely on audio autoplay. The framework unlocks audio only after player input, which is browser-safe.
