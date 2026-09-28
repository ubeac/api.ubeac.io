using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using uBeac.Idsrv.InputModels;
using uBeac.Idsrv.Models;
using uBeac.Idsrv.Services;
using uBeac.Models;
using uBeac.Web.Filters;

namespace uBeac.Idsrv.Controllers
{
    [Route("[controller]/[action]")]
    [TypeFilter(typeof(ResultSetResponseCodeFilter))]
    [TypeFilter(typeof(ModelStateValidationAsyncActionFilter))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly IUserProfileService _userProfileService;
        private readonly IConfiguration _configuration;

        public UserController(UserManager<User> userManager, 
                              SignInManager<User> signInManager, 
                              IEmailService emailService, 
                              ILogger<UserController> logger, 
                              IUserProfileService userProfileService,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
            _userProfileService = userProfileService;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultSet<Guid>> Register([FromBody] RegisterInputModel model)
        {
            var createDate = DateTime.UtcNow;
            ResultSet<Guid> resultSet;

            User user = new User { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Constants.ROLE_NORMAL_USERS);
                await _userManager.AddToRoleAsync(user, Constants.ROLE_REGISTERED_INTERNAL);

                if (user.UserName.ToLower().EndsWith("@momentaj.com") || user.UserName.ToLower().EndsWith("@ubeac.com") || user.UserName.ToLower().EndsWith("@ubeac.io"))
                    await _userManager.AddToRoleAsync(user, Constants.ROLE_ADMIN);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmUrl = Url.Action(action: nameof(UserController.ConfirmEmail), controller: "User", values: new { user.Email, code }, protocol: Request.Scheme);
                await _emailService.SendAccountConfirm(model.Email, HtmlEncoder.Default.Encode(confirmUrl));

                var userData = new UserProfile()
                {
                    Id = user.Id,
                    TimeZone = model.TimeZone,
                    TimeZoneOffset = model.TimeZoneOffset,
                    CreateDate = createDate,
                    UpdateDate = createDate,
                    LastActivityDate = createDate,
                    Username = user.UserName,
                    Email = user.Email
                };
                await _userProfileService.UpdateAsync(userData);

                resultSet = new ResultSet<Guid>(user.Id);
            }
            else
            {
                resultSet = new ResultSet<Guid>(Guid.Empty);
                foreach (var item in result.Errors)
                {
                    resultSet.AddError(new Error(item.Code, item.Description));                    
                }
            }
            return resultSet;
        }

        [HttpPost]
        public async Task<ResultSet<List<KeyValuePair<string, string>>>> Claims()
        {        

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                var resultSet = new ResultSet<List<KeyValuePair<string, string>>>();
                resultSet.AddError(new Error("InvalidClaims", "User is not logged in!"));
                return resultSet;
            }

            await _userProfileService.UpdateLastActivityDate(user.Id);

            var claims = (from p in User.Claims select new KeyValuePair<string, string>(p.Type, p.Value)).ToList();
            return await Task.FromResult(new ResultSet<List<KeyValuePair<string, string>>>(claims));
        }

        [HttpPost]
        public async Task<ResultSet<bool>> ChangePassword([FromBody]  ChangePasswordInputModel model)
        {
            var resultSet = new ResultSet<bool>(false);

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                resultSet.AddError(new Error("InvalidClaims", "User is not logged in!"));
                return resultSet;
            }

            //if (!user.EmailConfirmed)
            //{
            //    resultSet.AddError(new Error("EmailNotVerified", "User's email address is not verified!"));
            //    return resultSet;
            //}

            await _userProfileService.UpdateLastActivityDate(user.Id);

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            resultSet = new ResultSet<bool>(result.Succeeded);

