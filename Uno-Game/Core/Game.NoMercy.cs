using System.Numerics;
using Uno_Game.Cards;
using Uno_Game.Cards.NoMercyExpansion;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

public partial class Game
{
    private readonly HashSet<Player> eliminatedPlayers = new();
    private readonly List<(Player Player, NoMercyCoin Coin)> noMercyCoins = new();

    public IReadOnlyList<(Player Player, NoMercyCoin Coin)> NoMercyCoins => noMercyCoins;
    public bool IsPlayerEliminated(Player player) => eliminatedPlayers.Contains(player);

    private void InitializeNoMercyCoins()
    {
        if (Version != GameVersions.NoMercy || !Packs.HasFlag(ExpansionPacks.NoMercy))
            return;

        foreach (Player player in players)
        {
            NoMercyCoin coin = player.ChooseCoin(this);
            if (coin.Type != Coins.Mercy && coin.Type != Coins.NoMercy)
                throw new InvalidOperationException($"{player.Name} chose an invalid No Mercy coin.");

            noMercyCoins.Add((player, coin));
            ui.Show($"{player.Name} selected a {coin}.", MessageStyle.Event);
        }
    }

    private NoMercyCoin? GetCoin(Player player, Coins type) =>
        noMercyCoins
            .Where(entry => entry.Player == player && entry.Coin.Type == type)
            .Select(entry => entry.Coin)
            .FirstOrDefault();

    private void UseMercyCoinAtTurnStart(Player player)
    {
        NoMercyCoin? coin = GetCoin(player, Coins.Mercy);
        if (coin == null || coin.IsUsed || !player.ShouldUseMercyCoin(this))
            return;

        if (coin.TryUse())
            ReplaceHandWithSevenCards(player, "uses their Mercy coin");
    }

    private void ReplaceHandWithSevenCards(Player player, string reason)
    {
        foreach (Card card in player.Hand)
            Deck.Discard(card);

        player.Hand.Clear();
        for (int i = 0; i < InitialHandSize; i++)
            player.Hand.Add(Deck.Draw());

        ClearUnoAnnouncement(player);
        ui.Show($"{player.Name} {reason} and receives a new hand of 7 cards.", MessageStyle.Event);
    }

    public int ApplyNoMercyCoin(Player player, int drawCount)
    {
        if (drawCount <= 0 || Version != GameVersions.NoMercy
            || !Packs.HasFlag(ExpansionPacks.NoMercy))
            return drawCount;

        NoMercyCoin? coin = GetCoin(player, Coins.NoMercy);
        if (coin == null || coin.IsUsed || !player.ShouldUseNoMercyCoin(this, drawCount))
            return drawCount;

        if (!coin.TryUse())
            return drawCount;

        int doubledDrawCount = checked(drawCount * 2);
        ui.Show($"{player.Name} uses their No Mercy coin: the draw penalty doubles to {doubledDrawCount}.",
            MessageStyle.Event);
        return doubledDrawCount;
    }

    private void CheckMercyRule(Player player)
    {
        if (Version != GameVersions.NoMercy
            || player.Hand.Count <= 24
            || eliminatedPlayers.Contains(player))
            return;

        NoMercyCoin? coin = GetCoin(player, Coins.Mercy);
        if (coin != null && coin.TryUse())
        {
            ReplaceHandWithSevenCards(player, "automatically uses their Mercy coin");
            return;
        }

        eliminatedPlayers.Add(player);
        ui.Show($"{player.Name} has more than 24 cards and is eliminated.", MessageStyle.Warning);
        ui.Pause(MicroDelayMs);

        List<Player> activePlayers = players
            .Where(candidate => !eliminatedPlayers.Contains(candidate))
            .ToList();

        if (activePlayers.Count == 1)
            DeclareWinner(activePlayers[0]);
    }

    public void ColorRoulette()
    {
        AdvanceTurn();
        Player victim = CurrentPlayer;
        ChooseColor();
        CardColor color = CurrentColor;
        DrawUntilColor(victim, color, extraCards: 0);
        SkipFrom(victim, 1);
    }

    public void FinalAttack()
    {
        Player attacker = CurrentPlayer;
        Player victim = NextPlayer();

        ui.Show($"{attacker.Name} uses final attack!", MessageStyle.Event);
        ui.ShowHands(new List<(string PlayerName, IReadOnlyList<Card> Cards)> { (attacker.Name, attacker.Hand.ToList()) });

        int specialCardsCount = attacker.Hand.Where(card => card is not NumberCard).Count();

        if(specialCardsCount < 7)
        {
            ui.Show($"{victim.Name} draws {specialCardsCount} card(s)", MessageStyle.Event);
            DrawCards(victim, specialCardsCount);
        }
        else
        {
            ui.Show($"{victim.Name} draws 25 card(s)", MessageStyle.Event);
            DrawCards(victim, 25);

            foreach (Player p in players)
            {
                if (Winner != null)
                    return;

                if (p == attacker || p == victim || IsPlayerEliminated(p))
                    continue;

                ui.Show($"{p.Name} draws 5 card(s)", MessageStyle.Event);
                DrawCards(p, 5);
            }
        }
    }

    public void SuddenDeathProtocol()
    {
        ui.Show("Everyone draw cards until have 24", MessageStyle.Warning);

        foreach (Player player in players)
        {
            if (Winner != null)
                return;

            if (IsPlayerEliminated(player))
                continue;

            int drawCount = Math.Abs(player.Hand.Count() - 24);
            DrawCards(player, drawCount);
        }
    }
}
