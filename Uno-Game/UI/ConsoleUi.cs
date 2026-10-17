using Uno_Game.Cards;
using Uno_Game.Core;

namespace Uno_Game.UI;

public class ConsoleUi : IGameUi
{
    public void Show(string text, MessageStyle style = MessageStyle.Normal)
    {
        switch (style)
        {
            case MessageStyle.Event:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case MessageStyle.Npc:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case MessageStyle.Warning:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case MessageStyle.Success:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            default:
                Console.ResetColor();
                break;
        }

        Console.WriteLine(text);
        Console.ResetColor();
    }

    public void ShowTable(Card topCard, CardColor currentColor, IReadOnlyList<Card> hand, Func<Card, bool> isPlayable)
    {
        Console.ResetColor();

        //A wild card on the pile shows the color that was chosen for it
        string top = topCard.IsWild ? $"{topCard} ({currentColor})" : topCard.ToString();
        Console.WriteLine("Actual card: " + top + "\n");
        Console.WriteLine("Your hand:");

        for (int i = 0; i < hand.Count; i++)
        {
            Console.ForegroundColor = isPlayable(hand[i]) ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{i + 1}) {hand[i]}");
        }

        Console.ResetColor();
    }

    public int AskNumber(string prompt, int min, int max)
    {
        if (!string.IsNullOrEmpty(prompt))
            Console.WriteLine(prompt);

        while (true)
        {
            string? line = Console.ReadLine();

            //No more input (Ctrl+Z / Ctrl+D): stop instead of looping forever
            if (line == null)
                throw new EndOfStreamException("No more input.");

            if (int.TryParse(line, out int number) && number >= min && number <= max)
                return number;

            Show("\nSelect a valid option", MessageStyle.Warning);
        }
    }

    public void Pause(int milliseconds) => Thread.Sleep(milliseconds);
}
