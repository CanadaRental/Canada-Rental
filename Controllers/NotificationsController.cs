using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;
using Microsoft.AspNetCore.Authorization;

namespace OntarioGo.Controllers
{
    public class NotificationsController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";

        private int GetUserId()
        {
            var claim = User.FindFirst("UserId")?.Value;
            if (claim == null)
                throw new Exception("User not authenticated");
            return int.Parse(claim);
        }

        // =========================
        // VER NOTIFICACIONES
        // =========================
        [Authorize] 
        public IActionResult Index()
        {
            List<NotificationEntity> notifications = new List<NotificationEntity>();

            int userId = GetUserId(); 

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT * FROM Notifications
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    notifications.Add(new NotificationEntity
                    {
                        NotificationId = (int)reader["NotificationId"],
                        UserId = (int)reader["UserId"],
                        Message = reader["Message"]?.ToString(),
                        IsRead = (bool)reader["IsRead"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }

            return View(notifications);
        }

        
        [Authorize]
        public IActionResult MarkAsRead(int id)
        {
            int userId = GetUserId();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Notifications SET IsRead = 1 WHERE NotificationId=@Id AND UserId=@UserId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UserId", userId); 

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

     
        // se usa desde otros controllers, no necesita vista
        public void CreateNotification(int userId, string message)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                INSERT INTO Notifications (UserId, Message, IsRead, CreatedAt)
                VALUES (@UserId, @Message, 0, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Message", message ?? "");

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}