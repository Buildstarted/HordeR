public static class GuidExtensions
{
    public static string ShortGuid(this Guid guid)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return Convert.ToBase64String(guid.ToByteArray()).Replace("=", "").Replace("+", chars[Random.Shared.Next(chars.Length)].ToString());
    }
}
