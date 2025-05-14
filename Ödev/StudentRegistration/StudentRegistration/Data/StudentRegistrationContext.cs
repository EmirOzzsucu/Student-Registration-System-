using Microsoft.EntityFrameworkCore;
using StudentRegistration.Models;

namespace StudentRegistration.Data
{
    public class StudentRegistrationContext : DbContext
    {
        public StudentRegistrationContext(DbContextOptions<StudentRegistrationContext> options)
            :   base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
    }
}
