using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uno_Game.Core;

namespace Uno_Game.UI
{
    public static class GameVersionMenu
    {
        private static readonly (string Name, GameVersions Ver)[] Available =
        {
            ("Uno", GameVersions.Normal),
            ("Uno Flip!", GameVersions.Flip),
        };

        public static GameVersions Select(IGameUi ui)
        {
            string lines = string.Join("\n", Available.Select((available, i) =>
                $"{i + 1}) {available.Name}"));

            //AskNumber already repeats until the number is between 1 and the number of versions
            int option = ui.AskNumber($"Select a version of the game\n{lines}", 1, Available.Length);

            return Available[option - 1].Ver;
        }
    }
}
