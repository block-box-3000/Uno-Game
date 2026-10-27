using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.UI;

namespace Uno_Game.Core
{
    // ---------------------------------------------------------------- Speed pack
    public partial class Game
    {
        /// <summary>
        /// Speed pack: how many cards each player must play on their turn. Starts at 1 and only goes back
        /// to 1 when someone can't keep up.
        /// </summary>
        public int SpeedLevel { get; private set; } = 1;

        private void IncreaseSpeed(int levels)
        {
            SpeedLevel += levels;
            ui.Show($"Speed level is now {SpeedLevel}", MessageStyle.Event);
        }

        private void ResetSpeed()
        {
            if (SpeedLevel == 1)
                return;

            SpeedLevel = 1;
            ui.Show("Speed level goes back to 1", MessageStyle.Event);
        }
    }
}
