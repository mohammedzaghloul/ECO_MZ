using ECO.Api.Helper;
using ECO.BLL.DTO.AddressDtos;
using ECO.BLL.DTO.Auth;

using ECO.BLL.DTO.Order;
using ECO.BLL.DTO.UserDtos;
using ECO.BLL.Services.Identity;

using ECO.BLL.Services.Token;
using ECO.BLL.Services.UserInfo;
using ECO.DAL.Entities;
using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Authentication.Google;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;



namespace ECO.Api.Controller

{

    public class AccountController : BaseController

    {

        private readonly IAuthService _auth;

        private readonly ICurrentUserService _userService;

        private readonly IAddressService _addressService;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenerateToken _generateToken;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;


        public AccountController(

            IAuthService Auth,

            ICurrentUserService userService,

            IAddressService addressService,
            UserManager<ApplicationUser> userManager,
            IGenerateToken generateToken,
            IRefreshTokenService refreshTokenService,
            IConfiguration configuration)
        {

            _auth = Auth;

            _userService = userService;

            _addressService = addressService;

            _userManager = userManager;
            _generateToken = generateToken;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
        }



        [HttpGet("google-challenge")]

        public IActionResult GoogleChallenge()

        {

            if (string.IsNullOrWhiteSpace(_configuration["Google:ClientId"]) ||

                string.IsNullOrWhiteSpace(_configuration["Google:ClientSecret"]))

            {

                return Redirect("/account/login?socialError=notconfigured");

            }



            var props = new AuthenticationProperties { RedirectUri = "/api/Account/google-callback" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);

        }



        [HttpGet("google-callback")]

        public async Task<IActionResult> GoogleCallback()

        {

            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded || result.Principal is null)

                return Redirect("/account/login?socialError=failed");



