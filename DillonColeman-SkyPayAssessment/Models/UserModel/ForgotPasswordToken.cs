using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DillonColeman_SkyPayAssessment.Models.UserModel
{
    public class ForgotPasswordToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public required string Token { get; set; }
        public bool IsValidated { get; set; } = false;
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
