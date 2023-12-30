namespace DillonColeman_SkyPayAssessment.Exceptions
{
    [Serializable]
    public class InvalidEmailException(string invalidEmail) : Exception("Invalid Email : " + invalidEmail)
    {
    }
}

