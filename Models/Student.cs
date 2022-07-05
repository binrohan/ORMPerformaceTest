using System.ComponentModel.DataAnnotations;

namespace EFPerformance.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Email> Emails { get; set; }
    }
}