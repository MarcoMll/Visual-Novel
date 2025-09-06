# Save System Integration Guide

This guide explains how to integrate the runtime save system into your game.

## Overview

The save system is composed of three core components:

- **BaseGameData**: An abstract class for any serializable data section. It handles JSON serialization to Unity `PlayerPrefs` and defines a unique save key.
- **PlayerGameData** and **GraphGameData**: Concrete data containers that store player information (items, traits, relationships) and graph progression (current node, flags).
- **GameDataManager**: A MonoBehaviour that orchestrates saving and loading for all registered data sections.

## Setup

1. **Create Data Sections**
   - Extend `BaseGameData` to define new data sections.
   - Give each data section a unique `SaveKey` string.

```csharp
[Serializable]
public class InventoryGameData : BaseGameData
{
    public List<string> Items = new();
    protected override string SaveKey => "INVENTORY_DATA";
}
```

2. **Register Data Sections**
   - Add a `GameDataManager` component to a scene object.
   - In the inspector, populate the `dataSections` list with instances of your data containers.

3. **Trigger Save and Load**
   - Call `SaveGame()` or `LoadGame()` on the `GameDataManager` when appropriate (e.g., when the player clicks a save button).

```csharp
public class SaveMenu : MonoBehaviour
{
    [SerializeField] private GameDataManager manager;

    public void OnSaveClicked() => manager.SaveGame();
    public void OnLoadClicked() => manager.LoadGame();
}
```

## Adding New Sections

To add more data, create another class inheriting from `BaseGameData` and add it to the manager's list. The system will automatically serialize it along with the others.

```csharp
[Serializable]
public class StatsGameData : BaseGameData
{
    public int Level;
    public int Experience;
    protected override string SaveKey => "STATS_DATA";
}
```

Register `StatsGameData` in the `GameDataManager` to include it in save and load operations.

## Summary

By using `GameDataManager` and extending `BaseGameData`, you can organize all game state persistence in a flexible and extensible way, keeping runtime data centralized and easy to maintain.

