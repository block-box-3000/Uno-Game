using Uno_Game.Cards;
using Uno_Game.Cards.DefensePack;
using Uno_Game.Cards.FrenemiesPack;
using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

/// <summary>
/// All the state of one match (players, deck, direction, whose turn it is) and the
/// operations cards use to change it. Cards call these methods from <see cref="Card.Apply"/>.
/// </summary>
public partial class Game
{
    public const int InitialHandSize = 7;
    private const int TurnDelayMs = 2000;
    private const int ShortDelayMs = 1500;

    private readonly IGameUi ui;
    private readonly Random rng;
    private readonly List<Player> players;
    private int currentIndex;
    private bool repeatTurn;

    public IReadOnlyList<Player> Players => players;
    public Deck Deck { get; }
    public ExpansionPacks Packs { get; }

    public Card TopCard { get; private set; }

    /// <summary>The color that must be matched. For wild cards it is the color the player chose.</summary>
    public CardColor CurrentColor { get; private set; }

    /// <summary>1: clockwise, -1: anticlockwise.</summary>
    public int Direction { get; private set; } = 1;

    public Player CurrentPlayer => players[currentIndex];
    public Player? Winner { get; private set; }

    /// <summary>The version of the game: normal Uno (with expansion packs) or Uno Flip.</summary>
    public GameVersions Version { get; }

    public Game(IEnumerable<Player> players, Deck deck, ExpansionPacks packs, IGameUi ui, Random? rng = null,
        GameVersions version = GameVersions.Normal)
    {
        this.players = players.ToList();
        if (this.players.Count < 2)
            throw new ArgumentException("A game needs at least 2 players.", nameof(players));
        for (int i = 0; i < this.players.Count; i++)
        {
            if (this.players[i].Index != i)
                throw new ArgumentException("Each player's Index must match its position in the list.", nameof(players));
        }

        Deck = deck;
        Packs = packs;
        Version = version;
        this.ui = ui;
        this.rng = rng ?? Random.Shared;

        foreach (Player player in this.players)
            DrawCards(player, InitialHandSize);

        TopCard = Deck.TakeStartingCard();
        CurrentColor = TopCard.Color;
    }

    /// <summary>Player 1 is the human, everyone else is an NPC.</summary>
    public static Game CreateStandard(ExpansionPacks packs, IGameUi ui, int playerCount = 4, Random? rng = null,
        GameVersions version = GameVersions.Normal)
    {
        //Uno Flip has no expansion packs
        if (version == GameVersions.Flip)
            packs = ExpansionPacks.None;

        var players = new List<Player> { new HumanPlayer(0, ui) };
        for (int i = 1; i < playerCount; i++)
            players.Add(new NpcPlayer(i, rng));

        return new Game(players, new Deck(packs, rng, version), packs, ui, rng, version);
    }

    // ---------------------------------------------------------------- game loop

    public Player Run()
    {
        while (Winner == null)
            PlayTurn();

        return Winner;
    }

