// // Services/UserServiceImpl.cs
// using System;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.Extensions.Configuration;
// using Microsoft.IdentityModel.Tokens;
// using dotnetapp.Models;
// using dotnetapp.Repositories;
// using System.Security.Cryptography;

// namespace dotnetapp.Services
// {
//     public class UserServiceImpl : UserService
//     {
//         private readonly UserRepository _userRepository;
//         private readonly IConfiguration _configuration;

//         public UserServiceImpl(UserRepository userRepository, IConfiguration configuration)
//         {
//             _userRepository = userRepository;
//             _configuration = configuration;
//         }

//         public User register(User user)
//         {
//             return _userRepository.register(user);
//         }

//         public User login(User user)
//         {
//             return _userRepository.login(user);
//         }

//         public string GenerateJwtToken(User user)
//         {
//             var tokenHandler = new JwtSecurityTokenHandler();

//             using (var hmac = new HMACSHA256())
//             {
//                 // Generate a random 256-bit key
//                 var key = new byte[32]; // 256 bits
//                 using (var rng = new RNGCryptoServiceProvider())
//                 {
//                     rng.GetBytes(key);
//                 }

//                 hmac.Key = key;

//                 var tokenDescriptor = new SecurityTokenDescriptor
//                 {
//                     Subject = new ClaimsIdentity(new[]
//                     {
//                         new Claim(ClaimTypes.Name, user.Email),
//                         // Add other claims as needed
//                     }),
//                     Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
//                     SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//                 };

//                 var token = tokenHandler.CreateToken(tokenDescriptor);
//                 return tokenHandler.WriteToken(token);
//             }
//         }
//     }
// }


using dotnetapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using dotnetapp.Data;
namespace dotnetapp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context; // Add this field


        public UserService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;

        }

        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                //var customUser = new User
                //{
                //    Username = user.Username,
                //    Password = user.Password,
                //    Role = user.Role,
                //    // Set any additional properties you have in your custom User entity
                //};

                //// Add the customUser to the DbSet and save it
                //_context.Users.Add(customUser);
                //await _context.SaveChangesAsync();


                var identityUser = new IdentityUser
                {
                    UserName = user.Username,
                };

                Console.WriteLine(identityUser.UserName);


                var result = await _userManager.CreateAsync(identityUser, user.Password);
                Console.WriteLine("asd"+result);

                if (result.Succeeded)
                {
                    // Assign roles to the user
                    if (user.Role == "admin")
                    {
                        Console.WriteLine("data " + identityUser);
                        await _userManager.AddToRoleAsync(identityUser, "admin");
                    }
                    else if (user.Role == "applicant")
                    {
                        Console.WriteLine("data1 " + identityUser);

                        await _userManager.AddToRoleAsync(identityUser, "applicant");
                        Console.WriteLine("data2 " + identityUser);

                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                // Handle exceptions appropriately (e.g., logging)
                return false; // Registration failed
            }
        }

        //    public async Task<string> LoginAsync(string username, string password)
        //    {
        //        try
        //        {
        //            var user = await _userManager.FindByNameAsync(username);

        //            if (user == null || !(await _signInManager.CheckPasswordSignInAsync(user, password, false)).Succeeded)
        //                return null; // Invalid username or password

        //            // Generate a JWT token
        //            var token = GenerateJwtToken(user);
        //            Console.WriteLine("hai" + token);

        //            return token;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("zxcvbnm" + ex.Message);
        //            // Handle exceptions appropriately (e.g., logging)
        //            return null; // Login failed
        //        }
        //    }


        //    private string GenerateJwtToken(IdentityUser user)
        //    {
        //        Console.WriteLine(user.UserName);
        //        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, user.UserName),
        //};

        //        // Retrieve roles for the user
        //        var roles = _userManager.GetRolesAsync(user).Result;

        //        // Add role claims to the JWT token
        //        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        //        var token = new JwtSecurityToken(
        //            _configuration["Jwt:Issuer"],
        //            _configuration["Jwt:Audience"],
        //            claims,
        //            expires: DateTime.Now.AddHours(2), // Token expiry time
        //            signingCredentials: credentials
        //        );

        //        return new JwtSecurityTokenHandler().WriteToken(token);
        //    }

        public async Task<string> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);

                if (user == null || !(await _signInManager.CheckPasswordSignInAsync(user, password, false)).Succeeded)
                    return null; // Invalid username or password

                // Generate a JWT token
                var token = GenerateJwtToken(user);
                Console.WriteLine("hai" + token);

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine("zxcvbnm" + ex.Message);
                // Handle exceptions appropriately (e.g., logging)
                return null; // Login failed
            }
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            Console.WriteLine(user.UserName);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
    };

            // Retrieve roles for the user
            var roles = _userManager.GetRolesAsync(user).Result;

            Console.WriteLine("summa"+roles);

            // Add role claims to the JWT token
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2), // Token expiry time
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        //    private async Task<string> GenerateJwtToken(IdentityUser user)
        //    {
        //        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.Name, user.UserName),
        //};

        //        // Add the user's role to claims
        //        var roles = await _userManager.GetRolesAsync(user);
        //        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        //        var token = new JwtSecurityToken(
        //            _configuration["Jwt:Issuer"],
        //            _configuration["Jwt:Audience"],
        //            claims,
        //            expires: DateTime.Now.AddHours(2), // Token expiry time
        //            signingCredentials: credentials
        //        );

        //        return new JwtSecurityTokenHandler().WriteToken(token);
        //    }



        //private string GenerateJwtToken(IdentityUser user)
        //{
        //    Console.WriteLine(user.UserName);
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.Name, user.UserName),
        //        // You can add role claims here if needed
        //    };


        //    var token = new JwtSecurityToken(
        //        _configuration["Jwt:Issuer"],
        //        _configuration["Jwt:Audience"],
        //        claims,
        //        expires: DateTime.Now.AddHours(2), // Token expiry time
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }
}
