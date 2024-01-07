namespace DillonColeman_PaySkyAssessment.Service.VacancyService
{
    public interface IVacancyService
    {
        public Task<VacancyServiceResponse<List<GetVacancyDto>>> GetVacancies();
        public Task<VacancyServiceResponse<GetVacancyDto>> GetVacancy(int id);
        public Task<VacancyServiceResponse<GetVacancyDto>> CreateVacancy(CreateVacancyDto vacancy);
        public Task<VacancyServiceResponse<GetVacancyDto>> UpdateVacancy(UpdateVacancyDto vacancy, int id);
        public Task<VacancyServiceResponse<DeleteVacancyDto>> DeleteVacancy(int id);
        public Task<VacancyServiceResponse<GetVacancyDto>> ApplyForVacancy(int id);
    }
}
