using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.TeamsVersion
{
    public class WildTeamDrawCard : Card
    {
        public int DrawCount { get; }
        public WildTeamDrawCard(int drawCount) : base(CardColor.Wild, 50)
        {
            if (drawCount % 2 == 0)
                DrawCount = drawCount;
            else
                throw new ArgumentException("Draw count must be an even number");
        }

        protected override string Name => $"Wild team draw {DrawCount}";

        public override void Apply(Game game)
        {
            game.ChooseColor();
            game.WildTeamDraw(DrawCount);
        }
    }
}
