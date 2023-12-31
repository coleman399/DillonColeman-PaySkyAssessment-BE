using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DillonColeman_SkyPayAssessment.Models.VacancyModel
{
    public class Applicant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.Now;
        public int TimesApplied { get; set; }
        [ForeignKey("VacancyId"), JsonIgnore]
        public int VacancyId { get; set; }
    }
}
