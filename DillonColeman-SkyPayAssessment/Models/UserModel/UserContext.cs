namespace DillonColeman_PaySkyAssessment.Models.UserModel
{
    public class UserContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<ForgotPasswordToken> ForgotPasswordTokens { get; set; }

        public UserContext() { }

        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // write fluent API configurations here
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.PasswordHash)
                    .IsRequired();
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Role)
                    .IsRequired();
                entity.OwnsOne(e => e.ForgotPasswordToken);
                entity.OwnsOne(e => e.RefreshToken);
                entity.Property(e => e.AccessToken);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);
            });
        }
    }
}
