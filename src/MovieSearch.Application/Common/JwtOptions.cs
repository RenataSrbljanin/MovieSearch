namespace MovieSearch.Application.Common;

// Premestila sam klasu ovde kako bi bila dostupna Application sloju bez kršenja pravila arhitekture
public class JwtOptions
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int TokenExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}