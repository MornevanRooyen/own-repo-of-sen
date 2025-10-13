using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Services
{
    public class AuthService
    {
        // Registers a new user
        public void RegisterUser()
        {
            // TODO: Implement user registration logic
            // Example: validate input, hash password, save user to DB
            Console.WriteLine("User registered successfully.");
        }

        // Sends an email verification token
        public void IssueEmailVerification()
        {
            // TODO: Generate token & send email
            Console.WriteLine("Email verification issued.");
        }

        // Verifies the user’s email
        public void EmailVerify()
        {
            // TODO: Validate token and mark email as verified
            Console.WriteLine("Email verified successfully.");
        }

        // Sends a phone verification code
        public void IssuePhoneVerification()
        {
            // TODO: Send SMS code (e.g., via Twilio)
            Console.WriteLine("Phone verification issued.");
        }

        // Verifies the user’s phone number
        public void PhoneVerify()
        {
            // TODO: Check code and mark phone as verified
            Console.WriteLine("Phone verified successfully.");
        }

        // Signs the user in
        public void SignIn()
        {
            // TODO: Authenticate user (validate email/password, create session)
            Console.WriteLine("User signed in.");
        }

        // Signs the user out
        public void SignOut()
        {
            // TODO: End user session
            Console.WriteLine("User signed out.");
        }

        // Issues a password reset token
        public void IssuePasswordReset()
        {
            // TODO: Generate token & send email
            Console.WriteLine("Password reset issued.");
        }

        // Resets the user’s password
        public void ResetPassword()
        {
            // TODO: Validate token & update password hash
            Console.WriteLine("Password reset successfully.");
        }
    }
}
