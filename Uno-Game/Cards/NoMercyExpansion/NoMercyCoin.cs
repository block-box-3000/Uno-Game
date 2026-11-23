using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.Cards.NoMercyExpansion
{
    public class NoMercyCoin
    {
        public Coins Type { get; }
        public bool IsUsed { get; private set; }

        public NoMercyCoin(Coins coin)
        {
            Type = coin;
        }

        public bool TryUse()
        {
            if (IsUsed)
                return false;

            IsUsed = true;
            return true;
        }

        public override string ToString() => Type == Coins.Mercy ?
            "Mercy coin" : "No Mercy coin";
    }
}
