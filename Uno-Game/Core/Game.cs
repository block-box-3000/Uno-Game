using Uno_Game.Cards;
using Uno_Game.Cards.ReversePack;
using Uno_Game.Cards.StackPack;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core;

/// <summary>
/// All the state of one match (players, deck, direction, whose turn it is) and the
/// operations cards use to change it. Cards call these methods from <see cref="Card.Apply"/>.
/// </summary>
public class Game
{
    public const int InitialHandSize = 7;
    private const int TurnDelayMs = 2000;
    private const int ShortDelayMs = 1500;

    private readonly IGameUi ui;
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

    public Game(IEnumerable<Player> players, Deck deck, ExpansionPacks packs, IGameUi ui)
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
        this.ui = ui;

        foreach (Player player in this.players)
            DrawCards(player, InitialHandSize);

        TopCard = Deck.TakeStartingCard();
        CurrentColor = TopCard.Color;
    }

    /// <summary>Player 1 is the human, everyone else is an NPC.</summary>
    public static Game CreateStandard(ExpansionPacks packs, IGameUi ui, int playerCount = 4, Random? rng = null)
    {
        var players = new List<Player> { new HumanPlayer(0, ui) };
        for (int i = 1; i < playerCount; i++)
            players.Add(new NpcPlayer(i, rng));

        return new Game(players, new Deck(packs, rng), packs, ui);
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
        List<Card> playable = player.Hand.Where(IsPlayable).ToList();

        if (playable.Count == 0)
        {
            ui.Show($"No cards available for {player.Name}, draw 1 card", MessageStyle.Warning);
            DrawCards(player, 1);
        }
        else
        {
            Card card = player.ChooseCard(this, playable);
            if (!playable.Contains(card))
                throw new InvalidOperationException($"{player.Name} chose a card that can't be played: {card}");

            PlayCard(player, card, announce: true);
            if (HasWon(player))
                return;

            card.Apply(this);
            if (Winner != null)
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
        Player player = CurrentPlayer;
        CurrentColor = player.ChooseColor(this);

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
    /// The next player draws cards and/or skips turns. A player who would draw cards and holds a
    /// Wild No U card may play it in response (<see cref="Player.ShouldPlayNoU"/>): the direction
    /// reverses, they choose the color and the punishment goes back to the player who played the card.
    /// </summary>
    public void PunishNextPlayer(int drawCount, int turnsSkipped)
    {
        Player attacker = CurrentPlayer;
        Player victim = NextPlayer();

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

            //The punishment goes back to whoever played the Draw card
            currentIndex = attacker.Index;
            victim = attacker;
        }

        ApplyPunishment(victim, drawCount, turnsSkipped);
    }

    /// <summary>
    /// Stack pack. A Draw card or a Stack card starts a pile of cards to draw. The next players answer
    /// with their own Stack cards while they can (see <see cref="CanAnswerStack"/>); each answer adds to the pile.
    /// The first player who can't answer draws the whole pile and loses their turn.
    /// </summary>
    public void ResolveStack(int startingPenalty)
    {
        int stack = startingPenalty;
        ui.Show($"{CurrentPlayer.Name} add {stack} card(s) to the stack", MessageStyle.Event);
        ui.Pause(ShortDelayMs);

        while (true)
        {
            //Cheapest answer first, so wild stack cards are saved for when they are needed
            Player next = NextPlayer();
            StackCard? answer = next.Hand
                .OfType<StackCard>()
                .Where(CanAnswerStack)
                .OrderBy(card => card.Points)
                .FirstOrDefault();

            if (answer == null)
                break;

            currentIndex = next.Index;
            PlayCard(next, answer, announce: false);
            if (HasWon(next))
                return;

            //After PlayCard: PlayCard resets CurrentColor to the card's own color (Wild)
            if (answer.IsWild)
                ChooseColor();

            int added = answer.GetPenalty(this);
            stack += added;
            ui.Show($"{next.Name} add {added} card(s) to the stack", MessageStyle.Event);
            ui.Pause(ShortDelayMs);
        }

        //Goes through PunishNextPlayer so a Wild No U can send the whole pile back
        PunishNextPlayer(stack, turnsSkipped: 1);
    }

    /// <summary>
    /// Official rules: any Stack card can answer a Stack card. To answer a Draw card the Stack card
    /// must match its color (or the color chosen for a Wild Draw 4), unless it is wild.
    /// </summary>
    private bool CanAnswerStack(StackCard card) =>
        TopCard is StackCard || card.IsWild || card.Color == CurrentColor;

    /// <summary>Wild Stack Number: flips cards until a number card shows up. That number is the penalty.</summary>
    public int RevealPenaltyNumber()
    {
        var (number, flipped) = Deck.RevealUntilNumber();
        ui.Show($"{CurrentPlayer.Name} flips: {string.Join(", ", flipped)}", MessageStyle.Event);
        ui.Pause(ShortDelayMs);
        return number;
    }

    // ---------------------------------------------------------------- internals

    private void ApplyPunishment(Player victim, int drawCount, int turnsSkipped)
    {
        if (drawCount > 0)
        {
            ui.Show($"{victim.Name} draws {drawCount} card(s)", MessageStyle.Event);
            DrawCards(victim, drawCount);
        }

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
        player.Hand.Remove(card);
        Deck.Discard(TopCard);
        TopCard = card;
        CurrentColor = card.Color; //Wild cards choose their color in Apply

        if (announce && !player.IsHuman)
            ui.Show($"{player.Name} use the card: {card}", MessageStyle.Npc);
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
            Winner = player;
            ui.Show($"\n{player.Name} wins!", MessageStyle.Success);
            return true;
        }

        return false;
    }

    private void AdvanceTurn()
    {
        if (repeatTurn)
        {
            repeatTurn = false;
            return;
        }

        currentIndex = WrapIndex(currentIndex + Direction);
    }

    private int WrapIndex(int index) => ((index % players.Count) + players.Count) % players.Count;

    // SOLO PARA PRUEBAS: borrar despues
    public void ForceTopCard(Card card, CardColor? color = null)
    {
        TopCard = card;
        CurrentColor = color ?? card.Color;
    }
}
