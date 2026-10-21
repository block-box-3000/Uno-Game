namespace Uno_Game.Core;

[Flags]
public enum ExpansionPacks
{
    None = 0,
    Reverse = 1 << 0,
    Stack = 1 << 1,
    Swap = 1 << 2,
    Speed = 1 << 3,
    Frenemies = 1 << 4,
    Billie = 1 << 7,

    // Planned packs (not implemented yet)
    Defense = 1 << 5,
    Comeback = 1 << 6,
    NoMercy = 1 << 8
}
