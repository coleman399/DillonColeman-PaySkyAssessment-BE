namespace DillonColeman_PaySkyAssessment.Exceptions
{
    [Serializable]
    public class InvalidUserNameException(string invalidUserName) : Exception("Invalid User Name : " + invalidUserName)
    {
    }
}

