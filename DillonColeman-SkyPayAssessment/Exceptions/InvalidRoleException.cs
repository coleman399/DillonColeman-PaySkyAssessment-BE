namespace DillonColeman_SkyPayAssessment.Exceptions
{
    [Serializable]
    public class InvalidRoleException(string invalidRole) : Exception("Invalid Role: " + invalidRole + ". Valid roles include Admin and User.")
    {
    }
}
