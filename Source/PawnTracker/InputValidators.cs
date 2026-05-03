using System.Linq;
using Verse;

namespace PawnHistory.Source.PawnTracker;

public static class InputValidators
{
    public static bool DigitsOnly(string buffer)
    {
        if (buffer.NullOrEmpty())
            return true;

        return buffer.All(char.IsDigit);
    }

    public static bool TryPositiveInt(string text, out int value, out string error)
    {
        value = 0;
        error = null;

        if (text.NullOrEmpty())
        {
            error = "Enter a page number.";
            return false;
        }

        if (!int.TryParse(text, out value))
        {
            error = "Enter a valid whole number.";
            return false;
        }

        if (value <= 0)
        {
            error = "Enter a positive page number.";
            return false;
        }

        return true;
    }
}

