using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercyExpansion 
{
    public class WildSuddenDeathCard : Card
    {
        public WildSuddenDeathCard() : base(CardColor.Wild, 50)
        {

        }

        protected override string Name => "Wild Sudden Death";

        public override void Apply(Game game)
        {
            game.SuddenDeathProtocol();
            game.ChooseColor();
        }
    }
}
