using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebApi.Entities;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User GetByEmail(string email);
        User Create(User user, string password);
        void Update(User user);
        Task<User> ForgotPasswordAsync(string email);
        void Delete(int id);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private ILogger _logger;
        private readonly IConfiguration _config;

        public UserService(DataContext context, ILogger<UserService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.email == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordMD5(password, user.password))
                return null;

            // authentication successful
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.Where(x => x.email == email).FirstOrDefault();
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_context.Users.Any(x => x.email == user.email))
                throw new AppException("Username \"" + user.email + "\" is already taken");

            if (user.email.EndsWith("@gmail.com") || user.email.EndsWith("@yahoo.com") || user.email.EndsWith("@hotmail.com") || user.email.EndsWith("@outlook.com") || user.email.EndsWith("@rediff.com") || user.email.EndsWith("@apple.com"))
            {
                // Email address contains one of the specified domains
                throw new AppException("You should use working email.");
            }

            //byte[] passwordHash, passwordSalt;
            //CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.password = MD5Hash(password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public async Task<User> ForgotPasswordAsync(string email)
        {
            var user = _context.Users.SingleOrDefault(x => x.email == email);
            // check if username exists
            if (user == null)
                throw new AppException("Email \"" + email + "\" doesn't exist.");

            //create random password 
            string password = CreateRandomPassword();
            user.password = MD5Hash(password);
            
            _context.Users.Update(user);
            _context.SaveChanges();

            await SendEmailAsync(user.email, password);

            return user;
        }

        public async Task SendEmailAsync(string toEmail, string password)
        {
            string sendGridApiKey = _config.GetValue<string>("AppSettings:TwilioApiKey");
            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                throw new Exception("The 'SendGridApiKey' is not configured");
            }

            var client = new SendGridClient(sendGridApiKey);
            string message = "Your new password <b>" + password + "</b>";
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("yudomaryanto12@gmail.com", "SPAIPI.com"),
                Subject = "New Password",
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(toEmail));

            //var from = new EmailAddress("administrator@spaipi.com", "administrator");

            //var subject = "New Password";

            //var to = new EmailAddress(toEmail, "");

            //var plainTextContent = message;

            //var htmlContent = message;

            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);


            var response = await client.SendEmailAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email queued successfully");
            }
            else
            {
                _logger.LogError("Failed to send email");
                // Adding more information related to the failed email could be helpful in debugging failure,
                // but be careful about logging PII, as it increases the chance of leaking PII
            }
        }

        private static string CreateRandomPassword()
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Minimum size 8. Max size is number of all allowed chars.  
            int size = random.Next(8, validChars.Length);

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }

        public void Update(User userParam)
        {
            //var user = _context.Users.Find(userParam.email);
            var user = _context.Users.Where(x => x.email == userParam.email).FirstOrDefault();

            if (user == null)
                throw new AppException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.email) && userParam.email != user.email)
            {
                // throw error if the new username is already taken
                if (_context.Users.Any(x => x.email == userParam.email))
                    throw new AppException("Email " + userParam.email + " is already taken");

                user.email = userParam.email;
            }

            // update password if provided
            if (!string.IsNullOrWhiteSpace(user.password))
            {
                user.password = MD5Hash(user.password);
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordMD5(string password, string passwordDb)
        {
            password = MD5Hash(password);
            if (password == passwordDb)
                return true;
            else return false;
        }
        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        public static string MD5Hash(string input)
        {
            Encoding encoding = Encoding.UTF8;
            using (HMACMD5 hmac = new HMACMD5(encoding.GetBytes("P@ssw0rd")))
            {
                var msg = encoding.GetBytes(input);
                var hash = hmac.ComputeHash(msg);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }
    }
}