using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("user_id", true)]
        [Column("user_id", ignoreOnInsert: true)]
        public int Id { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("phone_num")]
        public string PhoneNum { get; set; }

        [Column("email")]
        public string Email { get; set; }

        // ✅ Do not update this field when saving profile changes
        [Column("password_hash", ignoreOnUpdate: true)]
        public string PasswordHash { get; set; }

        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [Column("role", ignoreOnUpdate: true)]
        public string RoleString { get; set; }

        // ✅ Ignore system-managed timestamps
        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; }

        [Column("last_login", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? LastLogin { get; set; }

        [Column("profile_picture_path")]
        public string ProfilePicturePath { get; set; }

        // ✅ Case-sensitive column names in Supabase
        [Column("city_town")]
        public string City { get; set; }

        [Column("Country")]   // Capitalized in DB
        public string Country { get; set; }

        [Column("Timezone")]  // Capitalized in DB
        public string Timezone { get; set; }

        [Column("website")]
        public string Website { get; set; }

        [Column("programme")]
        public string Program { get; set; }

        [Column("year_of_study")]
        public string Year { get; set; }

        [Column("about")]
        public string About { get; set; }

        [Column("contact_preference")]
        public string ContactPreference { get; set; }

        [Column("interests")]
        public string Interests { get; set; }

        [Column("subjects")]
        public string Subjects { get; set; }

        // =============================
        // Password Utilities
        // =============================
        public void ChangePassword(string newPassword)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));
            PasswordHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        public bool VerifyPassword(string inputPassword)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputPassword));
            var hashedInput = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return PasswordHash == hashedInput;
        }

        public void SetRole(Role role) => RoleString = role.ToString();

        public Role GetRole() =>
            Enum.TryParse<Role>(RoleString, out var parsedRole)
                ? parsedRole
                : Role.student;
    }

    public enum Role
    {
        student,
        admin,
        tutor
    }
}
