namespace incidenskezelo_backend.Configuration;

public class JwtOptions
{
    public const int MinKeyLength = 32;

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpireHours { get; set; } = 8;
}
