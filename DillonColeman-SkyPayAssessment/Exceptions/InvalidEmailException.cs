namespace DillonColeman_PaySkyAssessment.Exceptions
{
    [Serializable]
    public class InvalidEmailException(string invalidEmail) : Exception("Invalid Email : " + invalidEmail)
    {
    }
}

