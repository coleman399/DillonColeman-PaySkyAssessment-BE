namespace DillonColeman_SkyPayAssessment.Exceptions
{
    [Serializable]
    public class InvalidPasswordException(string invalidPassword) : Exception("Invalid Password : " + invalidPassword + ". Password must be at least 8 characters in length and contain 1 lowercase letter, 1 uppercase letter, and 1 digit.")
    {
    }
}
