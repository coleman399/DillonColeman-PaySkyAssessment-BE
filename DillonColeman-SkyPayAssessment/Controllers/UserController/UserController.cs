using DillonColeman_SkyPayAssessment.Models.UserModel;
using DillonColeman_SkyPayAssessment.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DillonColeman_SkyPayAssessment.Controllers.UserController
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        // POST api/User/getUsers
        [HttpGet("getUsers"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserServiceResponse<List<GetUserDto>>>> GetUsers()
        {
            UserServiceResponse<List<GetUserDto>> result = await _userService.GetUsers();
            if (result.Success == false) return BadRequest(result);
            return Ok(result);
        }

        // POST api/User/register
        [HttpPost("register"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<GetUserDto>>> RegisterUser([FromBody] RegisterUserDto newUser)
        {
            UserServiceResponse<GetUserDto> result = await _userService.RegisterUser(newUser);
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Created("register", result);
        }

        // POST api/User/login
        [HttpPost("login"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<GetLoggedInUserDto>>> LoginUser([FromBody] LoginUserDto loginUser)
        {
            UserServiceResponse<GetLoggedInUserDto> result = await _userService.LoginUser(loginUser);
            if (result.Success == false) return Unauthorized(result);
            return Ok(result);
        }

        // PUT api/User/updateUser?id={id}
        [HttpPut("updateUser"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<GetLoggedInUserDto>>> UpdateUser([FromBody] UpdateUserDto user)
        {
            UserServiceResponse<GetLoggedInUserDto> result = await _userService.UpdateUser(user);
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Ok(result);
        }

        // DELETE api/User/deleteUser?id={id}
        [HttpDelete("deleteUser"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<DeleteUserDto>>> DeleteUser()
        {
            UserServiceResponse<DeleteUserDto> result = await _userService.DeleteUser();
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Ok(result);
        }

        // Post api/User/refreshToken
        [HttpPost("refreshToken"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<GetLoggedInUserDto>>> RefreshToken()
        {
            UserServiceResponse<GetLoggedInUserDto> result = await _userService.RefreshToken();
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Ok(result);
        }

        // Post api/User/logout
        [HttpPost("logout"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<GetLoggedOutUserDto>>> LogoutUser()
        {
            UserServiceResponse<GetLoggedOutUserDto> result = await _userService.LogoutUser();
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Ok(result);

        }

        // Post api/User/forgotPassword
        [HttpPost("forgotPassword"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserServiceResponse<GetForgotPasswordUserDto>>> ForgotPassword(ForgotPasswordUserDto user)
        {
            UserServiceResponse<GetForgotPasswordUserDto> result = await _userService.ForgotPassword(user);
            if (result.Success == false) return BadRequest(result);
            return Ok(result);
        }

        // Post api/User/resetPasswordConfirmation
        [HttpPost("resetPasswordConfirmation"), AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<GetResetPasswordUserDto>>> ResetPasswordConfirmation([FromQuery] string token)
        {
            UserServiceResponse<GetResetPasswordUserDto> result = await _userService.ResetPasswordConfirmation(token);
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Ok(result);
        }

        // Post api/User/resetPassword
        [HttpPost("resetPassword"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserServiceResponse<PasswordResetUserDto>>> ResetPassword([FromBody] ResetPasswordUserDto resetPassword)
        {
            UserServiceResponse<PasswordResetUserDto> result = await _userService.ResetPassword(resetPassword);
            if (result.Success == false) return BadRequest(result);
            if (result.Data == null && result.Success == true) return Unauthorized();
            return Ok(result);
        }
    }
}
