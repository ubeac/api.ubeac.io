using System;
using System.Threading.Tasks;
using uBeac.Services.Email;
using uBeac.Storage.File;

namespace uBeac.Idsrv.Services
{
    public interface IEmailService
    {
        Task SendAccountConfirm(string emailAddress, string confirmationUrl);
        Task SendForgotPassword(string emailAddress, string callbackUrl);
        Task SendChangePassword(string emailAddress);
    }

    public class EmailService : IEmailService
    {
        public const string ACCOUNTCONFIRM_BODY_FILENAME = "Email_ConfirmAccount_Body.txt";
        public const string ACCOUNTCONFIRM_SUBJECT_FILENAME = "Email_ConfirmAccount_Subject.txt";
        public const string RESETPASSWORD_BODY_FILENAME = "Email_ForgotPassword_Body.txt";
        public const string RESETPASSWORD_SUBJECT_FILENAME = "Email_ForgotPassword_Subject.txt";
        public const string CHANGEPASSWORD_BODY_FILENAME = "Email_ChangePassword_Body.txt";
        public const string CHANGEPASSWORD_SUBJECT_FILENAME = "Email_ChangePassword_Subject.txt";

        private readonly IEmailSender _emailSender;
        private readonly IFileStorage _fileStorage;
        private readonly FileStorageFactory _fileStorageFactory;

        private string accountConfirmSubject = "uBeac Email Confirmation";
        //private string accountConfirmBody = "Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>";
        private string accountConfirmBody = "Please confirm your account by clicking this link: <a href='[[url]]'>link</a>";
        private string forgotPasswordSubject = "uBeac Reset Password";
        private string forgotPasswordBody = "Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>";
        private string changePasswordSubject = "uBeac Password changed";
        private string changePasswordBody = "This is just a confirmation email that your uBeac password has been successfully changed.";

        public EmailService(IEmailSender emailSender, FileStorageFactory fileStorageFactory)
        {
            emailSender.ThrowIfNull();
            fileStorageFactory.ThrowIfNull();

            _emailSender = emailSender;
            _fileStorageFactory = fileStorageFactory;
            _fileStorage = _fileStorageFactory.GetStorage("IdentityServer");
            Init();
        }

        private void Init()
        {
            try
            {
                accountConfirmSubject = _fileStorage.ReadAsync(string.Empty, ACCOUNTCONFIRM_SUBJECT_FILENAME).Result;
                accountConfirmBody = _fileStorage.ReadAsync(string.Empty, ACCOUNTCONFIRM_BODY_FILENAME).Result;
                forgotPasswordSubject = _fileStorage.ReadAsync(string.Empty, RESETPASSWORD_SUBJECT_FILENAME).Result;
                forgotPasswordBody = _fileStorage.ReadAsync(string.Empty, RESETPASSWORD_BODY_FILENAME).Result;
                changePasswordSubject = _fileStorage.ReadAsync(string.Empty, CHANGEPASSWORD_SUBJECT_FILENAME).Result;
                changePasswordBody = _fileStorage.ReadAsync(string.Empty, CHANGEPASSWORD_BODY_FILENAME).Result;
            }
            catch (Exception)
            {

            }
        }

        public async Task SendAccountConfirm(string emailAddress, string confirmationUrl)
        {
            try
            {
                await _emailSender.SendEmailAsync(emailAddress, accountConfirmSubject, accountConfirmBody.Replace("[[url]]", confirmationUrl));
            }
            catch (Exception)
            {
            }
        }

        public async Task SendForgotPassword(string emailAddress, string callbackUrl)
        {
            try
            {
                await _emailSender.SendEmailAsync(emailAddress, forgotPasswordSubject, forgotPasswordBody.Replace("[[url]]", callbackUrl));
            }
            catch (Exception)
            {
            }
        }

        public async Task SendChangePassword(string emailAddress)
        {
            try
            {
                await _emailSender.SendEmailAsync(emailAddress, changePasswordSubject, changePasswordBody);
            }
            catch (Exception)
            {
            }
        }
    }
}
