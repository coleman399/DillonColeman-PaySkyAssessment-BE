namespace DillonColeman_SkyPayAssessment.Exceptions
{
    [Serializable]
    public class InvalidUserNameException(string invalidUserName) : Exception("Invalid User Name : " + invalidUserName)
    {
    }
}

