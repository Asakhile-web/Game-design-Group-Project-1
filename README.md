# Game-design-project
learning how to install

## In-game HUD and pause menu

Built at runtime by `PlayerController` (no scene setup needed): `GameHUD` (objective/timer, pause, journal and inventory buttons, notifications) and `GameMenu` (pause, inventory, journal, settings, confirm dialogs).

| Key | Action |
| --- | --- |
| Esc / Start | Pause, or go back one panel |
| Tab / I | Inventory |
| J | Clue journal |
| Hold Alt | Free the cursor to click HUD buttons |

- Opening any panel pauses the game (`Time.timeScale = 0`, audio paused, player input off). Closing a panel opened from the pause menu goes back to the pause menu.
- Clues: add `ClueInteractable` to an object and assign a `ClueDefinition` (Create > Writers Study > Clue Definition).
- Objectives: call `ObjectiveTracker.SetObjective` / `CompleteObjective` (e.g. from a `SimpleInteractable` UnityEvent), or set the starting objective on the player's `ObjectiveTracker`.
- The Main Menu button appears only if a scene named `MainMenu` is in Build Settings.
