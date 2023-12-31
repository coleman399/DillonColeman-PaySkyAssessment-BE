namespace DillonColeman_SkyPayAssessment.Models.VacancyModel
{
    public class VacancyContext : DbContext
    {
        public virtual DbSet<Vacancy> Vacancies { get; set; }
        public virtual DbSet<Applicant> Applicants { get; set; }

        public VacancyContext() { }

        public VacancyContext(DbContextOptions<VacancyContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // write fluent API configurations here
            modelBuilder.Entity<Vacancy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Applicates);
                entity.Property(e => e.Volume)
                    .IsRequired();
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.ExpiresOn);
            });
        }
    }
}
