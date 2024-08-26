using BaldurBillsApp.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BaldurBillsApp.Services
{
    public class UserService
    {
        private readonly BaldurBillsDbContext _context;

        public UserService(BaldurBillsDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser> Authenticate(string username, string password)
        {
            var user = await _context.AppUsers.SingleOrDefaultAsync(x => x.UserName == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task CreateUser(string username, string password, string email, string firstName, string lastName)
        {
            var hashedPassword = HashPassword(password);
            var newUser = new AppUser
            {
                UserName = username,
                PasswordHash = hashedPassword,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                LastLogin = DateTime.Now
            };

            _context.AppUsers.Add(newUser);
            await _context.SaveChangesAsync();
        }

        private string HashPassword(string password)
        {
            // Generate a hash for the password using BCrypt
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}