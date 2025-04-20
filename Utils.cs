using System.Text.RegularExpressions;

public class Utils
{
    public static bool IsValidTelegramUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        string pattern = @"^[a-zA-Z][a-zA-Z0-9_]{4,31}$";
        return Regex.IsMatch(username, pattern);
    }
}