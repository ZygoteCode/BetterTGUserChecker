using JA3;

public class Program
{
    public static void Main()
    {
        Console.Title = "BetterTGUserChecker | Made by https://github.com/GabryB03/";

        if (!File.Exists("users.txt"))
        {
            Logger.LogError("The file 'users.txt' does not exist. Please, create and fill it with all the usernames to check.");
            goto exit;
        }

        Logger.LogSuccess("Succesfully found the file 'users.txt', parsing.");
        string[] users = File.ReadAllLines("users.txt");

        if (users.Length == 0)
        {
            Logger.LogError("No usernames are found in the file 'users.txt'.");
            goto exit;
        }

        List<string> validUsernames = new List<string>();

        foreach (string user in users)
        {
            if (Utils.IsValidTelegramUsername(user))
            {
                validUsernames.Add(user);
            }
        }

        if (validUsernames.Count == 0)
        {
            Logger.LogError($"Loaded {users.Length} usernames from the file 'users.txt' but no one of the loaded usernames has a valid format.");
            goto exit;
        }

        Logger.LogSuccess("Succesfully parsed all usernames from 'users.txt'.");
        Logger.LogInfo($"Usernames loaded: {users.Length}, real valid usernames to check: {validUsernames.Count}");

        string _validUsernames = "", _invalidUsernames = "";
        int _validUsernamesCount = 0, _invalidUsernamesCount = 0;
        ResourceSemaphore _validUsernamesSemaphore = new ResourceSemaphore(),
            _invalidUsernamesSemaphore = new ResourceSemaphore();

        foreach (string validUsername in validUsernames)
        {
            new Thread(() =>
            {
                Ja3MessageHandler handler = new Ja3MessageHandler();

                using (HttpClient client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
                    string response = client.GetStringAsync($"https://t.me/{validUsername}/").GetAwaiter().GetResult();
                    bool valid = false;

                    if (!response.Contains("tgme_icon_user"))
                    {
                        valid = true;
                    }

                    if (valid)
                    {
                        while (!_validUsernamesSemaphore.IsResourceAvailable())
                        {
                            Thread.Sleep(1);
                        }

                        _validUsernamesSemaphore.LockResource();
                        Interlocked.Increment(ref _validUsernamesCount);

                        if (_validUsernames == "")
                        {
                            _validUsernames = validUsername;
                        }
                        else
                        {
                            _validUsernames += "\r\n" + validUsername;
                        }

                        _validUsernamesSemaphore.UnlockResource();
                    }
                    else
                    {
                        while (!_invalidUsernamesSemaphore.IsResourceAvailable())
                        {
                            Thread.Sleep(1);
                        }

                        _invalidUsernamesSemaphore.LockResource();
                        Interlocked.Increment(ref _invalidUsernamesCount);

                        if (_invalidUsernames == "")
                        {
                            _invalidUsernames = validUsername;
                        }
                        else
                        {
                            _invalidUsernames += "\r\n" + validUsername;
                        }

                        _invalidUsernamesSemaphore.UnlockResource();
                    }
                }
            }).Start();
        }

        while ((_validUsernamesCount + _invalidUsernamesCount) != validUsernames.Count)
        {
            Thread.Sleep(1);
        }

        File.WriteAllText("valid.txt", _validUsernames);
        File.WriteAllText("invalid.txt", _invalidUsernames);

        Logger.LogSuccess($"Succesfully finished checking all the {validUsernames.Count} usernames.");
        Logger.LogSuccess($"Valid & existing usernames: {_validUsernamesCount}. Saved to 'valid.txt' file.");
        Logger.LogSuccess($"Invalid & non-existing usernames: {_invalidUsernamesCount}. Saved to 'invalid.txt' file.");

    exit: Logger.LogWarning("Press ENTER to exit from the program.");
        Console.ReadLine();
        return;
    }
}