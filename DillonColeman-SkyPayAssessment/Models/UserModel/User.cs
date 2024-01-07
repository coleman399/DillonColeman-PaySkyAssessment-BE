using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DillonColeman_PaySkyAssessment.Models.UserModel
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public required string UserName { get; set; } = string.Empty;
        [Required]
        public required string PasswordHash { get; set; } = string.Empty;
        [Required, EmailAddress]
        public required string Email { get; set; } = string.Empty;
        [Required]
        public required string Role { get; set; } = string.Empty;
        [Required]
        public required string AccessToken { get; set; } = string.Empty;
        public RefreshToken? RefreshToken { get; set; }
        public ForgotPasswordToken? ForgotPasswordToken { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    }
}
