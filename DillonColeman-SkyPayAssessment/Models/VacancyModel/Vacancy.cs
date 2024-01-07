using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DillonColeman_PaySkyAssessment.Models.VacancyModel
{
    public class Vacancy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public List<Applicant> Applicates { get; set; } = [];
        [Required]
        public int Volume { get; set; } = 0;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
        [Required]
        public DateTime ExpiresOn { get; set; } = DateTime.MinValue;
    }
}
