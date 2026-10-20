namespace Uno_Game.Core;

[Flags]
public enum ExpansionPacks
{
    None = 0,
    Reverse = 1 << 0,
    Stack = 1 << 1,
    Swap = 1 << 2,
    Speed = 1 << 3,

    // Planned packs (not implemented yet)
    Frenemies = 1 << 4,
    Defense = 1 << 5,
    Comeback = 1 << 6,
    Billie = 1 << 7,
    NoMercy = 1 << 8
}
