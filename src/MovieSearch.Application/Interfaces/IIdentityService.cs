using MovieSearch.Application.Dtos;

public interface IIdentityService
{
    // Metoda koja vraća JWT string ako su kredencijali ispravni, inače null
    string? Authenticate(LoginRequestDto request);
}   