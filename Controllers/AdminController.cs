using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using Microsoft.AspNetCore.Authorization;

namespace OntarioGo.Controllers
{
    [Authorize] 
    public class AdminController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";

       
        private string GetUserRole()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        }

        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;

            if (claim == null)
                throw new Exception("User not authenticated");

            return int.Parse(claim);
        }

     
     
        public IActionResult Dashboard()
        {
            if (GetUserRole() != "Admin")
                return RedirectToAction("Login", "Auth");

            List<PropertyEntity> properties = new List<PropertyEntity>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Properties ORDER BY CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    properties.Add(new PropertyEntity
                    {
                        PropertyId = (int)reader["PropertyId"],
                        Title = reader["Title"]?.ToString(),
                        Price = (decimal)reader["Price"],
                        City = reader["City"]?.ToString(),
                        Province = reader["Province"]?.ToString(),
                        IsAvailable = (bool)reader["IsAvailable"],
                        OwnerId = (int)reader["OwnerId"]
                    });
                }
            }

            return View(properties);
        }

        
        public IActionResult DeleteProperty(int id)
        {
            if (GetUserRole() != "Admin")
                return RedirectToAction("Login", "Auth");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Properties WHERE PropertyId=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LogAdminAction("DeleteProperty", id);

            return RedirectToAction("Dashboard");
        }

      
        public IActionResult ToggleAvailability(int id)
        {
            if (GetUserRole() != "Admin")
                return RedirectToAction("Login", "Auth");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Properties 
                SET IsAvailable = CASE WHEN IsAvailable = 1 THEN 0 ELSE 1 END
                WHERE PropertyId=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LogAdminAction("ToggleAvailability", id);

            return RedirectToAction("Dashboard");
        }

       
        public IActionResult Users()
        {
            if (GetUserRole() != "Admin")
                return RedirectToAction("Login", "Auth");

            List<UserEntity> users = new List<UserEntity>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Users";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new UserEntity
                    {
                        UserId = (int)reader["UserId"],
                        FullName = reader["FullName"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Role = reader["Role"]?.ToString(),
                        IsActive = (bool)reader["IsActive"]
                    });
                }
            }

            return View(users);
        }

        
        public IActionResult ToggleUser(int id)
        {
            if (GetUserRole() != "Admin")
                return RedirectToAction("Login", "Auth");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                UPDATE Users 
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE UserId=@Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LogAdminAction("ToggleUser", id);

            return RedirectToAction("Users");
        }

        
        private void LogAdminAction(string actionType, int? propertyId)
        {
            int adminId = GetUserId(); 

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO AdminActions 
                (AdminId, PropertyId, ActionType, ActionDate)
                VALUES (@AdminId, @PropertyId, @ActionType, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@AdminId", adminId);
                cmd.Parameters.AddWithValue("@PropertyId", (object?)propertyId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActionType", actionType);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}