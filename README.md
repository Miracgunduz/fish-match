# Fish Match

A match-3 puzzle game where every match feeds and grows your own living aquarium. Clear gems on the board to earn currency, unlock new fish species, decorate your tank, and progress through a chapter-based level map.

Built with **Unity 6** and C#.

## Gameplay Loop

1. **Match gems** on the Match-3 grid to earn currency and progress.
2. **Unlock fish** and watch your collection swim around a persistent aquarium.
3. **Feed your fish** to keep them happy and growing.
4. **Decorate the tank** with coral, seaweed, and pearls bought from the decoration shop.
5. **Progress through chapters** on the level map, unlocking new levels and rewards.
6. **Track your collection** in the Fish-Dex — see every species you've discovered and your completion progress.

## Features

- **Match-3 core** — grid-based match detection and resolution (`Match3Grid`, `Match3GridController`)
- **Living aquarium** — unlocked fish swim persistently in your tank (`AquariumManager`, `AquariumFishController`)
- **Fish collection (Fish-Dex)** — species definitions, ownership tracking, and per-fish progress (`FishDexController`, `FishCollectionManager`, `FishOwnershipRegistry`, `FishPieceProgress`)
- **Currency & shop** — earn currency from matches, spend it on aquarium decorations (`CurrencyManager`, `DecorationShopController`)
- **Feeding mechanic** — keep your fish fed and growing (`FishFeedingController`, `FeedingState`)
- **Level map & chapters** — structured progression across multiple chapters (`LevelMapController`, `LevelChapters`, `LevelRules`)
- **Save/load system** — persistent player progress (`SaveManager`, `SaveData`)
- **Unit-tested game logic** — EditMode tests cover match detection, currency, ownership registries, feeding state, and level/chapter rules

## Project Structure

```
Assets/
├── Art/              Sprites and UI art (Kenney asset packs + custom shapes)
├── Editor/           Editor tooling used to scaffold scenes and prefabs
├── Prefabs/          Aquarium, Match-3, and decoration prefabs
├── Scenes/           MainMenu, Match3Test, Aquarium, FishDex, LevelMap
├── ScriptableObjects/ Fish species and decoration definitions (data-driven content)
├── Scripts/Match3/   Core gameplay, aquarium, collection, shop, and save systems
└── Tests/EditMode/   Unit tests for grid logic, matching, currency, ownership, feeding, and level rules
```

## Getting Started

1. Install [Unity Hub](https://unity.com/download) and Unity **6000.5.5f1** (or later Unity 6 release).
2. Clone this repository.
3. Open the project folder in Unity Hub.
4. Open `Assets/Scenes/MainMenu.unity` and press Play.

## Tech Stack

- Unity 6 (6000.5.5f1)
- C#
- Unity Test Framework (EditMode tests)
