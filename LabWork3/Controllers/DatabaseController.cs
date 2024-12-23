using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementApi.Data;
using StudentManagementApi.Models;
using StudentManagementApi.Exceptions;  // Додано для обробки CustomDatabaseException
using System.Linq;

namespace StudentManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatabaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                // Спроба зчитування даних з бази для перевірки з'єднання
                var testData = _context.Faculties.FirstOrDefault();
                if (testData != null)
                {
                    return Ok("Connection successful!");
                }
                return BadRequest("No data found in database.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }

        [HttpGet("faculties")]
        public IActionResult GetAllFaculties()
        {
            try
            {
                var faculties = _context.Faculties.ToList();
                if (faculties.Any())
                {
                    return Ok(faculties);
                }
                return NotFound("No faculties found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve faculties: {ex.Message}");
            }
        }

        [HttpPost("add-faculty")]
        public IActionResult AddFaculty([FromBody] Faculty newFaculty)
        {
            try
            {
                if (newFaculty == null || string.IsNullOrEmpty(newFaculty.FacultyName))
                {
                    return BadRequest("Faculty name is required.");
                }

                _context.Faculties.Add(newFaculty);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetAllFaculties), new { id = newFaculty.FacultyId }, newFaculty);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to add faculty: {ex.Message}");
            }
        }

        [HttpDelete("delete-students/{k}")]
        public IActionResult DeleteStudents(int k)
        {
            try
            {
                // Використовуємо прокручуваний курсор для видалення студентів
                var studentsToDelete = _context.Students
                    .Where(s => s.Role == "Student" && s.Group.Students.Count < k)  // Перевірка на кількість студентів у групі
                    .ToList();

                if (studentsToDelete.Any())
                {
                    _context.Students.RemoveRange(studentsToDelete);
                    _context.SaveChanges();
                    return Ok($"Deleted {studentsToDelete.Count} students.");
                }

                return NotFound($"No students found with less than {k} in group.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete students: {ex.Message}");
            }
        }

        [HttpGet("faculties-with-groups-and-students")]
        public IActionResult GetFacultiesWithGroupsAndStudents()
        {
            try
            {
                // Завантажуємо факультети разом з групами та студентами
                var faculties = _context.Faculties
                    .Include(f => f.Groups)  // Завантажуємо групи
                        .ThenInclude(g => g.Students)  // Завантажуємо студентів
                    .ToList();

                if (faculties.Any())
                {
                    return Ok(faculties);
                }

                return NotFound("No faculties found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve faculties with groups and students: {ex.Message}");
            }
        }


        [HttpGet("students-with-groups")]
        public IActionResult GetStudentsWithGroups()
        {
            try
            {
                // Використовуємо Include для завантаження зв'язків між таблицями
                var students = _context.Students
                    .Include(s => s.Group)   // Завантажуємо групу
                    .ThenInclude(g => g.Faculty)  // Завантажуємо факультет
                    .ToList();

                if (students.Any())
                {
                    return Ok(students);
                }
                return NotFound("No students found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve students: {ex.Message}");
            }
        }

        [HttpGet("largest-groups/{facultyName}")]
        public IActionResult GetLargestGroupsByFaculty(string facultyName)
        {
            try
            {
                // Знаходимо факультет за назвою
                var faculty = _context.Faculties
                    .Include(f => f.Groups)  // Завантажуємо групи
                    .ThenInclude(g => g.Students)  // Завантажуємо студентів
                    .FirstOrDefault(f => f.FacultyName == facultyName);

                if (faculty == null)
                {
                    return NotFound($"Faculty {facultyName} not found.");
                }

                // Знаходимо групи з найбільшою кількістю студентів
                var largestGroups = faculty.Groups
                    .Where(g => g.Students.Any())  // Групи з хоча б одним студентом
                    .OrderByDescending(g => g.Students.Count)
                    .ToList();

                if (largestGroups.Any())
                {
                    var maxStudentCount = largestGroups.First().Students.Count;
                    var result = largestGroups
                        .Where(g => g.Students.Count == maxStudentCount)
                        .ToList();

                    return Ok(result);
                }

                return Ok($"На факультеті {facultyName} немає студентів.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve largest groups: {ex.Message}");
            }
        }

    }
}
