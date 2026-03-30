using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace OntarioGo.Controllers
{
    [Authorize] 
    public class UsersController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";


        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;

            if (claim == null)
                throw new Exception("UserId claim not found");

            return int.Parse(claim);
        }

 
        public IActionResult Profile()
        {
            int userId = GetUserId();

            UserEntity user = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Users WHERE UserId=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = new UserEntity
                    {
                        UserId = (int)reader["UserId"],
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Role = reader["Role"].ToString(),
                        IsActive = (bool)reader["IsActive"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    };
                }
            }

            return View(user);
        }

        public IActionResult Edit()
        {
            int userId = GetUserId();

            UserEntity user = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Users WHERE UserId=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = new UserEntity
                    {
                        UserId = (int)reader["UserId"],
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString()
                    };
                }
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(UserEntity model)
        {
            int userId = GetUserId();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE Users 
                                 SET FullName=@Name, Email=@Email 
                                 WHERE UserId=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", userId);
                cmd.Parameters.AddWithValue("@Name", model.FullName ?? "");
                cmd.Parameters.AddWithValue("@Email", model.Email ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Profile");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            int userId = GetUserId();

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error = "Passwords cannot be empty";
                return View();
            }

            string oldHash = HashPassword(oldPassword);
            string newHash = HashPassword(newPassword);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE UserId=@Id AND PasswordHash=@OldPassword";

                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Id", userId);
                checkCmd.Parameters.AddWithValue("@OldPassword", oldHash);

                conn.Open();
                int valid = (int)checkCmd.ExecuteScalar();
                conn.Close();

                if (valid == 0)
                {
                    ViewBag.Error = "Current password is incorrect";
                    return View();
                }

                string updateQuery = "UPDATE Users SET PasswordHash=@NewPassword WHERE UserId=@Id";

                SqlCommand cmd = new SqlCommand(updateQuery, conn);
                cmd.Parameters.AddWithValue("@Id", userId);
                cmd.Parameters.AddWithValue("@NewPassword", newHash);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Profile");
        }

        private string HashPassword(string password)
        {
            if (password == null) return ""; 

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
    }
}