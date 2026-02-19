namespace Common;

public static class UserStore
{
    private static readonly Dictionary<string, string> Users = new()
    {
        ["alice"] = CryptoUtils.Hash("alice123"),
        ["bob"]   = CryptoUtils.Hash("bob123"),
        ["zahar"] = CryptoUtils.Hash("zahar123")
    };

    public static bool Validate(string user, string password)
    {
        return Users.TryGetValue(user, out var hash) &&
               CryptoUtils.Hash(password) == hash;
    }
}