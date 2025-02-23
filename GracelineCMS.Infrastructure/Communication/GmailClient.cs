using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using GracelineCMS.Domain.Communication;
using MimeKit;
using System.Text;

namespace GracelineCMS.Infrastructure.Communication
{
    public class GmailClient(string encodedCredential) : IEmailClient
    {
        public async Task Send()
        {
            var credential = AuthenticateServiceAccount();
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail API Example",
            });

            await SendEmailAsync(service);
        }

        // Authenticate with Service Account
        private ICredential AuthenticateServiceAccount()
        {
            GoogleCredential credential;
            var jsonCredential = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredential));

            credential = GoogleCredential.FromJson(jsonCredential)
                    .CreateScoped(GmailService.Scope.GmailSend)
                    .CreateWithUser("waustinlynn@gracelinesoftware.com");  // The user on whose behalf you're sending the email

            return credential.UnderlyingCredential;
        }

        // Send an email using the Gmail API
        private static async Task SendEmailAsync(GmailService service)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Graceline Software", "waustinlynn@gracelinesoftware.com"));
            message.To.Add(new MailboxAddress("Austin Lynn", "waustinlynn@gmail.com"));
            message.Subject = "Subject of Email";
            message.Body = new TextPart("plain")
            {
                Text = "This is a test email sent using Gmail API with a service account."
            };

            var stream = new MemoryStream();
            await message.WriteToAsync(stream);
            var encodedMessage = Convert.ToBase64String(stream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            var emailMessage = new Message { Raw = encodedMessage };

            await service.Users.Messages.Send(emailMessage, "me").ExecuteAsync();
        }

        public Task SendEmailAsync(EmailMessage emailMessage)
        {
            throw new NotImplementedException();
        }
    }
}

