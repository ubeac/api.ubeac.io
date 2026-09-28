using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using uBeac.Services.Email;
using uBeac.Storage.File;

namespace uBeac.Api.Services
{
    public interface IEmailService
    {
        Task SendUserAdded(string emailAddress, string teamName);
    }

    public class EmailService : IEmailService
    {

        private readonly IEmailSender _emailSender;
        private readonly IFileStorage _fileStorage;
        private readonly FileStorageFactory _fileStorageFactory;
        private readonly ILogger<EmailService> _logger;

        private string userAddedSubject = "uBeac Notification - You have now access to a new Team";
        private string userAddedBody = "Please be aware that You have now access to a new Team";

        public EmailService(IEmailSender emailSender, FileStorageFactory fileStorageFactory, ILogger<EmailService> logger)
        {
            emailSender.ThrowIfNull();
            fileStorageFactory.ThrowIfNull();
            _logger = logger;

            _emailSender = emailSender;
            _fileStorageFactory = fileStorageFactory;
            _fileStorage = _fileStorageFactory.GetStorage("IdentityServer");
            Init();
        }

        private void Init()
        {
            try
            {
                userAddedBody = _fileStorage.ReadAsync(string.Empty, "Email_User_Access_Add_Body.txt").Result;
                userAddedSubject = _fileStorage.ReadAsync(string.Empty, "Email_User_Access_Add_Subject.txt").Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "an error accured on email initialization}");
            }
        }

        public async Task SendUserAdded(string emailAddress, string teamName)
        {
            try
            {
                await _emailSender.SendEmailAsync(emailAddress, userAddedSubject, userAddedBody.Replace("[[Team]]", teamName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "an error accured on sending User Add Notification}");
            }
        }
    }
}
