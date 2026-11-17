using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.PartyVersion
{
    public class PointTakenCard : Card
    {
        public PointTakenCard(CardColor color) : base (color, 20)
        {

        }

        protected override string Name => "Wild Point Taken Card";

        public override void Apply(Game game)
        {
            game.PointTakenDrawCards();
        }
    }
}
