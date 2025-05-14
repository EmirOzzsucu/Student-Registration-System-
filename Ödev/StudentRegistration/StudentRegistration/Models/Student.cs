using System.ComponentModel.DataAnnotations;

namespace StudentRegistration.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Surname is required.")]
        [StringLength(50, ErrorMessage = "Surname cannot be longer than 50 characters.")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "School number is required.")]
        [StringLength(50, ErrorMessage = "School number cannot be longer than 50 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "School number must contain only numbers.")]
        public string? SchoolNumber { get; set; }

        [Required(ErrorMessage = "Class is required.")]
        [StringLength(10, ErrorMessage = "Class cannot be longer than 10 characters.")]
        public string? Class { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        [AgeValidation(ErrorMessage = "Student must be at least 15 years old.")]
        public DateTime DateOfBirth { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public List<Course>? EnrolledCourses { get; set; }

        public string? ImagePath { get; set; }

        public class AgeValidation : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                if (value is DateTime birthDate)
                {
                    var age = DateTime.Now.Year - birthDate.Year;
                    if (DateTime.Now.Month < birthDate.Month || (DateTime.Now.Month == birthDate.Month && DateTime.Now.Day < birthDate.Day))
                    {
                        age--;
                    }

                    return age >= 15;
                }
                return false;
            }
        }

    }
}
