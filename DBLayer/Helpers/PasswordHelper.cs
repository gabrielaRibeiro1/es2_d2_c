namespace ESOF.WebApp.DBLayer.Helpers;

public static class PasswordHelper
{
    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    }
    // Função para verificar a senha (caso esteja armazenada em texto plano)
    public static bool VerifyPasswordNoHash(string enteredPassword, string storedPassword)
    {
        return enteredPassword == storedPassword;
    }
    public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(storedHash);
    }
}
