namespace StudentManagementApi.Models
{
    public class Faculty
    {
        public int FacultyId { get; set; }
        public string? FacultyName { get; set; } 

        public ICollection<Group> Groups { get; set; } = new List<Group>();  
    }

}
