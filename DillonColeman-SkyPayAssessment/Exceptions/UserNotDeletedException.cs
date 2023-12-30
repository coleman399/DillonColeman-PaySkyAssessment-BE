namespace DillonColeman_SkyPayAssessment.Exceptions
{
    [Serializable]
    public class UserNotDeletedException : Exception
    {
        public UserNotDeletedException(int id) : base($"User with id {id} was not deleted.") { }
    }
}
