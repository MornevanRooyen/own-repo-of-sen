using System;
using System.Net;
using System.Net.Mail;

namespace Sen381.Business.Services
{
    public class EmailService
    {
        // ---------- SMTP Configuration ----------
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly bool _enableSsl = true;

        // ✅ CHANGE THESE VALUES
        private readonly string _fromAddress = "xavierbarnard10@gmail.com";  // your Gmail address
        private readonly string _password = "ivdq zduu tsik tnoy";           // Gmail App Password
        private readonly string _baseUrl = "https://localhost:7228";         // your app's base URL (change if deployed)

        // ---------- Core Email Sender ----------
        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    Credentials = new NetworkCredential(_fromAddress, _password)
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(_fromAddress, "CampusLearn Support"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mail.To.Add(toEmail);
                client.Send(mail);

                Console.WriteLine($"📨 Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send email to {toEmail}: {ex.Message}");
            }
        }

        // ---------- Email Verification ----------
        public void SendVerificationEmail(string toEmail, string token)
        {
            // ✅ Directly call backend verification endpoint
            string verifyUrl = $"{_baseUrl}/api/verify/verify-email?token={token}";

            string subject = "Verify Your Email - CampusLearn";
            string body =
                $"Hi there!\n\n" +
                $"Welcome to CampusLearn!\n\n" +
                $"Please verify your email address by clicking the link below:\n" +
                $"{verifyUrl}\n\n" +
                $"After verification, you'll be redirected to the login page.\n\n" +
                $"If you didn’t create this account, please ignore this email.\n\n" +
                $"Thank you,\nCampusLearn Support Team";

            SendEmail(toEmail, subject, body);
        }


        // ---------- Password Reset Email ----------
        public void SendPasswordResetEmail(string toEmail, string token)
        {
            string resetUrl = $"{_baseUrl}/reset-password?token={token}";
            string subject = "Reset Your Password - CampusLearn";
            string body =
                $"We received a request to reset your password.\n\n" +
                $"Click the link below to set a new one:\n" +
                $"{resetUrl}\n\n" +
                $"If you didn’t request this, you can safely ignore this email.\n\n" +
                $"Thank you,\nCampusLearn Support Team";

            SendEmail(toEmail, subject, body);
        }

        // ---------- Generic Notification ----------
        public void SendNotification(string toEmail, string subject, string message)
        {
            SendEmail(toEmail, subject, message);
        }
    }
}
