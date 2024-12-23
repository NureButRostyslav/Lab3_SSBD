namespace StudentManagementApi.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }

        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; } 

        public ICollection<Student> Students { get; set; } = new List<Student>();  
    }


}
