# Uno-Game

Juego de Uno por consola en C# (.NET 8). Jugador 1 eres tú; los otros 3 son NPC.

## Cómo ejecutarlo

```
dotnet run
```

Al iniciar eliges los packs de expansión (se activan/desactivan con su número) y luego `3) Start Game`.

## Reglas especiales

- **Reverse pack**: Reverse Skip, Reverse Draw 2, Wild Power Reverse (invierte y vuelves a jugar) y Wild No U
  (invierte al jugarla; si la tienes en la mano cuando te castigan con Draw/Skip, devuelve el castigo a quien lo jugó).
- **Stack pack**: Stack 1 y Stack 2. El siguiente jugador responde apilando mientras tenga una carta Stack;
  el primero que no pueda roba toda la pila y pierde el turno.
- Con 2 jugadores, Reverse funciona como Skip.

## Estructura

```
Program.cs            Punto de entrada (menú de packs + partida)
Core/
  Game.cs             Estado de la partida y operaciones que usan las cartas (Reverse, PunishNextPlayer, ResolveStack...)
  Deck.cs             Mazo + pila de descarte (se rebaraja sola) y composición del mazo por pack
  CardColor.cs        Colores
  ExpansionPacks.cs   Packs (flags)
Cards/                Una clase por carta; cada una sabe aplicar su efecto en Apply(Game)
  ReversePack/        Cartas del Reverse pack
  StackPack/          Cartas del Stack pack
Players/              Player (base), HumanPlayer (consola), NpcPlayer (estrategia)
UI/                   IGameUi (lo que el juego necesita de fuera), ConsoleUi, PackMenu
```

La lógica (`Core`, `Cards`, `Players`) no usa `Console` directamente: todo pasa por `IGameUi`.
Así se puede probar con una UI falsa (sin esperas ni teclado).

## Cómo agregar una carta nueva

1. Crea la clase en `Cards/` heredando de `Card`.
2. Implementa `Name` (texto sin color) y `Apply(Game game)` con su efecto, usando las operaciones de `Game`.
3. Agrégala en `Deck.CreateCards` (dentro del `if` del pack correspondiente).

No hay que tocar `Program.cs` ni `Game.PlayTurn`.

## Cómo agregar un pack nuevo

1. Ya existe su flag en `ExpansionPacks` (Swap, Speed, ...).
2. Agrega sus cartas a `Deck.CreateCards` bajo `if (packs.HasFlag(...))`.
3. Agrega su opción en `UI/PackMenu.cs`.
