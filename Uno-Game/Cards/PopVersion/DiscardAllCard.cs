using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.PopVersion
{
    public class DiscardAllCard : Card
    {
        public DiscardAllCard(CardColor color) : base(color, 20)
        {
            
        }

        protected override string Name => "Discard All Card";

        public override void Apply(Game game)
        {
            game.DiscardAllCardsOfColor(Color);
        }
    }
}
