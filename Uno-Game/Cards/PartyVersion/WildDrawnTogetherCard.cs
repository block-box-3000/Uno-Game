using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.PartyVersion
{
    public class WildDrawnTogetherCard : Card
    {
        public WildDrawnTogetherCard() : base(CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild Drawn Together Card";

        public override void Apply(Game game)
        {
            game.ConnectPlayers();
            game.ChooseColor();
        }
    }
}
