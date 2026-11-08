using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.TeamsVersion
{
    public class NumberSwapCard : NumberCard
    {
        public NumberSwapCard(CardColor color, int number) : base(color, number)
        {
        }

        protected override string Name => $"{Number} (swap)";

        public override bool SwapTeammateCard => true;
    }
}
