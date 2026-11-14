using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;
using Uno_Game.Players;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Uno_Game.Cards.PopVersion
{
    public class PopCard
    {
        public int DrawCount { get; }

        public bool IsPopPunish { get; }
        public bool IsPopUno { get; }

        public PopCard(int drawCount, bool isPopPunish = false, bool isPopUno = false)
        {
            DrawCount = drawCount;
            IsPopPunish = isPopPunish;
            IsPopUno = isPopUno;
        }

        /// <summary>The effect of the card. Called right after the card is played.</summary>
        public void Apply(Game game, Player player)
        {
            if (IsPopPunish)
                game.PopPunish(player);
            else if (IsPopUno)
                game.PopUno(player);
        }

        public override string ToString() => IsPopUno ? $"Pop Instant Uno Card" 
            : IsPopPunish ? $"+{DrawCount} Pop Card" 
            : $"+{DrawCount} Card";
    }
}
