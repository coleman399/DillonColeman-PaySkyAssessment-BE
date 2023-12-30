using DillonColeman_SkyPayAssessment.Exceptions;
using DillonColeman_SkyPayAssessment.Models.UserModel;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DillonColeman_SkyPayAssessment.Service
{
    public class UserService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, UserContext userContext, IMapper mapper) : IUserService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserContext _userContext = userContext;
        private readonly IMapper _mapper = mapper;

        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSecurityKey"]!));
            TokenValidationParameters parameters = new()
            {
                ValidateIssuerSigningKey = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };
            return tokenHandler.ValidateToken(token, parameters, out _);
        }

        private string CreateAccessToken(User user)
        {
            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
            ];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSecurityKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(7).AddSeconds(1),
                //Expires = DateTime.Now.AddYears(5),
                SigningCredentials = creds
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);
            return accessToken ?? throw new Exception("Access token could not be created.");
        }

        private static RefreshToken CreateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
            };
            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.ExpiresAt,

            };
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }

        private string CreateForgotPasswordToken(User user)
        {
            List<Claim> claims =
            [
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
            ];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSecurityKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Expires = DateTime.Now.AddYears(5),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var forgotPasswordToken = tokenHandler.WriteToken(token);
            return forgotPasswordToken ?? throw new Exception("Forgot password token could not be created.");
        }

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

        public async Task<UserServiceResponse<List<GetUserDto>>> GetUsers()
        {
            var serviceResponse = new UserServiceResponse<List<GetUserDto>>() { Data = null };
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    List<User> dbUsers = await _userContext.Users.ToListAsync();
                    serviceResponse.Data = dbUsers.Select(user => _mapper.Map<GetUserDto>(user)).ToList();
                }
                else
                {
                    throw new HttpContextFailureException();
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetUserDto>> RegisterUser(RegisterUserDto newUser)
        {
            var serviceResponse = new UserServiceResponse<GetUserDto>() { Data = null };
            try
            {
                // Check if user is valid
                if (!RegexFilters.IsValidUserName(newUser.UserName!)) throw new InvalidUserNameException(newUser.UserName!);
                if (!RegexFilters.IsValidPassword(newUser.Password!)) throw new InvalidPasswordException(newUser.Password!);
                if (!RegexFilters.IsValidEmail(newUser.Email!)) throw new InvalidEmailException(newUser.Email!);

                // Check if user role is valid
                bool validRole = false;
                foreach (var role in Enum.GetValues(typeof(Roles)))
                {
                    if (role.ToString() == newUser.Role)
                    {
                        validRole = true;
                        break;
                    }
                }
                if (!validRole) throw new InvalidRoleException(newUser.Role);

                // Check if email or user name are already being used
                await _userContext.Users.ForEachAsync(u =>
                {
                    if (u.Email == newUser.Email) throw new UnavailableEmailException();
                    if (u.UserName == newUser.UserName) throw new UnavailableUserNameException();
                });

                // Create user
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                var createdUser = _mapper.Map<User>(newUser);

                // Save user
                _userContext.Users.Add(createdUser);
                _userContext.SaveChanges();

                // Check if user was saved
                List<User> dbUsers = await _userContext.Users.ToListAsync();
                createdUser = dbUsers.FirstOrDefault(u => u.Email == newUser.Email)! ?? throw new UserNotSavedException();

                // Return user with updated response
                serviceResponse.Data = _mapper.Map<GetUserDto>(createdUser);
                serviceResponse.Message = "User registered successfully";
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetLoggedInUserDto>> LoginUser(LoginUserDto loginUser)
        {
            var serviceResponse = new UserServiceResponse<GetLoggedInUserDto>() { Data = null };
            try
            {
                bool userFound = false;
                bool userVerified = false;
                List<User> dbUsers = await _userContext.Users.ToListAsync();
                User? dbUser = null;
                foreach (User user in dbUsers)
                {
                    if (loginUser.Email != null && user.Email == loginUser.Email || loginUser.UserName != null && user.UserName == loginUser.UserName)
                    {
                        userFound = true;
                        if (BCrypt.Net.BCrypt.Verify(loginUser.Password, user.PasswordHash))
                        {
                            userVerified = true;
                            user.AccessToken = CreateAccessToken(user);
                            user.RefreshToken = CreateRefreshToken(user);
                            SetRefreshToken(user.RefreshToken);
                            _userContext.Users.Update(user);
                            _userContext.SaveChanges();
                            serviceResponse.Data = _mapper.Map<GetLoggedInUserDto>(user);
                            dbUser = user;
                        }
                        else
                        {
                            throw new UnauthorizedAccessException();
                        }
                    }
                    else
                    {
                        throw new UserNotFoundException();
                    }
                }

                if (userFound && userVerified)
                {
                    // Check if tokens were saved
                    if (dbUser == null) throw new UserFailedToUpdateException();
                    serviceResponse.Message = "User logged in successfully.";
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetLoggedOutUserDto>> LogoutUser()
        {
            var serviceResponse = new UserServiceResponse<GetLoggedOutUserDto>() { Data = null };
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var user = TokenCheck();

                    // delete access and refresh token
                    user.AccessToken = string.Empty;
                    user.RefreshToken = null;
                    _userContext.Users.Update(user);
                    _userContext.SaveChanges();
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshTokenId");

                    // Verify user's token was updated
                    List<User> dbUsers = await _userContext.Users.ToListAsync();
                    user = _userContext.Users.FirstOrDefault(u => u.Id == user.Id) ?? throw new UserNotFoundException(user.Id);
                    if (user.AccessToken != string.Empty) throw new UserFailedToUpdateException("AccessToken failed to update.");
                    if (user.RefreshToken != null) throw new UserFailedToUpdateException("RefreshToken failed to update.");

                    // update response
                    serviceResponse.Data = new GetLoggedOutUserDto();
                    serviceResponse.Message = "User logged out successfully.";
                }
                else
                {
                    throw new HttpContextFailureException();
                }

            }
            catch (Exception exception)
            {
                serviceResponse.Success = true;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetLoggedInUserDto>> UpdateUser(UpdateUserDto updateUser)
        {
            var serviceResponse = new UserServiceResponse<GetLoggedInUserDto>() { Data = null };
            try
            {
                //Check if update user is valid
                if (!RegexFilters.IsValidUserName(updateUser.UserName!)) throw new InvalidUserNameException(updateUser.UserName!);
                if (!RegexFilters.IsValidPassword(updateUser.Password!)) throw new InvalidPasswordException(updateUser.Password!);
                if (!RegexFilters.IsValidEmail(updateUser.Email!)) throw new InvalidEmailException(updateUser.Email!);

                if (_httpContextAccessor.HttpContext != null)
                {
                    var user = TokenCheck();

                    var dbUsers = await _userContext.Users.ToListAsync();

                    //Check if email or user name are already being used
                    dbUsers.ForEach(u =>
                    {
                        if (u.Email == updateUser.Email)
                        {
                            if (u.Id != user.Id) throw new UnavailableEmailException();
                        }

                        if (u.UserName == updateUser.UserName)
                        {
                            if (u.Id != user.Id) throw new UnavailableUserNameException();
                        }
                    });

                    // Update user 
                    // BCrypt Note: Password needs to be stored in a new variable before updating user
                    //   -hopefully that will change in the future
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(updateUser.Password);
                    user.PasswordHash = passwordHash;
                    _userContext.Users.Update(_mapper.Map(updateUser, user));
                    _userContext.SaveChanges();

                    // Verify user was updated
                    dbUsers = await _userContext.Users.ToListAsync();
                    user = dbUsers.FirstOrDefault(u => u.Id == user.Id) ?? throw new UserNotFoundException(user.Id);
                    if (user.Email != updateUser.Email && user.UserName != updateUser.UserName || !BCrypt.Net.BCrypt.Verify(updateUser.Password, user.PasswordHash)) throw new UserFailedToUpdateException();

                    // Sign user out
                    // delete access and refresh token
                    user.AccessToken = string.Empty;
                    user.RefreshToken = null;
                    _userContext.Users.Update(user);
                    _userContext.SaveChanges();
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshToken");
                    _httpContextAccessor.HttpContext.Response.Cookies.Delete("refreshTokenId");

                    // Verify user's tokens were deleted
                    dbUsers = await _userContext.Users.ToListAsync();
                    user = _userContext.Users.FirstOrDefault(u => u.Id == user.Id) ?? throw new UserNotFoundException(user.Id);
                    if (user.AccessToken != string.Empty) throw new UserFailedToUpdateException("Failed to sign out.");
                    if (user.RefreshToken != null) throw new UserFailedToUpdateException("Failed to sign out.");

                    // update response
                    serviceResponse.Data = new GetLoggedInUserDto();
                    serviceResponse.Message = "Account updated successfully. User logged out.";
                    return serviceResponse;
                }
                else
                {
                    throw new HttpContextFailureException();
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<DeleteUserDto>> DeleteUser()
        {
            var serviceResponse = new UserServiceResponse<DeleteUserDto>() { Data = null };
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var user = TokenCheck();

                    // Delete user
                    _userContext.Users.Remove(user);
                    _userContext.SaveChanges();

                    // Verify user was deleted
                    await _userContext.Users.ForEachAsync(c =>
                    {
                        if (c.Id == user.Id) throw new UserNotDeletedException(user.Id);
                    });

                    // Update response
                    serviceResponse.Data = new DeleteUserDto();
                    serviceResponse.Message = "User deleted successfully.";
                }
                else
                {
                    throw new HttpContextFailureException();
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetLoggedInUserDto>> RefreshToken()
        {
            var serviceResponse = new UserServiceResponse<GetLoggedInUserDto>() { Data = null };
            try
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var user = TokenCheck();

                    // Update user's token and refresh token
                    var newAccessToken = CreateAccessToken(user);
                    var newRefreshToken = CreateRefreshToken(user);
                    user.AccessToken = newAccessToken;
                    user.RefreshToken = newRefreshToken;
                    _userContext.Users.Update(user);
                    _userContext.SaveChanges();

                    // Verify user's token was updated
                    List<User> dbUsers = await _userContext.Users.ToListAsync();
                    user = _userContext.Users.FirstOrDefault(u => u.Id == user.Id) ?? throw new UserNotFoundException(user.Id);
                    if (user.AccessToken != newAccessToken) throw new UserFailedToUpdateException("AccessToken failed to update.");
                    if (user.RefreshToken!.Id != newRefreshToken.Id) throw new UserFailedToUpdateException("RefreshToken failed to update.");
                    if (user.RefreshToken.User != newRefreshToken.User) throw new UserFailedToUpdateException("RefreshToken failed to update.");
                    if (user.RefreshToken.UserId != newRefreshToken.UserId) throw new UserFailedToUpdateException("RefreshToken failed to update.");
                    if (user.RefreshToken.Token != newRefreshToken.Token) throw new UserFailedToUpdateException("RefreshToken failed to update.");
                    if (user.RefreshToken.ExpiresAt != newRefreshToken.ExpiresAt) throw new UserFailedToUpdateException("RefreshToken failed to update.");
                    if (user.RefreshToken.CreatedAt != newRefreshToken.CreatedAt) throw new UserFailedToUpdateException("RefreshToken failed to update.");

                    // Update response
                    SetRefreshToken(user.RefreshToken);
                    serviceResponse.Data = _mapper.Map<GetLoggedInUserDto>(user);
                    serviceResponse.Data.Token = user.AccessToken;
                }
                else
                {
                    throw new HttpContextFailureException();
                }
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetForgotPasswordUserDto>> ForgotPassword(ForgotPasswordUserDto user)
        {
            var serviceResponse = new UserServiceResponse<GetForgotPasswordUserDto>() { Data = null };
            string http = "https://";
            try
            {

                // Verify user exists
                List<User> dbUsers = await _userContext.Users.ToListAsync(); ;
                var dbUser = (user.UserName.IsNullOrEmpty() ? dbUsers.FirstOrDefault(u => u.Email == user.Email) : dbUsers.FirstOrDefault(u => u.UserName == user.UserName)) ?? dbUsers.FirstOrDefault(u => u.Email == user.Email) ?? throw new UserNotFoundException();

                // Generate token
                var token = CreateForgotPasswordToken(dbUser);

                // Save token
                var forgotPasswordToken = new ForgotPasswordToken
                {
                    Token = token,
                    UserId = dbUser.Id,
                    User = dbUser,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(30),
                };
                dbUser.ForgotPasswordToken = forgotPasswordToken;
                _userContext.Users.Update(dbUser);
                _userContext.SaveChanges();

                //Verify token was saved
                dbUsers = await _userContext.Users.ToListAsync();
                dbUser = dbUsers.FirstOrDefault(u => u.Id == dbUser.Id)!;
                if (dbUser!.ForgotPasswordToken!.Token != token) throw new UserFailedToUpdateException("ForgotPasswordToken failed to update.");
                string baseUrl = _httpContextAccessor.HttpContext!.Request.GetDisplayUrl().Split("/")[2];
                if (baseUrl.IsNullOrEmpty()) throw new HttpContextFailureException();

                // Update response
                serviceResponse.Data = new GetForgotPasswordUserDto();
                serviceResponse.Message = $"Navigate to {http}{baseUrl}/api/User/resetPasswordConfirmation?token={token}";
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<GetResetPasswordUserDto>> ResetPasswordConfirmation(string token)
        {
            var serviceResponse = new UserServiceResponse<GetResetPasswordUserDto>() { Data = null };
            string http = "https://";
            try
            {
                // Validate token
                ClaimsPrincipal claimsPrincipal;
                try
                {
                    claimsPrincipal = ValidateToken(token);
                }
                catch
                {
                    return serviceResponse;
                }

                // Find user
                List<User> dbUsers = await _userContext.Users.ToListAsync();
                var dbUser = dbUsers.FirstOrDefault(u => claimsPrincipal.FindFirstValue(ClaimTypes.Email) == u.Email) ?? dbUsers.FirstOrDefault(u => claimsPrincipal.FindFirstValue(ClaimTypes.Name) == u.UserName) ?? throw new UserNotFoundException();

                // Validate that Users ResetPasswordToken is the same as the incoming token then set it to validated
                if (dbUser.ForgotPasswordToken == null || !dbUser.ForgotPasswordToken.Token.Equals(token) || dbUser.ForgotPasswordToken.ExpiresAt < DateTime.Now)
                {
                    return serviceResponse;
                }
                else
                {
                    dbUser.ForgotPasswordToken.IsValidated = true;
                    _userContext.Users.Update(dbUser);
                    _userContext.SaveChanges();
                }

                // Verify that the token was set to validated
                dbUsers = await _userContext.Users.ToListAsync();
                dbUser = dbUsers.FirstOrDefault(u => dbUser.Id == u.Id)!;
                if (!dbUser!.ForgotPasswordToken!.IsValidated) throw new UserFailedToUpdateException();

                // Update Response
                dbUser.AccessToken = CreateAccessToken(dbUser);
                serviceResponse.Data = new GetResetPasswordUserDto() { Token = dbUser.AccessToken };
                string baseUrl = _httpContextAccessor.HttpContext!.Request.GetDisplayUrl().Split("/")[2];
                serviceResponse.Message = $"Reset Password Confirmation Operation Complete. Navigate to {http}{baseUrl}/api/User/resetPassword";
            }
            catch (Exception exception)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"{exception.Message}  {exception}";
            }
            return serviceResponse;
        }

        public async Task<UserServiceResponse<PasswordResetUserDto>> ResetPassword(ResetPasswordUserDto resetPasswordDto)
        {
            var serviceResponse = new UserServiceResponse<PasswordResetUserDto>() { Data = null };
            try
            {
                if (!RegexFilters.IsValidPassword(resetPasswordDto.Password)) throw new InvalidPasswordException(resetPasswordDto.Password);

                if (_httpContextAccessor.HttpContext != null)
                {
                    // Find user
                    var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UserNotFoundException());
                    var dbUser = _userContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new UserNotFoundException(userId);

                    // Update password
                    dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.Password);
                    dbUser.AccessToken = string.Empty;
                    _userContext.Users.Update(dbUser);
                    _userContext.SaveChanges();

                    // Verify user was saved
                    List<User> dbUsers = await _userContext.Users.ToListAsync();
                    dbUser = _userContext.Users.FirstOrDefault(u => u.Id == userId) ?? throw new UserNotFoundException(userId);
                    if (!BCrypt.Net.BCrypt.Verify(resetPasswordDto.Password, dbUser.PasswordHash)) throw new UserFailedToUpdateException();

                    // Update response
                    serviceResponse.Data = new PasswordResetUserDto();
                    serviceResponse.Message = "Reset Password Operation Complete.";
                }
                else
                {
                    throw new HttpContextFailureException();
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
