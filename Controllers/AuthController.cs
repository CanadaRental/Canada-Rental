using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OntarioGo.Controllers
{
    public class AuthController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserEntity entity)
        {
            entity.PasswordHash = HashPassword(entity.Password);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Users WHERE Email=@Email AND PasswordHash=@Password AND IsActive=1";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", entity.Email);
                cmd.Parameters.AddWithValue("@Password", entity.PasswordHash);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                   
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, reader["Email"].ToString()),
                        new Claim(ClaimTypes.Role, reader["Role"].ToString()),
                        new Claim("UserId", reader["UserId"].ToString())
                    };

                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("CookieAuth", principal);

                    return RedirectToAction("Index", "Properties");
                }
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserEntity entity)
        {
            entity.PasswordHash = HashPassword(entity.Password);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";

                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", entity.Email);

                conn.Open();
                int exists = (int)checkCmd.ExecuteScalar();
                conn.Close();

                if (exists > 0)
                {
                    ViewBag.Error = "Email already exists";
                    return View();
                }

                string insertQuery = @"INSERT INTO Users 
                (FullName, Email, PasswordHash, Role, IsActive, CreatedAt) 
                VALUES (@FullName, @Email, @Password, @Role, 1, GETDATE())";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@FullName", entity.FullName);
                cmd.Parameters.AddWithValue("@Email", entity.Email);
                cmd.Parameters.AddWithValue("@Password", entity.PasswordHash);
                cmd.Parameters.AddWithValue("@Role", entity.Role);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth"); // 👈 IMPORTANTE
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
