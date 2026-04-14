namespace MovieSearch.Application.Dtos; 

public class LoginRequestDto
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

// Kreiram DTO za jedinstven odgovor nakon uspešne prijave ili osvežavanja
public class AuthResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

// DTO koji klijent šalje kada želi novi AccessToken
public class RefreshRequestDto
{
    public string RefreshToken { get; set; } = default!;
}