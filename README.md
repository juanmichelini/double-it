# double-it

A 2048-style game for Godot (3.x Mono) with the game logic written in F#.

- `Game2048.fs` – pure F# game logic (`moveLeft`, `sumEquals`, `move`, `sproutTiles`, win/lose checks)
- `Game2048Lib.fsproj` – builds the F# logic into `Game2048Lib.dll`
- `Game2048Node.cs` – small C# bridge script that drives the UI from the F# library
- `Game2048.tscn`, `Tile.tscn` – Godot scenes (expect `fonts/Roboto-Bold.ttf`, not included)

## Build

```
dotnet build Game2048Lib.fsproj
```

Copy `bin/Debug/net472/Game2048Lib.dll` into the Godot project root and reference it from the
Godot C# project:

```xml
<ItemGroup>
  <Reference Include="Game2048Lib">
    <HintPath>$(ProjectDir)/Game2048Lib.dll</HintPath>
  </Reference>
</ItemGroup>
```

## Status

Work in progress. The code targets Godot 3.x Mono APIs (`RectMinSize`, `Scancode`, `.mono`) and
has not been verified to build end to end.