    public void PlayTurn()
    {
        Player player = CurrentPlayer;

        //Defense pack: a skip penalty passed to this player makes them lose this turn
        if (skippedPlayers.Remove(player))
        {
            ui.Show($"{player.Name} loses their turn", MessageStyle.Event);
            ui.Pause(TurnDelayMs);
            AdvanceTurn();
            ui.Show("");
            return;
        }

        //Frenemies pack: a Wild Nemesis lasts until its owner ends it, asked at the start of each of their rounds
        bool startsRound = previousTurnPlayer != player;
        previousTurnPlayer = player;
        if (startsRound)
            AskToEndNemeses(player);

        //Billie pack: Reverse Hands ends when its owner starts their next turn
        if (handsOwner == player)
        {
            handsOwner = null;
            ui.Show("The hands are face down again", MessageStyle.Event);
        }
        else if (handsOwner != null)
        {
            ShowRevealedHands();
        }

        //Billie pack: a Trick Wild Card stays active, round after round, until its victim can play.
        //trickPlayer is the owner when it is the victim's turn and they have nothing to play.
        Player? trickPlayer = null;
        if (trickVictim != null)
        {
            if (player == trickVictim)
            {
                if (player.Hand.Any(IsPlayable))
                    ClearTrick();           //The victim can play: the trick is over
                else
                    trickPlayer = trickOwner;
            }
            else if (player != trickOwner)
            {
                ClearTrick();               //The turn order moved on (a skip, for example): the trick is over
            }
        }

        //Comeback pack: the U-N-O rule replaces the whole turn
        if (TryUnoRule(player))
        {
            ui.Pause(TurnDelayMs);
            AdvanceTurn();
            ui.Show("");
            return;
        }

        //Speed pack: the speed level when the turn starts is how many cards the player must play.
        //Speed-up icons played during the turn only count for the next player.
        int required = SpeedLevel;
        int played = 0;
        Card? lastPlayed = null;

        //Uno Flip: the human always sees which side is up
        if (Version == GameVersions.Flip && player.IsHuman)
            ui.Show($"--- {CurrentSide} side ---", MessageStyle.Event);

        while (played < required)
        {
            List<Card> playable = player.Hand
                .Where(card => IsPlayable(card) && (player.IsHuman || !IsIllegalWildDraw(player, card)))
                .ToList();
            if (playable.Count == 0)
                break;

            if (required > 1)
                ui.Show($"{player.Name}: card {played + 1} of {required}", MessageStyle.Event);

            Card card = player.ChooseCard(this, playable);
            if (!playable.Contains(card))
                throw new InvalidOperationException($"{player.Name} chose a card that can't be played: {card}");

            PlayCard(player, card, announce: true);
            played++;
            lastPlayed = card;
            if (HasWon(player))
                return;

            if (played == required)
            {
                //Only the last card of the turn has its effect
                card.Apply(this);
                if (Winner != null)
                    return;
            }
            else if (card.IsWild)
            {
                //Not the last card, but the color is needed to keep playing
                ChooseColor();
            }
        }

        if (played < required)
        {
            //Can't keep up: the card played last still counts, then the player draws the speed level
            if (lastPlayed != null)
            {
                lastPlayed.Apply(this);
                if (Winner != null)
                    return;
            }

            if (required == 1)
                ui.Show($"No cards available for {player.Name}, draw 1 card", MessageStyle.Warning);
            else
                ui.Show($"{player.Name} can't keep up with the speed! Draws {required} card(s)", MessageStyle.Warning);

            DrawCards(player, required);
            ResetSpeed();

            //Trick Wild Card: the player couldn't play anything
            if (trickPlayer != null && played == 0 && ResolveTrick(player, trickPlayer, required))
                return;
        }

        ui.Pause(TurnDelayMs);
        AdvanceTurn();
        ui.Show("");
    }

    public bool IsPlayable(Card card) =>
        card.IsWild || card.Color == CurrentColor || (!TopCard.IsWild && card.SharesSymbolWith(TopCard));

    public Player NextPlayer() => players[WrapIndex(currentIndex + Direction)];

    // ---------------------------------------------------------------- operations used by cards

    public void ChooseColor()
    {
        //A wild card chooses its color only once: it may already be chosen if the card was
        //played in the middle of a speed turn and later turned out to be the last one.
        //The card played on top of a Double Down keeps the color chosen with the Double Down.
        if (colorChosen || colorLocked)
            return;

        Player player = CurrentPlayer;
        CurrentColor = player.ChooseColor(this);
        colorChosen = true;

        if (!player.IsHuman)
            ui.Show($"{player.Name} selected color {CurrentColor.ToString().ToLower()}", MessageStyle.Event);
    }

    public void Reverse()
    {
        Direction = -Direction;
        string flow = Direction == 1 ? "clockwise" : "anticlockwise";
        ui.Show("Game flow is now " + flow, MessageStyle.Event);
    }

    public void SkipNextPlayers(int turns) => SkipFrom(NextPlayer(), turns);

    /// <summary>The current player plays again after this turn.</summary>
    public void GrantExtraTurn()
    {
        repeatTurn = true;
        ui.Show($"{CurrentPlayer.Name} plays again", MessageStyle.Event);
    }

