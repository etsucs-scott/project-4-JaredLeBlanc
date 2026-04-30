# SkyjoLike Card Game

This project is a digital version of the card game 
mimicking Skyjo, built using C# and Avalonia UI.

## Game Rules
```bash
WIN CONDITIONS
- When one player flips their last cards, it calls for the end of the game.
  The other player has one last turn. Any un-flipped cards will flip 
  automatically.

- The sum of each players cards will be calculated and the player with the
  lowest total wins(You want lower cards to have the smallest sum)

GAMEPLAY
- Two players
- Cards range from -2 to 12 (the lower the cards the better)
- Each player is dealt 12 cards(all face down)
- Each player starts by flipping any two of their cards (Setup)
- Players have 2 options at the beginning of each turn: 
	1. Choose the discarded card and replace it with any card on your board
	2. Draw random card, then either play the card anywhere or discard it
	   and flip any un-flipped card.
  This cycle is repeated every turn until one player flips their final
  un-flipped card.
```
## Build and Run

```bash
git clone https://github.com/etsucs-scott/project-4-JaredLeBlanc.git
cd project-4-JaredLeBlanc
dotnet build
dotnet run --project src/SkyjoAvaloniaApp.csproj

if Avalonia not installed you can try:
dotnet new install Avalonia.Templates
```

## Run Unit Test
```bash
IN VISUAL STUDIO
1. Open **Test Explorer**
2. Click **Run All Tests**
```

## Tech Stack
```bash
- C#
- .NET
- Avalonia UI (cross-platform UI framework)
- xUnit (unit testing framework)
```

## Data Storage
```bash
Game data is saved locally as a JSON file:

- `save.json`

Handled by:
- `SaveService`
- `GameState`
- `PlayerState`
```

## UML Diagram
```bash
The UML diagram includes:

- Core game logic (`GameManager`, `Player`, `Card`, `Deck`)
- UI layer (`GameViewModel`, `CardViewModel`)
- Persistence (`SaveService`, `GameState`)

File included:
- 'Skyjo_UML.png'
```

## Submission Note
```bash
Repository: https://github.com/etsucs-scott/oop-26s-project-4-1260-project-starter
```

## External Resources
```bash
Link: https://avaloniaui.net/
Link: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/xaml/
Link: https://avaloniaui.net/blog/introduction-to-xaml-a-beginner-s-guide
```