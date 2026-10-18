using Uno_Game.Cards;

namespace Uno_Game.Players;

/// <summary>What a player decides when playing a Swap 1 card.</summary>
public readonly record struct SwapChoice(Player Target, Card CardToGive);
