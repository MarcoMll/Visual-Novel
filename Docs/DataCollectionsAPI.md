# Runtime Data Collections API

This guide explains how to work with the runtime data containers that keep track of the player's state during the game.
Each container updates in real time but only writes to disk when `Save` method is called (or when the `GameDataManager` tells it to).

The system uses `BaseSO` assets with GUIDs so saved data can be turned back into references after loading.

## BaseSO Assets

`ItemSO`, `CharacterSO`, and `TraitSO` inherit from `BaseSO`.
A GUID is assigned automatically and registered at [SaveSystemIntegration.md](SaveSystemIntegration.md)runtime.
You can look up an asset by ID:

```csharp
var sword = BaseSO.GetByGuid<ItemSO>(guid);
```

## Inventory

`Inventory` keeps a list of `ItemSO` references.

```csharp
Inventory inventory = new Inventory();

// Add and remove items
inventory.AddItem(sword);
inventory.RemoveItem(sword);

// Query
bool hasSword = inventory.HasItem(sword);

// Saving
inventory.Save(); // store GUIDs
inventory.Load(); // rebuild list from GUIDs
```

## CharacterDatabase

`CharacterDatabase` tracks relationship values for characters.

```csharp
CharacterDatabase characters = new CharacterDatabase();

characters.RegisterCharacter(npc);
characters.ModifyRelationship(npc, +5);
int value = characters.GetRelationship(npc);

characters.Save();
characters.Load();
```

## TraitCollection

`TraitCollection` stores active traits.

```csharp
TraitCollection traits = new TraitCollection();

traits.AddTrait(braveTrait);
bool hasBrave = traits.HasTrait(braveTrait);

traits.Save();
traits.Load();
```

## FlagCollection

`FlagCollection` holds boolean flags identified by a string key.

```csharp
FlagCollection flags = new FlagCollection();

flags.SetFlag("met_villager", true);
bool met = flags.GetFlag("met_villager");
flags.RemoveFlag("met_villager");

flags.Save();
flags.Load();
```

## GameDataManager

To save or load everything at once, register the containers with a `GameDataManager` component.

```csharp
public class SaveController : MonoBehaviour
{
    [SerializeField] private GameDataManager manager;

    public void OnSaveClicked() => manager.SaveGame();
    public void OnLoadClicked() => manager.LoadGame();
}
```

Attach `SaveController` and assign the `GameDataManager` in the inspector. The manager calls `Save` and `Load` on each registered data section.

## Summary

- Collections update instantly and expose easy methods to read or modify data.
- Nothing is written to disk until you call `Save` on the collection or via `GameDataManager`.
- Assets derived from `BaseSO` carry GUIDs so saved references can be rebuilt after loading.
