using MovieSearch.Application.Dtos;

public interface IIdentityService
{
    // Ažuriram metodu da vraća AuthResponseDto koji sada sadrži oba tokena
    Task<AuthResponseDto?> AuthenticateAsync(LoginRequestDto request);
    
    // Dodajem metodu za validaciju i generisanje novog para tokena na osnovu starog refresh tokena
    Task<AuthResponseDto?> RefreshTokenAsync(RefreshRequestDto request);
}   