using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.StackPack
{
    public class WildStackCard : StackCard
    {
        public WildStackCard(int drawCount = 3, int points = 50) : base(CardColor.Wild, drawCount, points)
        {
        }

        protected override string Name => $"Wild stack {DrawCount}";
    }
}
