# Implementation Plan - UI & Interaction System Cleanup

This plan addresses the user's request to remove the legacy world-space debugger and improve the main UI panel system.

## User Review Required
> [!NOTE]
> **ChickenDebugger Removal**: The `ChickenDebugger` component and its associated world-space canvas will be removed. All chicken stats will now be viewed exclusively through the main UI panels.

## Proposed Changes

### Legacy Removal
#### [DELETE] [ChickenDebugger.cs](file:///C:/Users/Joan/Desktop/++ZOMBIES_A++/Unity/Zombies_A/My%20project/Assets/Scripts/Chicken/ChickenDebugger.cs)
- Remove this script entirely as it's no longer needed.

#### [MODIFY] [Chicken.cs](file:///C:/Users/Joan/Desktop/++ZOMBIES_A++/Unity/Zombies_A/My%20project/Assets/Scripts/Chicken/Chicken.cs)
- Remove `OnMouseDown` method that called `ChickenDebugger`.
- Ensure `IInteractable` implementation is robust.

#### [MODIFY] [SceneSetupTool.cs](file:///C:/Users/Joan/Desktop/++ZOMBIES_A++/Unity/Zombies_A/My%20project/Assets/Scripts/Editor/SceneSetupTool.cs)
- Remove `ChickenDebugger` addition and configuration from `CreateChickenPrefab`.
- Remove `CreateStatsPanel` method.

### UI Improvements
#### [MODIFY] [PanelManager.cs](file:///C:/Users/Joan/Desktop/++ZOMBIES_A++/Unity/Zombies_A/My%20project/Assets/Scripts/Core/PanelManager.cs)
- Review `maxPanelsPerLane` setting (default is 5, which should allow multiple panels).
- Ensure `leftLaneContainer` has a Vertical Layout Group to stack panels correctly.

## Verification Plan

### Manual Verification
1.  **Debugger Removal**: Click on a chicken. Verify NO world-space panel appears above it.
2.  **UI Panel**: Verify a panel appears in the Left Lane showing chicken stats.
3.  **Multiple Panels**: Click on multiple different chickens. Verify multiple panels stack in the Left Lane (up to the limit).
