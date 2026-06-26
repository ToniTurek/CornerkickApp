#define RESEND

using CornerkickApp.Shared.Models;
using CornerkickApp.Web.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
#if SENDGRID
using SendGrid;
using SendGrid.Helpers.Mail;
#endif
#if RESEND
using Resend;
#endif

namespace CornerkickApp.Web.Services
{
  public class CkEmailSender : IEmailSender
  {
    private readonly IConfiguration _configuration;

#if SENDGRID
    public CkEmailSender(IConfiguration configuration, IOptions<AuthMessageSenderOptions> optionsAccessor)
    {
      _configuration = configuration;
      Options = optionsAccessor.Value;
    }

    public AuthMessageSenderOptions Options { get; } //set only via Secret Manager
#endif
#if RESEND
    public CkEmailSender(IConfiguration configuration)
    {
      _configuration = configuration;
    }
#endif

#if SENDGRID
    private const string email_api_key = "CK_SENDGRID_API_KEY";
#endif
#if RESEND
    private const string email_api_key = "CK_RESEND_API_KEY";
#endif
    public async Task SendEmailAsync(string sEmailTo, string subject, string message)
    {
      const string sCkFromEmail = "mail@cornerkick-manager.de";
      const string sCkFromUser = "Cornerkick Manager";

      // Use connection string
      string? sApiKey = _configuration.GetSection(email_api_key)?.Value;

      if (string.IsNullOrEmpty(sApiKey)) {
        // Use environment variable
        sApiKey = Environment.GetEnvironmentVariable(email_api_key);
      }

      // Read from environment
      if (string.IsNullOrEmpty(sApiKey)) {
        CkAppShared.ckMng.tl.writeLog($"Error: Cannot read ApiKey '{email_api_key}' from connection string or environment", CornerkickManager.Main.sErrorFile);
        return;
      }

#if SENDGRID
      return Execute(sApiKey, subject, message, sEmailTo, new EmailAddress(sCkFromEmail, sCkFromUser));
#endif
#if RESEND
      IResend resend = ResendClient.Create(sApiKey);

      var resp = await resend.EmailSendAsync(new EmailMessage() {
        From = "onboarding@resend.dev",
        To = "s.jan@web.de",
        Subject = subject,
        HtmlBody = message,
      });
      /*
      var resp = await resend.EmailSendAsync(new EmailMessage() {
        From = sCkFromEmail,
        To = sEmailTo,
        Subject = subject,
        HtmlBody = message,
      });
       */
#endif
    }

    public async Task SendConfirmationLinkAsync(CornerkickAppUser user, string email, string confirmationLink)
    {
      string subject = "Bestätige deine E-Mail-Adresse";
      string message = $@"
        Hallo {user.Vorname} {user.Nachname},<br/><br/>
        bitte bestätige deine Registrierung, indem du auf den folgenden Link klickst:<br/>
        <a href=""{confirmationLink}"">{confirmationLink}</a><br/><br/>
        Viele Grüße,<br/>
        Dein Cornerkick-Team<br/>
      ";

      await SendEmailAsync(email, subject, message);
    }

#if SENDGRID
    public Task Execute(string apiKey, string subject, string message, string sEmailTo, EmailAddress emailFrom)
    {
      var options = new SendGridClientOptions {
          ApiKey = apiKey
      };
      options.SetDataResidency("eu"); 
      var client = new SendGridClient(options);
      // uncomment the above 6 lines if you are sending mail using a regional EU subuser
      // and remove the client declaration just below
      //var client = new SendGridClient(apiKey);

      var msg = new SendGridMessage() {
        From = emailFrom,
        Subject = subject,
        PlainTextContent = message,
        HtmlContent = message
      };
      msg.AddTo(new EmailAddress(sEmailTo));

      // Disable click tracking.
      // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
      msg.SetClickTracking(false, false);

      return client.SendEmailAsync(msg);
    }
#endif
  }
}
