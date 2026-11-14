using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;
using Uno_Game.Players;

namespace Uno_Game.Cards.PopVersion
{
    public class PopFlipCard: Card
    {
        public int DrawFlipCardsCount { get; }
        public PopFlipCard(CardColor color, int drawFlipCardsCount) : base(color, points: 20)
        {
            if (drawFlipCardsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(drawFlipCardsCount));

            DrawFlipCardsCount = drawFlipCardsCount;
        }

        protected override string Name => $"Draw {DrawFlipCardsCount} Pop Card";

        public override void Apply(Game game)
        {
            Player nextPlayer = game.NextPlayer();
            game.ShowPopFlip(nextPlayer, DrawFlipCardsCount);

            for (int i = 0; i < DrawFlipCardsCount; i++)
            {
                if (game.PopDeck.PopDraw(game, nextPlayer))
                    break;
            }
        }
    }
}
