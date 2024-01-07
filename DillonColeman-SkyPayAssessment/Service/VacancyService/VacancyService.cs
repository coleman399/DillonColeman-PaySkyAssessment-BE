using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DillonColeman_PaySkyAssessment.Service.VacancyService
{
    public class VacancyService(VacancyContext vacancyContext, IMapper mapper, IHttpContextAccessor httpContextAccessor, UserContext userContext) : IVacancyService
    {
        private readonly VacancyContext _vacancyContext = vacancyContext;
        private readonly IMapper _mapper = mapper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserContext _userContext = userContext;

        public User TokenCheck()
        {
            IEnumerable<Claim> claims;
            if (_httpContextAccessor.HttpContext!.User.Identity is ClaimsIdentity identity)
            {
                claims = identity.Claims;
            }
            else
            {
                throw new UserNotFoundException();
            }
            var userId = int.Parse(claims.First(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            string accessToken = _httpContextAccessor.HttpContext.Request.Headers.Authorization!;
            var dbUser = _userContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new UserNotFoundException(userId);
            if (dbUser.AccessToken != accessToken.Remove(0, 7)) throw new UnauthorizedAccessException();
            if (accessToken.IsNullOrEmpty()) throw new UnauthorizedAccessException();
            return dbUser;
        }

        public async Task<VacancyServiceResponse<GetVacancyDto>> ApplyForVacancy(int id)
        {
            var serviceResponse = new VacancyServiceResponse<GetVacancyDto>() { Data = null };
            try
            {
                User user = TokenCheck();
                Vacancy? vacancy = await _vacancyContext.Vacancies.FirstAsync(v => v.Id == id);
                if (vacancy.Equals(null))
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Vacancy not found.";
                    return serviceResponse;
                }
                else
                {
                    if (vacancy.ExpiresOn < DateTime.Now)
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = "Vacancy has expired.";
                        return serviceResponse;
                    }
                    if (vacancy.Volume == 0)
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = "Vacancy is full.";
                        return serviceResponse;
                    }
                    bool isApplicant = false;
                    foreach (var applicant in _vacancyContext.Applicants)
                    {
                        if (applicant.VacancyId == id && applicant.UserId == user.Id)
                        {
                            isApplicant = true;
                            if (applicant.AppliedDate > DateTime.Now.AddHours(-24))
                            {
                                serviceResponse.Success = false;
                                serviceResponse.Message = "You have already applied for this vacancy within the last 24 hours.";
                                return serviceResponse;
                            }
                            else
                            {
                                applicant.AppliedDate = DateTime.Now;
                                applicant.TimesApplied += applicant.TimesApplied;
                                vacancy.Volume -= 1;
                                _vacancyContext.Vacancies.Update(vacancy);
                                _vacancyContext.Applicants.UpdateRange(vacancy.Applicates);
                                await _vacancyContext.SaveChangesAsync();
                                serviceResponse.Success = true;
                                serviceResponse.Message = "You have successfully applied for this vacancy.";
                                return serviceResponse;
                            }
                        }
                    }
                    if (!isApplicant)
                    {
                        vacancy.Applicates.Add(new Applicant() { UserId = user.Id, TimesApplied = 1, VacancyId = id });
                        vacancy.Volume -= 1;
                        _vacancyContext.Vacancies.Update(vacancy);
                        _vacancyContext.Applicants.UpdateRange(vacancy.Applicates);
                        await _vacancyContext.SaveChangesAsync();
                        serviceResponse.Data = _mapper.Map<GetVacancyDto>(vacancy);
                        serviceResponse.Message = "You have successfully applied for this vacancy.";
                    }
                    return serviceResponse;
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
                return serviceResponse;
            }
        }

        public async Task<VacancyServiceResponse<GetVacancyDto>> CreateVacancy(CreateVacancyDto vacancy)
        {
            var serviceResponse = new VacancyServiceResponse<GetVacancyDto>() { Data = null };
            try
            {
                var newVacancy = _mapper.Map<Vacancy>(vacancy);
                _vacancyContext.Vacancies.Add(newVacancy);
                await _vacancyContext.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetVacancyDto>(newVacancy);
                serviceResponse.Message = "Vacancy created successfully.";
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<VacancyServiceResponse<DeleteVacancyDto>> DeleteVacancy(int id)
        {
            var serviceResponse = new VacancyServiceResponse<DeleteVacancyDto>() { Data = null };
            try
            {
                Vacancy? vacancy = await _vacancyContext.Vacancies.FirstAsync(v => v.Id == id);
                if (vacancy == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Vacancy not found.";
                }
                else
                {
                    vacancy.Applicates.ToList().ForEach(a => a.VacancyId = id);
                    _vacancyContext.Applicants.RemoveRange(vacancy.Applicates);
                    _vacancyContext.Vacancies.Update(vacancy);
                    await _vacancyContext.SaveChangesAsync();
                    vacancy = await _vacancyContext.Vacancies.FirstAsync(v => v.Id == vacancy.Id);
                    _vacancyContext.Vacancies.Remove(vacancy);
                    await _vacancyContext.SaveChangesAsync();
                    serviceResponse.Message = "Vacancy deleted successfully.";
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<VacancyServiceResponse<List<GetVacancyDto>>> GetVacancies()
        {
            var serviceResponse = new VacancyServiceResponse<List<GetVacancyDto>>() { Data = null };
            try
            {
                var vacancies = await _vacancyContext.Vacancies.ToListAsync();
                serviceResponse.Data = vacancies.Select(_mapper.Map<GetVacancyDto>).ToList();
                serviceResponse.Data.ForEach(vacancy => vacancy.Applicates = [.. _vacancyContext.Applicants.Where(a => a.VacancyId == vacancy.Id)]);
                serviceResponse.Message = "Vacancies retrieved successfully.";
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<VacancyServiceResponse<GetVacancyDto>> GetVacancy(int id)
        {
            var serviceResponse = new VacancyServiceResponse<GetVacancyDto>() { Data = null };
            try
            {
                var vacancy = await _vacancyContext.Vacancies.FirstOrDefaultAsync(v => v.Id == id);
                if (vacancy == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Vacancy not found.";
                }
                else
                {
                    serviceResponse.Data = _mapper.Map<GetVacancyDto>(vacancy);
                    serviceResponse.Data.Applicates = [.. _vacancyContext.Applicants.Where(a => a.VacancyId == vacancy.Id)];
                    serviceResponse.Message = "Vacancy retrieved successfully.";
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<VacancyServiceResponse<GetVacancyDto>> UpdateVacancy(UpdateVacancyDto vacancy, int id)
        {
            var serviceResponse = new VacancyServiceResponse<GetVacancyDto>() { Data = null };
            try
            {
                Vacancy? dbVacancy = await _vacancyContext.Vacancies.FirstAsync(v => v.Id == id);
                if (dbVacancy.Equals(null))
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Vacancy not found.";
                }
                else
                {
                    try
                    {
                        var charArray = vacancy.ExpiresOn!.ToCharArray();
                        var date = DateTime.Parse(charArray);
                        dbVacancy.ExpiresOn = date;
                    }
                    catch
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = "Invalid Expiration Date";
                        return serviceResponse;
                    }
                    dbVacancy.Id = id;
                    dbVacancy.Volume = vacancy.Volume;
                    dbVacancy.UpdatedAt = DateTime.Now;
                    _vacancyContext.Vacancies.Update(dbVacancy);
                    await _vacancyContext.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetVacancyDto>(dbVacancy);
                    serviceResponse.Message = "Vacancy updated successfully.";
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }
    }
}
