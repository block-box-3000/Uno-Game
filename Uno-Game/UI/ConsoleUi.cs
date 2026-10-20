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

    public void ShowHands(IReadOnlyList<(string PlayerName, IReadOnlyList<Card> Cards)> hands)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("---- Hands on the table ----");

        foreach (var (playerName, cards) in hands)
        {
            Console.ResetColor();
            Console.Write($"{playerName} ({cards.Count}): ");

            //Each card in its own color
            for (int i = 0; i < cards.Count; i++)
            {
                Console.ForegroundColor = ToConsoleColor(cards[i].Color);
                Console.Write(cards[i]);
                Console.ResetColor();
                if (i < cards.Count - 1)
                    Console.Write(", ");
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("----------------------------");
        Console.ResetColor();
    }

    private static ConsoleColor ToConsoleColor(CardColor color) => color switch
    {
        CardColor.Red => ConsoleColor.Red,
        CardColor.Yellow => ConsoleColor.Yellow,
        CardColor.Blue => ConsoleColor.Blue,
        CardColor.Green => ConsoleColor.Green,
        _ => ConsoleColor.White
    };

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
