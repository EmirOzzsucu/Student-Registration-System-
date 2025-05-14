using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Data;
using StudentRegistration.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace StudentRegistration.Controllers
{
    public class StudentController : Controller
    {
        private readonly StudentRegistrationContext _context;

        public StudentController(StudentRegistrationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 10;
            var studentsQuery = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => s.Name.Contains(searchString) ||
                                                         s.Surname.Contains(searchString) ||
                                                         s.SchoolNumber.Contains(searchString));
            }

            var students = await studentsQuery
                                .OrderBy(s => s.Id)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentPage"] = page;

            var totalStudents = await studentsQuery.CountAsync();
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalStudents / pageSize);

            return View(students);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Surname,SchoolNumber,Class,DateOfBirth")] Student student, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.SchoolNumber == student.SchoolNumber);

                if (existingStudent != null)
                {
                    ModelState.AddModelError("SchoolNumber", "A student with this school number already exists.");
                    return View(student);
                }

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    student.ImagePath = "/images/" + uniqueFileName;
                }

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Surname,SchoolNumber,Class,DateOfBirth,ImagePath")] Student student, IFormFile? ImageFile)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Students
                        .FirstOrDefaultAsync(s => s.SchoolNumber == student.SchoolNumber && s.Id != student.Id);

                    if (existingStudent != null)
                    {
                        ModelState.AddModelError("SchoolNumber", "A student with this school number already exists.");
                        return View(student);
                    }

                    var studentInDb = await _context.Students.FindAsync(id);
                    if (studentInDb == null)
                    {
                        return NotFound();
                    }

                    studentInDb.Name = student.Name;
                    studentInDb.Surname = student.Surname;
                    studentInDb.SchoolNumber = student.SchoolNumber;
                    studentInDb.Class = student.Class;
                    studentInDb.DateOfBirth = student.DateOfBirth;

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        studentInDb.ImagePath = "/images/" + uniqueFileName;
                    }

                    _context.Update(studentInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                if (!string.IsNullOrEmpty(student.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
