using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OntarioGo.Entity;

namespace OntarioGo.Controllers
{
    public class SearchController : Controller
    {
        private string connectionString = "Server=ontariodb.mssql.somee.com;Database=ontariodb;User Id=ontariogo_SQLLogin_1;Password=rmf7zitki5;TrustServerCertificate=True;";

  
        public IActionResult Index()
        {
            return View(new List<PropertyEntity>());
        }

        [HttpPost]
        public IActionResult Search(string city, decimal? minPrice, decimal? maxPrice, string propertyType)
        {
            List<PropertyEntity> results = new List<PropertyEntity>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Properties WHERE 1=1";

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrEmpty(city))
                {
                    query += " AND City LIKE @City";
                    cmd.Parameters.AddWithValue("@City", "%" + city + "%");
                }

                if (minPrice.HasValue)
                {
                    query += " AND Price >= @MinPrice";
                    cmd.Parameters.AddWithValue("@MinPrice", minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query += " AND Price <= @MaxPrice";
                    cmd.Parameters.AddWithValue("@MaxPrice", maxPrice.Value);
                }

                if (!string.IsNullOrEmpty(propertyType))
                {
                    query += " AND PropertyType = @Type";
                    cmd.Parameters.AddWithValue("@Type", propertyType);
                }

                query += " ORDER BY CreatedAt DESC";

                cmd.CommandText = query;

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new PropertyEntity
                    {
                        PropertyId = (int)reader["PropertyId"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString(),
                        Price = (decimal)reader["Price"],
                        City = reader["City"].ToString(),
                        Province = reader["Province"].ToString(),
                        Address = reader["Address"].ToString(),
                        PropertyType = reader["PropertyType"].ToString(),
                        Bedrooms = reader["Bedrooms"] == DBNull.Value ? null : (int?)reader["Bedrooms"],
                        Bathrooms = reader["Bathrooms"] == DBNull.Value ? null : (int?)reader["Bathrooms"],
                        IsAvailable = (bool)reader["IsAvailable"],
                        OwnerId = (int)reader["OwnerId"],
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
            }

            return View("Index", results);
        }
    }
}
