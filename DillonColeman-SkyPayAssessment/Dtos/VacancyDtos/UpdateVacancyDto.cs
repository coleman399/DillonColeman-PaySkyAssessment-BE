using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DillonColeman_PaySkyAssessment.Dtos.VacancyDtos
{
    public class UpdateVacancyDto
    {
        public int Volume { get; set; }
        [JsonIgnore, Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? ExpiresOn { get; set; }
    }
}
