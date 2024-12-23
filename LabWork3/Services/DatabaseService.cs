using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using StudentManagementApi.Exceptions; // Додано для користувацького виключення

namespace StudentManagementApi.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> DeleteStudentsWithCursorAsync(int minStudentCount)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("DeleteStudentsWithCursor", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@minStudentCount", minStudentCount);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        // Кидаємо користувацьке виключення з повідомленням і кодом помилки
                        throw new CustomDatabaseException("Error during deletion of students with cursor.", 500, ex);
                    }
                }
            }

            return "Студенти видалені успішно.";
        }

        public async Task<List<object>> GetTopGroupsByFacultyAsync(string facultyName)
        {
            var results = new List<object>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("SELECT * FROM GetTopGroupsByFaculty(@facultyName)", connection))
                {
                    command.Parameters.AddWithValue("@facultyName", facultyName);

                    try
                    {
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(new
                                {
                                    GroupName = reader["group_name"].ToString(),
                                    StudentCount = (int)reader["group_student_count"]
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Кидаємо користувацьке виключення при помилці SQL
                        throw new CustomDatabaseException("Error fetching top groups by faculty.", 500, ex);
                    }
                }
            }

            if (results.Count == 0)
            {
                // Можна також кинути виключення, якщо не знайдено жодної групи
                throw new CustomDatabaseException($"На факультеті {facultyName} немає студентів.", 404);
            }

            return results;
        }
    }
}