    /// <summary>
    /// The next player draws cards and/or skips turns. The victim may first answer with a Defense pack card
    /// (<see cref="TryDefend"/>). A player who would draw cards and holds a
    /// Wild No U card may play it in response (<see cref="Player.ShouldPlayNoU"/>): the direction
    /// reverses, they choose the color and the punishment goes back to the player who played the card.
    /// </summary>
    public void PunishNextPlayer(int drawCount, int turnsSkipped)
    {
        Player attacker = CurrentPlayer;
        Player victim = NextPlayer();
        Player source = attacker;   //Who makes the victim draw

        //Defense pack: the victim may answer with a defense card before anything else
        if (TryDefend(attacker, victim, drawCount, turnsSkipped))
            return;

        //Wild No U only answers cards that make you draw
        WildNoUCard? noU = drawCount > 0
            ? victim.Hand.OfType<WildNoUCard>().FirstOrDefault()
            : null;

        if (noU != null && victim.ShouldPlayNoU(this, drawCount))
        {
            ui.Show($"{victim.Name} draws {drawCount} card(s)", MessageStyle.Event);
            ui.Pause(ShortDelayMs);
            ui.Show("\nBut wait...\n", MessageStyle.Npc);
            ui.Pause(ShortDelayMs);
            ui.Show($"{victim.Name} plays a Wild No U Card and reverse the punishment\n", MessageStyle.Event);
            ui.Pause(TurnDelayMs);

            //The victim really plays the card: it becomes the top card (so it can win the game)
            currentIndex = victim.Index;
            PlayCard(victim, noU, announce: false);
            if (HasWon(victim))
                return;

            //The victim chooses the color that continues play (after PlayCard, which resets it)
            ChooseColor();
            Reverse();

            //The punishment goes back to whoever played the Draw card, and now it is the No U player who makes them draw
            currentIndex = attacker.Index;
            source = victim;
            victim = attacker;
        }

        ApplyPunishment(victim, source, drawCount, turnsSkipped);
    }

    // ---------------------------------------------------------------- internals

    private void ApplyPunishment(Player victim, Player source, int drawCount, int turnsSkipped)
    {
        if (drawCount > 0 && !ForceDraw(victim, source, drawCount))
            return;

        if (turnsSkipped > 0)
            SkipFrom(victim, turnsSkipped);
    }

    private void DrawCards(Player player, int count)
    {
        for (int i = 0; i < count; i++)
            player.Hand.Add(Deck.Draw());
    }

    private void SkipFrom(Player firstSkipped, int turns)
    {
        ui.Show($"Skipped {turns} turn(s)", MessageStyle.Event);
        currentIndex = WrapIndex(firstSkipped.Index + (turns - 1) * Direction);
    }

    private void PlayCard(Player player, Card card, bool announce)
    {
        //Uno Flip: a Wild Draw card is only legal without a card of the color in play (see ResolveWildDraw)
        matchedColorInHand = player.Hand.Any(other => other != card && other.Color == CurrentColor);

        player.Hand.Remove(card);
        Deck.Discard(TopCard);
        TopCard = card;
        CurrentColor = card.Color; //Wild cards choose their color in Apply
        colorChosen = false;

        if (announce && !player.IsHuman)
            ui.Show($"{player.Name} use the card: {card}", MessageStyle.Npc);

        //Every Speed Up icon counts as soon as the card is played, even in the middle of a turn
        if (card.SpeedUp > 0)
            IncreaseSpeed(card.SpeedUp);
    }

    /// <summary>Says UNO! when one card is left. Returns true if the player just won.</summary>
    private bool HasWon(Player player)
    {
        if (player.Hand.Count == 1)
        {
            ui.Show($"{player.Name} says: UNO!", MessageStyle.Event);
        }
        else if (player.Hand.Count == 0)
        {
            DeclareWinner(player);
            return true;
        }

        return false;
    }

    private void DeclareWinner(Player player)
    {
        Winner = player;
        ui.Show($"\n{player.Name} wins!", MessageStyle.Success);

        /*if (Version == GameVersions.Flip)
            ShowRoundScore(player);*/
    }

    private void AdvanceTurn()
    {
        if (forcedNext != null)
        {
            currentIndex = forcedNext.Index;
            forcedNext = null;
            return;
        }

        if (repeatTurn)
        {
            repeatTurn = false;
            return;
        }

        currentIndex = WrapIndex(currentIndex + Direction);
    }

    private int WrapIndex(int index) => ((index % players.Count) + players.Count) % players.Count;

    // SOLO PARA PRUEBAS
    public void ForceTopCard(Card card, CardColor? color = null)
    {
        TopCard = card;
        CurrentColor = color ?? card.Color;
    }
}
