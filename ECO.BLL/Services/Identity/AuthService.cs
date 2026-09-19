using AutoMapper;
using ECO.BLL.DTO;
using ECO.BLL.DTO.Auth;
using ECO.BLL.Services.Email;
using ECO.BLL.Services.Token;
using ECO.DAL.Entities;
using ECO.DAL.Sharing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using System.Net.Sockets;
using System.Security.Claims;

namespace ECO.BLL.Services.Identity
{

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;
        private readonly IGenerateToken generateToken;
        private readonly IConfiguration configuration;
        private readonly IRefreshTokenService refreshTokenService;
        private readonly EmailAppearanceService emailAppearanceService;
        private readonly int emailTokenLifespanMinutes;

        public AuthService(UserManager<ApplicationUser> userManager,IMapper mapper,
            IEmailService emailService,SignInManager<ApplicationUser> signInManager,
            IGenerateToken generateToken, IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            EmailAppearanceService emailAppearanceService)
        {
           _userManager = userManager;
            this.mapper = mapper;
            this.emailService = emailService;
            this.signInManager = signInManager;
            this.generateToken = generateToken;
            this.configuration = configuration;
            this.refreshTokenService = refreshTokenService;
            this.emailAppearanceService = emailAppearanceService;
            emailTokenLifespanMinutes = configuration.GetValue("Auth:EmailTokenLifespanMinutes", 60);
        }

      
        public async Task<string> LoginAsync(LoginDto login)
        {
            if (login == null)
                return null;
            var findUser = await CheckEamilExists(login.Email);
            if (findUser == null)
                return "Please Check Your Email Or Password";

            if (!findUser.EmailConfirmed)
            {
                return "Please confirm your email before logging in";
            }
            var result =await signInManager.CheckPasswordSignInAsync(findUser, login.Password,true);
            if (result.Succeeded)
            {
                var bootstrapEmails = configuration.GetSection("Admin:BootstrapEmails").Get<string[]>() ?? Array.Empty<string>();
                if (bootstrapEmails.Contains(findUser.Email, StringComparer.OrdinalIgnoreCase) &&
                    !await _userManager.IsInRoleAsync(findUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(findUser, "Admin");
                }
                return await generateToken.GenerateTokenAsync(findUser,_userManager);
            }

            return "Please Check Your Email Or Password";

        }
        public async Task<string> RegisterAsync(RegisterDto register)
        {
            if (register == null)
                return null;
            if (await _userManager.FindByNameAsync(register.UserName) != null ||
                await _userManager.FindByEmailAsync(register.Email) != null)
                return "Username or Email is already registered";
            var user = mapper.Map<ApplicationUser>(register);
            user.DisplayName = register.UserName;
            var autoConfirmEmail = configuration.GetValue<bool>("Auth:AutoConfirmEmail");
            user.EmailConfirmed = autoConfirmEmail;

            var result =await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
                return result.Errors.ToList()[0].Description;
            var bootstrapEmails = configuration.GetSection("Admin:BootstrapEmails").Get<string[]>() ?? Array.Empty<string>();
            if (bootstrapEmails.Contains(register.Email, StringComparer.OrdinalIgnoreCase))
                await _userManager.AddToRoleAsync(user, "Admin");
            if (autoConfirmEmail)
                return "";
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
             try
             {
                 await SendAccountEmailAsync(user, code, isReset: false);
             }
             catch (SmtpCommandException)
             {
                 return "Account created, but the confirmation email could not be sent. Please contact support.";
             }
             catch (SmtpProtocolException)
             {
                 return "Account created, but the confirmation email could not be sent. Please contact support.";
             }
             catch (TimeoutException)
             {
                 return "Account created, but the confirmation email could not be sent. Please contact support.";
             }
             catch (SocketException)
             {
                 return "Account created, but the confirmation email could not be sent. Please contact support.";
             }


            return "";
        }
        public  async Task SendEmail(string email,string code,string component,string subject,string Message)
        {
            var result =new EmailDto(
                email,
                configuration["EmailSetting:From"]
                    ?? throw new InvalidOperationException("EmailSetting:From is required to send account emails."),
                subject,
                EmailStringBody.Send(email, code, component, Message, GetFrontendUrl()));
          await  emailService.SendEmail(result);
        }

        private string GetFrontendUrl()
        {
            var configuredUrl = configuration["FrontendUrl"];
            if (string.IsNullOrWhiteSpace(configuredUrl))
                throw new InvalidOperationException("FrontendUrl is required to create account email links.");

            return configuredUrl.TrimEnd('/');
        }
        public async Task<bool> SendEmailAndForgetPassword(string email)
        {
            var findUser = await _userManager.FindByEmailAsync(email);
            if (findUser != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(findUser);
                try
                {
                    await SendAccountEmailAsync(findUser, token, isReset: true);
                }
                catch (SmtpCommandException) { }
                catch (SmtpProtocolException) { }
                catch (TimeoutException) { }
                catch (SocketException) { }
            }
           return false;
        }
        public async Task<string> ResetPassword(ResetPasswordDto resetPassword)
        {
            var findUser = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (findUser != null)
            {
                var result= await _userManager.ResetPasswordAsync(findUser, resetPassword.Token, resetPassword.Password);    
                if (result.Succeeded)
                {
                    await refreshTokenService.RevokeAllAsync(findUser.Id);
                    return "Password Change  Success";
                }
                return result.Errors.ToList()[0].Description;
            }
            return "Email or Password not found";
        }
        public async Task<bool> ActiveAccount(ActiveAccountDto activeAccountDto) 
        {
            var findUser = await _userManager.FindByEmailAsync(activeAccountDto.Email);
            if (findUser != null)
            {
                var result = await _userManager.ConfirmEmailAsync(findUser,activeAccountDto.Token);
                if (result.Succeeded)
                {
                    return true;
                }

               var token =await _userManager.GenerateEmailConfirmationTokenAsync(findUser);
                try
                {
                    await SendAccountEmailAsync(findUser, token, isReset: false);
                }
                catch (SmtpCommandException) { }
                catch (SmtpProtocolException) { }
                catch (TimeoutException) { }
                catch (SocketException) { }
            }
            return false;
        }

        /// <summary>
        /// Sends the activation / password-reset email using the branded template
        /// (theme + per-type/language copy from the DB). Same link shape as EmailStringBody.
        /// </summary>
        private async Task SendAccountEmailAsync(ApplicationUser user, string token, bool isReset)
        {
            var look = await emailAppearanceService.GetAsync();
            var copy = await emailAppearanceService.GetCopyAsync(isReset ? EmailTemplateCopy.Reset : EmailTemplateCopy.Activation);
            var frontendUrl = GetFrontendUrl();
            var component = isReset ? "reset-password" : "active";
            var link = $"{frontendUrl}/account/{Uri.EscapeDataString(component)}" +
                       $"?email={Uri.EscapeDataString(user.Email ?? string.Empty)}&code={Uri.EscapeDataString(token)}";

            var subject = EmailTemplateCopy.FillSubject(copy.Subject, look.BrandName, user.DisplayName, orderId: null);
            var body = isReset
                ? ResetPasswordEmailTemplate.Build(look, copy, link, emailTokenLifespanMinutes, user.DisplayName)
                : AccountActivationEmailTemplate.Build(look, copy, link, emailTokenLifespanMinutes, user.DisplayName);

            await emailService.SendEmail(new EmailDto(user.Email ?? string.Empty, user.Email ?? string.Empty, subject, body));
        }
    
        public async  Task<ApplicationUser> CheckEamilExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;
            else return user;
        }
    }

}
 