            var email = result.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))

                return Redirect("/account/login?socialError=noemail");



            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)

            {

                var displayName = result.Principal.FindFirstValue(ClaimTypes.GivenName)

                    ?? result.Principal.FindFirstValue(ClaimTypes.Name)

                    ?? email.Split('@')[0];



                user = new ApplicationUser

                {

                    UserName = email,

                    Email = email,

                    EmailConfirmed = true,

                    DisplayName = displayName

                };

                var created = await _userManager.CreateAsync(user);

                if (!created.Succeeded)

                    return Redirect("/account/login?socialError=failed");

            }



            await IssueTokensAsync(user);



            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');
            return Redirect(string.IsNullOrWhiteSpace(frontendUrl) ? "/" : $"{frontendUrl}/");

        }

        [HttpPost("register")]

        public async Task <IActionResult> Register(RegisterDto registerDto)

        {

            var result =await _auth.RegisterAsync(registerDto);

            if (result == null)

                return BadRequest(new ResponseApi(400,result));

            if (!string.IsNullOrWhiteSpace(result))

                return BadRequest(new ResponseApi(400, result));



            return Ok(new ResponseApi(200, "Registration successful. Please confirm your email."));

        }



        [HttpPost("Login")]

        public async Task<IActionResult> Login(LoginDto loginDto)

        {

            var result = await _auth.LoginAsync(loginDto);

            if (result == null)

                return BadRequest(new ResponseApi(400, result));



            if (result.StartsWith("Please ", StringComparison.OrdinalIgnoreCase))

                return Unauthorized(new ResponseApi(401, result));



            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
                return Unauthorized(new ResponseApi(401, "User not found"));

            await IssueTokensAsync(user, result);



            return Ok(new ResponseApi(200));

        }



        [HttpPost("ActiveAccount")]

        public async Task<IActionResult> Active(ActiveAccountDto activeAccount)

        {

            var result=await _auth.ActiveAccount(activeAccount);

            return result ? Ok(new ResponseApi(200)) : BadRequest(new ResponseApi(400));



        }



        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPassword)
        {
            if (resetPassword == null ||
                string.IsNullOrWhiteSpace(resetPassword.Email) ||
                string.IsNullOrWhiteSpace(resetPassword.Token) ||
                string.IsNullOrWhiteSpace(resetPassword.Password))
                return BadRequest(new ResponseApi(400, "Email, code and new password are required"));

            var result = await _auth.ResetPassword(resetPassword);
            if (result != null && result.Contains("Success"))
                return Ok(new ResponseApi(200, result));
            return BadRequest(new ResponseApi(400, result));
        }

        [HttpGet("Send-Email-Or-Get-Password")]

        public async Task<IActionResult> forget(string email)

        {

            var result=await _auth.SendEmailAndForgetPassword(email);

            return result ? Ok(new ResponseApi(200)) : BadRequest(new ResponseApi(400));



        }



        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await _refreshTokenService.RevokeAsync(refreshToken);

            Response.Cookies.Delete("token", CreateAuthCookieOptions());
            Response.Cookies.Delete("refresh_token", CreateRefreshCookieOptions());
            return Ok(new ResponseApi(200, "Logged out successfully"));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new ResponseApi(401, "Refresh token is missing"));

            var result = await _refreshTokenService.RotateAsync(refreshToken);
            if (result is null)
                return Unauthorized(new ResponseApi(401, "Refresh token is invalid or expired"));

            await IssueTokensAsync(result.User, result.AccessToken, result.RefreshToken);
            return Ok(new ResponseApi(200));
        }



        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDto>> GetUser()

        {

            var currentUser = await _userService.GetCurrentUser();

            return Ok(new GenericResponseApi<UserDto>(200, "GetCurrentUser", currentUser));

        }



        [Authorize]

        [HttpGet("GetAddress")]

        public async Task<ActionResult<AddressDto>> GetAddress()

        {

            var address = await _addressService.GetUserAddressAsync();

            return Ok(new GenericResponseApi<AddressDto>(200, "Address", address));

        }



        [Authorize]
        [HttpPost("UpdateAddress")]
        public async Task<ActionResult<AddressDto>> UpdateAddress([FromBody] ShippingAddressDto shippingAddressDto)
        {
            var updatedAddress = await _addressService.UpdateAsync(shippingAddressDto);
            return Ok(new GenericResponseApi<AddressDto>(200, "Address updated successfully", updatedAddress));

        }



        [Authorize(Roles = "Admin")]
        [HttpPost("UpdateAddressByEmail")]
        public async Task<ActionResult<AddressDto>> UpdateAddressByEmail(
            [FromQuery] string email,
            [FromBody] ShippingAddressDto shippingAddressDto)
        {
            try
            {
                var updatedAddress = await _addressService.UpdateByEmailAsync(email, shippingAddressDto);
                return Ok(new GenericResponseApi<AddressDto>(200, "Address updated successfully", updatedAddress));
            }
            catch (InvalidOperationException)
            {
                return NotFound(new ResponseApi(404, "User not found"));
            }
        }



        private CookieOptions CreateAuthCookieOptions()
        {
            var isHttps = HttpContext.Request.IsHttps;

            return new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Path = "/",
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            };
        }

        private CookieOptions CreateRefreshCookieOptions()
        {
            var isHttps = HttpContext.Request.IsHttps;
            var refreshTokenDays = Math.Max(1, _configuration.GetValue("Token:RefreshTokenDays", 30));
            return new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Path = "/api/Account",
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(refreshTokenDays)
            };
        }

        private async Task IssueTokensAsync(
            ApplicationUser user,
            string? accessToken = null,
            string? refreshToken = null)
        {
            accessToken ??= await _generateToken.GenerateTokenAsync(user, _userManager);
            if (refreshToken is null)
            {
                var tokens = await _refreshTokenService.CreateAsync(user);
                refreshToken = tokens.RefreshToken;
            }

            Response.Cookies.Append("token", accessToken, CreateAuthCookieOptions());
            Response.Cookies.Append("refresh_token", refreshToken, CreateRefreshCookieOptions());
        }



    }

}
