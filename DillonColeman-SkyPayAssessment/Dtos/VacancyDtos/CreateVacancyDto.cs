using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DillonColeman_PaySkyAssessment.Dtos.VacancyDtos
{
    public class CreateVacancyDto
    {
        [Required]
        public int Volume { get; set; }
        [JsonIgnore, Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? ExpiresOn { get; set; }
    }
}
