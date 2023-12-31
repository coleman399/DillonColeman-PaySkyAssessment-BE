using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DillonColeman_SkyPayAssessment.Dtos.VacancyDtos
{
    public class UpdateVacancyDto
    {
        public int Volume { get; set; }
        [JsonIgnore, Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? ExpiresOn { get; set; }
    }
}
