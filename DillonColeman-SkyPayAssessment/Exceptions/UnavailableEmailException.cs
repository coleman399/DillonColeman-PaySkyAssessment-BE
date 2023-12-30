namespace DillonColeman_SkyPayAssessment.Exceptions
{
    [Serializable]
    public class UnavailableEmailException : Exception
    {
        public UnavailableEmailException() : base("Email is already being used.") { }
    }
}
