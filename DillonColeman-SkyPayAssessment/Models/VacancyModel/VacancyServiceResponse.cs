namespace DillonColeman_SkyPayAssessment.Models.VacancyModel
{
    public class VacancyServiceResponse<T>
    {
        public T? Data { get; set; }

        public bool Success { get; set; } = true;

        public string? Message { get; set; } = string.Empty;
    }
}