            if (result.Succeeded)
            {
                await _emailService.SendChangePassword(user.Email);
            }

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    resultSet.AddError(new Error(item.Code, item.Description));
                    resultSet.Code = ResponseCodes.BadRequest;
                }
            }

            return resultSet;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string email, string code)
        {
            // Email confirmation link and reset/forgot password link expiration date is controlling by asp.net identity
            // and default expiration date for those is 1 day. The property name is *** TokenLifeSpan ***

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                throw new ApplicationException($"Unable to load user with email '{email}'.");
            }

            if (user.EmailConfirmed)
            {
                return Redirect(_configuration.GetValue<string>("PostConfirmEmailUrl"));
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                await _userProfileService.UpdateLastActivityDate(user.Id);
                return Redirect(_configuration.GetValue<string>("PostConfirmEmailUrl"));
            }
            throw new ApplicationException($"Unable to update email confirmation for user with email '{email}'.");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultSet<bool>> ForgotPassword([FromBody] ForgotPasswordInputModel model)
        {
            // Email confirmation link and reset/forgot password link expiration date is controlling by asp.net identity
            // and default expiration date for those is 1 day. The property name is *** TokenLifeSpan ***

            var resultSet = new ResultSet<bool>(true);
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                resultSet.AddError(new Error("EmailNotExist", "Email address does not exist!"));
                resultSet.Data = false;
                return resultSet;
            }

            if (!(await _userManager.IsEmailConfirmedAsync(user)))
            {
                resultSet.AddError(new Error("EmailNotConfirmed", "Email address is not confirmed!"));
                resultSet.Data = false;
                return resultSet;
            }

            if (resultSet.Errors.Count == 0)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var parametersToAdd = new Dictionary<string, string> {
                    { "email", user.Email} ,
                    { "code", code }
                };

                var callbackUrl = QueryHelpers.AddQueryString(_configuration.GetValue<string>("PostForgotPasswordUrl"), parametersToAdd);
                await _emailService.SendForgotPassword(user.Email, callbackUrl);

            }

            return resultSet;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultSet<bool>> ResetPassword([FromBody] ResetPasswordInputModel model)
        {
            // Email confirmation link and reset/forgot password link expiration date is controlling by asp.net identity
            // and default expiration date for those is 1 day. The property name is *** TokenLifeSpan ***

            var resultSet = new ResultSet<bool>(false);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                resultSet.AddError(new Error("EmailNotExist", "Email address does not exist!"));
                return resultSet;
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                resultSet.Code = ResponseCodes.OK;
                await _userProfileService.UpdateLastActivityDate(user.Id);
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    resultSet.AddError(new Error(item.Code, item.Description));
                    resultSet.Code = ResponseCodes.BadRequest;
                }
            }

            return resultSet;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultSet<bool>> Logout()
        {
            var resultSet = new ResultSet<bool>(true);
            await _signInManager.SignOutAsync();
            return resultSet;
        }

        [HttpPost]
        public async Task<ResultSet<bool>> ResendEmail()
        {
            var resultSet = new ResultSet<bool>(true);

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                resultSet.AddError(new Error("InvalidClaims", "User is not logged in!"));
                resultSet.Code = ResponseCodes.BadRequest;
                resultSet.Data = false;
                return resultSet;
            }

            await _userProfileService.UpdateLastActivityDate(user.Id);

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = Url.Action(action: nameof(UserController.ConfirmEmail), controller: "User", values: new { user.Email, code }, protocol: Request.Scheme);
            await _emailService.SendAccountConfirm(user.Email, HtmlEncoder.Default.Encode(confirmUrl));

            return resultSet;
        }

        [HttpPost]
        public async Task<ResultSet<UserProfile>> Profile()
        {
            var resultSet = new ResultSet<UserProfile>();

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                resultSet.AddError(new Error("InvalidClaims", "User is not logged in!"));
                resultSet.Code = ResponseCodes.UnAuthorized;
                return resultSet;
            }

            resultSet.Data = await _userProfileService.GetByIdAsync(user.Id);
            resultSet.Code = ResponseCodes.OK;
            return resultSet;

        }

        [HttpPost]
        public async Task<ResultSet<UserProfile>> Update([FromBody] UserProfileUpdateInputModel model)
        {
            var resultSet = new ResultSet<UserProfile>();
            var updateDate = DateTime.UtcNow;

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser is null)
            {
                resultSet.AddError(new Error("InvalidClaims", "User is not logged in!"));
                resultSet.Code = ResponseCodes.UnAuthorized;
                return resultSet;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber) && (model.PhoneNumber != currentUser.PhoneNumber))
            {
                currentUser.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(currentUser);
            }

            // todo: check here, it should be rewritten
            var userProfile = await _userProfileService.GetByIdAsync(currentUser.Id);

            userProfile.UpdateDate = updateDate;
            userProfile.PhoneNumber = currentUser.PhoneNumber;
            userProfile.FirstName = model.FirstName ?? null;
            userProfile.LastName = model.LastName ?? null;
            userProfile.WebSite = model.WebSite ?? null;
            userProfile.Picture = model.Picture;
            userProfile.Address = model.Address ?? null;
            userProfile.BirthDate = model.BirthDate ?? null;
            userProfile.LastActivityDate = updateDate;

            await _userProfileService.UpdateAsync(userProfile);

            resultSet.Data = userProfile;
            resultSet.Code = ResponseCodes.OK;

            return resultSet;

        }

        [HttpGet]
        public async Task<ResultSet<UserProfile>> GetUserProfileByEmail(string email)
        {
            var userProfile = await _userProfileService.GetByEmailAsync(email);

            if (userProfile is null)
            {
                var resultSet = new ResultSet<UserProfile>(ResponseCodes.NotFound);
                resultSet.AddError(new Error("UserNotExist", "User with this email does not exist!"));
                return resultSet;
            }                

            return new ResultSet<UserProfile>(userProfile);
        }
    }
}
