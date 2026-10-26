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
    private const int PettyPlayCards = 3;      //Cards drawn because of a Wild Petty Play
    private const int NemesisExtraCards = 2;   //Extra cards drawn by the enemy of a Wild Nemesis

    private readonly IGameUi ui;
    private readonly Random rng;
    private readonly List<Player> players;
    private int currentIndex;
    private bool repeatTurn;
    private bool colorChosen;
    private bool colorLocked;   //Comeback: the card played on top of a Double Down doesn't change the color

    //Billie pack
    private Player? handsOwner;     //Reverse Hands: hands stay face up until this player starts their next turn
    private Player? trickOwner;     //Trick Wild Card: who played it...
    private Player? trickVictim;    //...and the player who must answer it
    private readonly Dictionary<Player, List<Player>> nemeses = new();   //Frenemies: owner of a Wild Nemesis -> their enemies
    private readonly HashSet<Player> skippedPlayers = new();   //Defense: players who lose their next turn because a penalty was passed to them
    private Player? previousTurnPlayer;     //Who played the previous turn (to detect the start of a new round for a player)
    private Player? forcedNext;     //Who plays right after the current turn, overriding the normal order

    public IReadOnlyList<Player> Players => players;
    public Deck Deck { get; }
    public ExpansionPacks Packs { get; }

    public Card TopCard { get; private set; }

    /// <summary>The color that must be matched. For wild cards it is the color the player chose.</summary>
    public CardColor CurrentColor { get; private set; }

    /// <summary>1: clockwise, -1: anticlockwise.</summary>
    public int Direction { get; private set; } = 1;

    /// <summary>
    /// Speed pack: how many cards each player must play on their turn. Starts at 1 and only goes back
    /// to 1 when someone can't keep up.
    /// </summary>
    public int SpeedLevel { get; private set; } = 1;

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
            //Cheapest first, so wild stack cards are saved for when they are needed
            Player next = NextPlayer();
            List<StackCard> answers = next.Hand
                .OfType<StackCard>()
                .Where(CanAnswerStack)
                .OrderBy(card => card.Points)
                .ToList();

            //Answering is optional: the player may refuse and draw the stack
            StackCard? answer = answers.Count > 0 ? next.ChooseStackAnswer(this, answers, stack) : null;
            if (answer == null)
                break;

            if (!answers.Contains(answer))
                throw new InvalidOperationException($"{next.Name} chose a card that can't answer the stack: {answer}");

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


    // ---------------------------------------------------------------- Swap pack

    /// <summary>Swap 1: the current player takes a random card from another player and gives them one of theirs.</summary>
    public void SwapCardWithPlayer()
    {
        Player player = CurrentPlayer;
        SwapChoice choice = player.ChooseSwap(this);
        Player target = choice.Target;
        Card given = choice.CardToGive;

        if (target == player || !players.Contains(target) || !player.Hand.Contains(given))
            throw new InvalidOperationException($"{player.Name} made an invalid swap.");

        Card taken = target.Hand[rng.Next(target.Hand.Count)];

        player.Hand.Remove(given);
        target.Hand.Remove(taken);
        player.Hand.Add(taken);
        target.Hand.Add(given);

        //Humans see which cards moved; for the rest it stays secret
        if (player.IsHuman)
            ui.Show($"You gave {given} to {target.Name} and took {taken}", MessageStyle.Event);
        else if (target.IsHuman)
            ui.Show($"{player.Name} took your {taken} and gave you {given}", MessageStyle.Event);
        else
            ui.Show($"{player.Name} swaps a card with {target.Name}", MessageStyle.Event);
    }

    /// <summary>Refresh Hand: the whole hand goes to the discard pile and the player draws the same number of cards.</summary>
    public void RefreshHand()
    {
        Player player = CurrentPlayer;
        int count = player.Hand.Count;

        foreach (Card card in player.Hand)
            Deck.Discard(card);
        player.Hand.Clear();

        DrawCards(player, count);
        ui.Show($"{player.Name} refreshes their hand ({count} card(s))", MessageStyle.Event);
    }

    /// <summary>Wild Force Trade: the current player picks two different players and they swap hands.</summary>
    public void TradeHands()
    {
        (Player first, Player second) = CurrentPlayer.ChooseTradePlayers(this);

        if (first == second || !players.Contains(first) || !players.Contains(second))
            throw new InvalidOperationException($"{CurrentPlayer.Name} chose an invalid pair of players.");

        List<Card> firstHand = first.Hand.ToList();
        first.Hand.Clear();
        first.Hand.AddRange(second.Hand);
        second.Hand.Clear();
        second.Hand.AddRange(firstHand);

        ui.Show($"{first.Name} and {second.Name} swap hands", MessageStyle.Event);
    }

    /// <summary>Wild Pass Hands: every player passes their hand to the next player in the current direction.</summary>
    public void PassHands()
    {
        List<List<Card>> hands = players.Select(player => player.Hand.ToList()).ToList();

        for (int i = 0; i < players.Count; i++)
        {
            Player receiver = players[WrapIndex(i + Direction)];
            receiver.Hand.Clear();
            receiver.Hand.AddRange(hands[i]);
        }

        string flow = Direction == 1 ? "clockwise" : "anticlockwise";
        ui.Show($"Everybody passes their hand to the next player ({flow})", MessageStyle.Event);
    }

    // ---------------------------------------------------------------- Billie Eilish pack

    /// <summary>
    /// Wild Pass Color: every player passes all their cards of the chosen color to the player on their left
    /// (the next one in clockwise order, whatever the current direction). Players without that color pass
    /// nothing but still receive the cards passed to them.
    /// </summary>
    public void PassColor()
    {
        CardColor color = CurrentColor;
        int count = players.Count;

        List<List<Card>> passed = players.Select(player => player.Hand.Where(card => card.Color == color).ToList()).ToList();

        foreach (Player player in players)
            player.Hand.RemoveAll(card => card.Color == color);

        for (int i = 0; i < count; i++)
            players[(i + 1) % count].Hand.AddRange(passed[i]);

        for (int i = 0; i < count; i++)
        {
            Player giver = players[i];
            Player receiver = players[(i + 1) % count];
            if (passed[i].Count == 0)
                continue;

            //Humans see exactly which cards moved; for the rest it stays secret
            if (giver.IsHuman)
                ui.Show($"You pass {string.Join(", ", passed[i])} to {receiver.Name}", MessageStyle.Event);
            else if (receiver.IsHuman)
                ui.Show($"{giver.Name} passes you {string.Join(", ", passed[i])}", MessageStyle.Event);
            else
                ui.Show($"{giver.Name} passes {passed[i].Count} {color} card(s) to {receiver.Name}", MessageStyle.Event);
        }

        CheckEmptyHands();
    }

    /// <summary>A hand that ends up empty wins (starting with the current player).</summary>
    private void CheckEmptyHands()
    {
        for (int k = 0; k < players.Count; k++)
        {
            Player player = players[WrapIndex(currentIndex + k)];
            if (player.Hand.Count == 0)
            {
                DeclareWinner(player);
                return;
            }
        }
    }

    /// <summary>
    /// Trick Wild Card: every time the next player can't play, the current player plays again,
    /// until that player manages to play (see <see cref="ResolveTrick"/>).
    /// </summary>
    public void SetTrick()
    {
        trickOwner = CurrentPlayer;
        trickVictim = NextPlayer();
        ui.Show($"Trick! {trickOwner.Name} keeps playing until {trickVictim.Name} can play a {CurrentColor} card", MessageStyle.Event);
    }

    /// <summary>
    /// The victim of a Trick Wild Card had nothing to play and already drew. If they drew a single card and
    /// it can be played, they play it and the trick is over. Otherwise the owner plays again and the trick
    /// stays active: it repeats every round until the victim manages to play. Returns true if the game is over.
    /// </summary>
    private bool ResolveTrick(Player victim, Player owner, int drawn)
    {
        if (drawn == 1)
        {
            Card card = victim.Hand[^1];
            if (IsPlayable(card))
            {
                //Cleared first: the card they play may start a new trick
                ClearTrick();
                ui.Show($"{victim.Name} draws {card}, which can be played", MessageStyle.Event);
                PlayCard(victim, card, announce: true);
                if (HasWon(victim))
                    return true;

                card.Apply(this);
                return Winner != null;
            }
        }

        ui.Show($"{owner.Name} plays again (still waiting for {victim.Name})", MessageStyle.Event);
        forcedNext = owner;
        return false;
    }

    private void ClearTrick()
    {
        trickOwner = null;
        trickVictim = null;
    }

    /// <summary>
    /// Wild Color Challenge: every other player must show a card of the chosen color. Whoever can't draws 2 cards;
    /// if everybody shows one, the current player draws 3.
    /// </summary>
    public void ColorChallenge()
    {
        Player challenger = CurrentPlayer;
        CardColor color = CurrentColor;
        bool everybodyShowed = true;

        ui.Show($"Color challenge! Everybody shows a {color} card", MessageStyle.Event);

        for (int k = 1; k < players.Count; k++)
        {
            Player other = players[WrapIndex(currentIndex + k * Direction)];
            Card? shown = other.Hand.FirstOrDefault(card => card.Color == color);

            if (shown == null)
            {
                everybodyShowed = false;
                ui.Show($"{other.Name} has no {color} card and draws 2 cards", MessageStyle.Event);
                if (!ForceDraw(other, challenger, 2, announce: false))
                    return;
            }
            else if (other.IsHuman)
            {
                ui.Show($"You show {shown}", MessageStyle.Event);
            }
            else
            {
                //Only the challenger gets to see the card
                string text = challenger.IsHuman ? shown.ToString() : $"a {color} card";
                ui.Show($"{other.Name} shows {text}", MessageStyle.Event);
            }
        }

        if (everybodyShowed)
        {
            ui.Show($"Everybody showed a {color} card: {challenger.Name} draws 3 cards", MessageStyle.Event);
            DrawCards(challenger, 3);
        }
    }

    /// <summary>
    /// Reverse Hands: every hand is shown face up at the start of each turn until the player
    /// who played the card starts their next turn.
    /// </summary>
    public void RevealHands()
    {
        handsOwner = CurrentPlayer;
        ui.Show($"{handsOwner.Name} plays Reverse Hands! Everybody lays their hand face up until {handsOwner.Name}'s next turn",
            MessageStyle.Event);
    }

    private void ShowRevealedHands() =>
        ui.ShowHands(players.Select(player => (player.Name, (IReadOnlyList<Card>) player.Hand.ToList())).ToList());

    // ---------------------------------------------------------------- Frenemies pack

    /// <summary>Wild Petty Play played on your turn: the chosen player draws 3 cards.</summary>
    public void PettyPlay()
    {
        Player player = CurrentPlayer;
        Player target = player.ChoosePettyPlayTarget(this);

        if (target == player || !players.Contains(target))
            throw new InvalidOperationException($"{player.Name} chose an invalid target.");

        ui.Show($"{player.Name} picks {target.Name} for a petty play", MessageStyle.Event);
        ForceDraw(target, player, PettyPlayCards);
    }

    /// <summary>
    /// Wild Nemesis: the current player names another player as their enemy. Every time they make that
    /// player draw cards, the enemy draws 2 extra cards, until the owner ends the beef
    /// (see <see cref="AskToEndNemeses"/>). A player can have several enemies.
    /// </summary>
    public void SetNemesis()
    {
        Player owner = CurrentPlayer;
        nemeses.TryGetValue(owner, out List<Player>? current);

        List<Player> candidates = players.Where(player => player != owner && (current == null || !current.Contains(player))).ToList();
        if (candidates.Count == 0)
        {
            ui.Show($"{owner.Name} already has everybody as an enemy", MessageStyle.Event);
            return;
        }

        Player enemy = owner.ChooseEnemy(this, candidates);
        if (!candidates.Contains(enemy))
            throw new InvalidOperationException($"{owner.Name} chose an invalid enemy.");

        if (current == null)
            nemeses[owner] = current = new List<Player>();
        current.Add(enemy);

        ui.Show($"{owner.Name} names {enemy.Name} as their nemesis: every time {owner.Name} makes {enemy.Name} draw, " +
                $"{enemy.Name} draws {NemesisExtraCards} extra cards", MessageStyle.Event);
    }

    /// <summary>At the start of their round, the owner of a Wild Nemesis decides for each enemy whether the beef is over.</summary>
    private void AskToEndNemeses(Player owner)
    {
        if (!nemeses.TryGetValue(owner, out List<Player>? enemies))
            return;

        foreach (Player enemy in enemies.ToList())
        {
            if (!owner.ShouldEndNemesis(this, enemy))
                continue;

            enemies.Remove(enemy);
            ui.Show($"{owner.Name} ends the beef with {enemy.Name}", MessageStyle.Event);
        }

        if (enemies.Count == 0)
            nemeses.Remove(owner);
    }

    /// <summary>Wild Eliminator: every opponent discards all their wild cards.</summary>
    public void EliminateWilds()
    {
        Player eliminator = CurrentPlayer;
        ui.Show($"{eliminator.Name} plays Wild Eliminator! Every opponent discards their wild cards", MessageStyle.Event);

        for (int k = 1; k < players.Count; k++)
        {
            Player other = players[WrapIndex(currentIndex + k * Direction)];
            List<Card> wilds = other.Hand.Where(card => card.IsWild).ToList();
            if (wilds.Count == 0)
                continue;

            foreach (Card wild in wilds)
            {
                other.Hand.Remove(wild);
                Deck.Discard(wild);
            }

            //Humans see exactly which cards were discarded; for the rest it stays secret
            if (other.IsHuman || eliminator.IsHuman)
                ui.Show($"{(other.IsHuman ? "You discard" : other.Name + " discards")} {string.Join(", ", wilds)}", MessageStyle.Event);
            else
                ui.Show($"{other.Name} discards {wilds.Count} wild card(s)", MessageStyle.Event);
        }

        CheckEmptyHands();
    }

    /// <summary>Wild Instigator: chosen opponents draw from 1 to 4 cards each (in turn order).</summary>
    public void Instigate()
    {
        Player instigator = CurrentPlayer;
        IReadOnlyList<(Player Target, int Cards)> choices = instigator.ChooseInstigatorTargets(this);

        bool valid = choices.All(choice => choice.Target != instigator && players.Contains(choice.Target) && choice.Cards >= 1 && choice.Cards <= 4)
                     && choices.Select(choice => choice.Target).Distinct().Count() == choices.Count;
        if (!valid)
            throw new InvalidOperationException($"{instigator.Name} made an invalid instigation.");

        if (choices.Count == 0)
        {
            ui.Show($"{instigator.Name} instigates nobody", MessageStyle.Event);
            return;
        }

        ui.Show($"{instigator.Name} plays Wild Instigator!", MessageStyle.Event);

        for (int k = 1; k < players.Count; k++)
        {
            Player target = players[WrapIndex(currentIndex + k * Direction)];
            foreach (var choice in choices.Where(choice => choice.Target == target))
            {
                if (!ForceDraw(target, instigator, choice.Cards))
                    return;
            }
        }
    }

    /// <summary>
    /// A player makes ANOTHER player draw cards. Every forced draw goes through here, so the Frenemies cards
    /// work with any penalty (Draw cards, stacks, color challenge...):
    /// - Wild Nemesis: the victim draws 2 extra cards if the source named them as their enemy.
    /// - Wild Petty Play: the victim may answer out of turn, making the source draw 3 cards (the answer can be
    ///   answered back). It goes to the discard pile, so it changes neither the top card, the color nor whose turn it is:
    ///   play resumes from the current position.
    /// Returns false if the game ended on the way (the victim played their last card).
    /// </summary>
    private bool ForceDraw(Player victim, Player source, int count, bool announce = true)
    {
        WildPettyPlayCard? pettyPlay = victim.Hand.OfType<WildPettyPlayCard>().FirstOrDefault();
        if (pettyPlay != null && victim.ShouldPlayPettyPlay(this, source, count))
        {
            victim.Hand.Remove(pettyPlay);
            Deck.Discard(pettyPlay);
            ui.Show($"{victim.Name} plays a Wild Petty Play Card out of turn!", MessageStyle.Event);
            ui.Pause(ShortDelayMs);

            if (victim.Hand.Count == 0)
            {
                DeclareWinner(victim);
                return false;
            }

            if (!ForceDraw(source, victim, PettyPlayCards))
                return false;
        }

        if (announce)
            ui.Show($"{victim.Name} draws {count} card(s)", MessageStyle.Event);

        int extra = nemeses.TryGetValue(source, out List<Player>? enemies) && enemies.Contains(victim) ? NemesisExtraCards : 0;
        if (extra > 0)
            ui.Show($"{source.Name} is {victim.Name}'s nemesis: {extra} extra card(s)", MessageStyle.Event);

        DrawCards(victim, count + extra);
        return true;
    }

    // ---------------------------------------------------------------- Defense pack

    /// <summary>
    /// A Draw or Skip penalty falls on <paramref name="victim"/>, who may answer with a defense card
    /// (<see cref="DefenseCard"/>). The card is played out of turn: it becomes the top card and the victim chooses the
    /// color, but whose turn it is does not change, so play resumes from the current position (the victim, no longer
    /// penalized, plays next in the normal order). A penalty passed on by a defense card is final (see
    /// <see cref="PassPenalty"/>). Returns true if the penalty was dealt with.
    /// </summary>
    private bool TryDefend(Player attacker, Player victim, int drawCount, int turnsSkipped)
    {
        List<DefenseCard> options = victim.Hand
            .OfType<DefenseCard>()
            .Where(card => card.CanAnswer(drawCount))
            .ToList();
        if (options.Count == 0)
            return false;

        DefenseCard? defense = victim.ChooseDefense(this, options, attacker, drawCount, turnsSkipped);
        if (defense == null)
            return false;
        if (!options.Contains(defense))
            throw new InvalidOperationException($"{victim.Name} chose a defense card that can't answer this penalty: {defense}");

        ui.Show($"{victim.Name} answers with {defense}!", MessageStyle.Event);
        ui.Pause(ShortDelayMs);

        //The victim plays the card, so they are the one who chooses the color (after PlayCard, which resets it)
        int turnOwner = currentIndex;
        currentIndex = victim.Index;
        PlayCard(victim, defense, announce: false);
        if (HasWon(victim))
            return true;

        ChooseColor();
        currentIndex = turnOwner;

        defense.Answer(this, victim, attacker, drawCount, turnsSkipped);
        return true;
    }

    /// <summary>
    /// A defense card moves a penalty from <paramref name="from"/> to <paramref name="to"/>: the new target draws the cards
    /// and loses their next turn. It is final: no defense card and no Wild No U can answer a passed penalty.
    /// </summary>
    public void PassPenalty(Player from, Player to, int drawCount, int turnsSkipped)
    {
        ui.Show($"The penalty goes to {to.Name}", MessageStyle.Event);

        if (drawCount > 0 && !ForceDraw(to, from, drawCount))
            return;

        if (turnsSkipped > 0)
        {
            skippedPlayers.Add(to);
            ui.Show($"{to.Name} will lose their next turn", MessageStyle.Event);
        }
    }

    public void BlockPenalty(Player victim) =>
        ui.Show($"{victim.Name} blocks the penalty", MessageStyle.Event);

    /// <summary>The next player after <paramref name="player"/> in the current order.</summary>
    public Player PlayerAfter(Player player) => players[WrapIndex(player.Index + Direction)];

    /// <summary>Wild Redirect: the victim picks any other player to receive the penalty.</summary>
    public Player ChooseRedirectTarget(Player victim)
    {
        List<Player> candidates = players.Where(player => player != victim).ToList();
        Player target = victim.ChooseRedirectTarget(this, candidates);

        if (!candidates.Contains(target))
            throw new InvalidOperationException($"{victim.Name} chose an invalid target.");

        return target;
    }

    // ---------------------------------------------------------------- Comeback pack

    /// <summary>Wild Swap Hands: the current player swaps hands with any other player.</summary>
    public void SwapHands()
    {
        Player player = CurrentPlayer;
        Player target = player.ChooseSwapHandsTarget(this);

        if (target == player || !players.Contains(target))
            throw new InvalidOperationException($"{player.Name} chose an invalid player to swap hands with.");

        List<Card> mine = player.Hand.ToList();
        player.Hand.Clear();
        player.Hand.AddRange(target.Hand);
        target.Hand.Clear();
        target.Hand.AddRange(mine);

        //Humans see the cards they get; for the rest it stays secret
        if (player.IsHuman)
            ui.Show($"You swap hands with {target.Name} and get: {string.Join(", ", player.Hand)}", MessageStyle.Event);
        else if (target.IsHuman)
            ui.Show($"{player.Name} swaps hands with you: you get {string.Join(", ", target.Hand)}", MessageStyle.Event);
        else
            ui.Show($"{player.Name} swaps hands with {target.Name}", MessageStyle.Event);
    }

    /// <summary>
    /// Wild Double Down (its color is already chosen): the player discards an additional card on top of it.
    /// That card is played normally on the chosen color and has its effect (draw, skip...), but the game keeps the
    /// color chosen with the Double Down, even if the additional card is wild. If there is no card to play, nothing happens.
    /// </summary>
    public void DoubleDown()
    {
        Player player = CurrentPlayer;
        CardColor chosen = CurrentColor;

        List<Card> playable = player.Hand.Where(IsPlayable).ToList();
        if (playable.Count == 0)
        {
            ui.Show($"{player.Name} has no card to discard on top of it", MessageStyle.Event);
            return;
        }

        Card extra = player.ChooseCard(this, playable);
        if (!playable.Contains(extra))
            throw new InvalidOperationException($"{player.Name} chose a card that can't be played: {extra}");

        PlayCard(player, extra, announce: true);
        CurrentColor = chosen;
        if (HasWon(player))
            return;

        colorLocked = true;
        try
        {
            extra.Apply(this);
        }
        finally
        {
            colorLocked = false;
        }
    }

    /// <summary>Wild Match the Max: the chosen player draws until their hand is as big as the largest hand.</summary>
    public void MatchTheMax()
    {
        Player player = CurrentPlayer;
        Player target = player.ChooseMatchTarget(this);

        if (target == player || !players.Contains(target))
            throw new InvalidOperationException($"{player.Name} chose an invalid player.");

        int missing = LargestHand() - target.Hand.Count;
        if (missing <= 0)
        {
            ui.Show($"{target.Name} already has the largest hand", MessageStyle.Event);
            return;
        }

        ForceDraw(target, player, missing);
    }

    /// <summary>Wild Misery Loves Company: every other player draws until their hand is as big as the largest hand.</summary>
    public void MiseryLovesCompany()
    {
        Player player = CurrentPlayer;
        int largest = LargestHand();   //Computed once, before anyone draws

        for (int k = 1; k < players.Count; k++)
        {
            Player other = players[WrapIndex(currentIndex + k * Direction)];
            int missing = largest - other.Hand.Count;

            if (missing <= 0)
            {
                ui.Show($"{other.Name} already has the largest hand", MessageStyle.Event);
                continue;
            }

            if (!ForceDraw(other, player, missing))
                return;
        }
    }

    private int LargestHand() => players.Max(player => player.Hand.Count);

    /// <summary>
    /// U-N-O rule: a player holding a U, an N and an O card (and at least one more card) may, at the start of their turn,
    /// discard down to one card for an instant UNO. All the other cards go to the discard pile under the three U-N-O cards,
    /// whose actions are not activated, and the player chooses the next color. It takes the whole turn.
    /// Returns true if the rule was used.
    /// </summary>
    private bool TryUnoRule(Player player)
    {
        Card? u = player.Hand.FirstOrDefault(card => card.UnoLetter == 'U');
        Card? n = player.Hand.FirstOrDefault(card => card.UnoLetter == 'N');
        Card? o = player.Hand.FirstOrDefault(card => card.UnoLetter == 'O');
        if (u == null || n == null || o == null)
            return false;

        //The card to keep can't be one of the three that spell U-N-O
        List<Card> candidates = player.Hand.Where(card => card != u && card != n && card != o).ToList();
        if (candidates.Count == 0)
            return false;

        Card? keep = player.ChooseUnoKeep(this, candidates);
        if (keep == null)
            return false;
        if (!candidates.Contains(keep))
            throw new InvalidOperationException($"{player.Name} chose an invalid card to keep: {keep}");

        ui.Show($"{player.Name} spells U-N-O and discards down to one card!", MessageStyle.Event);

        foreach (Card card in player.Hand.Where(card => card != keep && card != u && card != n && card != o).ToList())
        {
            player.Hand.Remove(card);
            Deck.Discard(card);
        }

        //The three U-N-O cards end up on top, so the last one is the top card of the table
        Deck.Discard(TopCard);
        player.Hand.Remove(u);
        player.Hand.Remove(n);
        player.Hand.Remove(o);
        Deck.Discard(u);
        Deck.Discard(n);
        TopCard = o;
        CurrentColor = CardColor.Wild;
        colorChosen = false;
        ChooseColor();

        ui.Show($"{player.Name} says: UNO!", MessageStyle.Event);
        return true;
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

    private void IncreaseSpeed(int levels)
    {
        SpeedLevel += levels;
        ui.Show($"Speed level is now {SpeedLevel}", MessageStyle.Event);
    }

    private void ResetSpeed()
    {
        if (SpeedLevel == 1)
            return;

        SpeedLevel = 1;
        ui.Show("Speed level goes back to 1", MessageStyle.Event);
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

    // SOLO PARA PRUEBAS: borrar despues
    public void ForceTopCard(Card card, CardColor? color = null)
    {
        TopCard = card;
        CurrentColor = color ?? card.Color;
    }
}
