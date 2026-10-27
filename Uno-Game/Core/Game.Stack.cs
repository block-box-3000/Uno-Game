using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Cards.StackPack;
using Uno_Game.Players;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    // ---------------------------------------------------------------- Stack pack
    public partial class Game
    {
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

    }
}
