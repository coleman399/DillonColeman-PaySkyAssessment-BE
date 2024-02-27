namespace DillonColeman_PaySkyAssessment.Exceptions
{
    [Serializable]
    public class InvalidRoleException(string invalidRole) : Exception("Invalid Role: " + invalidRole + ". Valid roles include Employer or Applicant.")
    {
    }
}
