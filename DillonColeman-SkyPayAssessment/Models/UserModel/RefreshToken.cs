using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DillonColeman_PaySkyAssessment.Models.UserModel
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public required string Token { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddDays(1);
    }
}
