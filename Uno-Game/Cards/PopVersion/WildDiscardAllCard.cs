using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.PopVersion
{
    public class WildDiscardAllCard : Card
    {
        public WildDiscardAllCard() : base(CardColor.Wild, 50)
        {
        }

        protected override string Name => "Wild Discard All Card";

        public override void Apply(Game game)
        {
            game.ChooseColor();
            game.DiscardAllCardsOfColor(game.CurrentColor);
        }
    }
}
