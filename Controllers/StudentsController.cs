using EFPerformance.Dto;
using EFPerformance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EFPerformance.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
    private readonly ILogger<StudentController> _logger;
    private readonly EFContext _db;

    public StudentController(ILogger<StudentController> logger, EFContext db)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("Student")]
    public IActionResult AddStudent([FromBody] string name)
    {
        var student = new Student()
        {
            Name = name
        };

        _db.Students.Add(student);

        _db.SaveChanges();

        return Ok();
    }

    [HttpPost("Email/{studentId}")]
    public IActionResult AddEmail([FromBody] string emailAddress, [FromRoute] int studentId)
    {
        var email = new Email()
        {
            EmailAddress = emailAddress,
            StudentId = studentId
        };

        _db.Emails.Add(email);

        _db.SaveChanges();

        return Ok();
    }

    [HttpGet]
    public IActionResult Get()
    {
        var students = _db.Students
                           .Include(s => s.Emails)
                           .AsSplitQuery()
                           .ToList();

        return Ok(students);
    }

    [HttpGet("sp")]
    public IActionResult GetBySP()
    {
        var student = _db.StudentViewModel
                         .FromSqlRaw("spGetStudentName")
                         .OrderByDescending(s => s.Name)
                         .ToList();

        return Ok(student);
    }

    [HttpGet("{studentId}")]
    public IActionResult Get(int studentId)
    {
        var students = _db.Students
                           .Include(s => s.Emails)
                           .Where(s => s.Id == studentId)
                           .ToList();

        return Ok(students);
    }

    [HttpGet("Benchmark")]
    public IActionResult Benchmark()
    {
        var marks = new List<Benchmark>();

        marks.Add(GetStudentsWithEmails_OldStyledLinq());
        marks.Add(GetStudentsWithEmails_LinqNewFeatures());
        marks.Add(GetStudentsWithEmails_SQL());
        marks.Add(GetStudentsWithEmails_SP());

        return Ok(marks);
    }

    private Benchmark GetStudentsWithEmails_OldStyledLinq()
    {
        var mark = new Benchmark("EF LINQ Vanilla");

        mark.SetStartingTime();

        var students = _db.Students
                           .Include(s => s.Emails)
                           .ToList();

        mark.SetFinishingTime();

        return mark;
    }

    private Benchmark GetStudentsWithEmails_LinqNewFeatures()
    {
        var mark = new Benchmark("EF LINQ Splite Query");

        mark.SetStartingTime();

        var students = _db.Students
                           .Include(s => s.Emails)
                           .AsSplitQuery()
                           .ToList();

        mark.SetFinishingTime();

        return mark;
    }

    private Benchmark GetStudentsWithEmails_SQL()
    {
        var mark = new Benchmark("EF SQL");

        mark.SetStartingTime();

        var students = _db.Students
                         .FromSqlRaw("SELECT * FROM Students").ToList();
        
        var ids = String.Join(",", students.Select(s => s.Id).ToList());

        var idsParam = new SqlParameter("@ListOfStudentId", ids);

        var emails = _db.Emails.FromSqlRaw("SELECT * FROM Emails WHERE Id IN (SELECT convert(int, value) FROM STRING_SPLIT(@ListOfStudentID, ','))", idsParam).ToList();

        foreach (var student in students)
        {
            student.Emails = emails.Where(e => e.StudentId == student.Id).ToList();
        }

        mark.SetFinishingTime();

        return mark;
    }

    private Benchmark GetStudentsWithEmails_SP()
    {
        var mark = new Benchmark("EF SP");

        mark.SetStartingTime();

        var students = _db.Students
                         .FromSqlRaw("EXECUTE [dbo].[spStudent_GetAll]").ToList();
        
        var ids = String.Join(",", students.Select(s => s.Id).ToList());

        var idsParam = new SqlParameter("@ListOfStudentId", ids);
        
        var emails = _db.Emails.FromSqlRaw("EXECUTE dbo.spEmails_GetByStudents @ListOfStudentId", idsParam).ToList();

        foreach (var student in students)
        {
            student.Emails = emails.Where(e => e.StudentId == student.Id).ToList();
        }

        mark.SetFinishingTime();

        return mark;
    }
}
