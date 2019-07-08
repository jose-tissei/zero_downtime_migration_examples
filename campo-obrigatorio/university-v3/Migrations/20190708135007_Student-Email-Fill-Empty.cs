using System.Linq;
using ContosoUniversity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContosoUniversity.Migrations
{
    using System;

    /// <summary>
    /// The student email fill empty.
    /// </summary>
    public partial class StudentEmailFillEmpty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                const int AmountPerIteration = 100;
                var total = context.Students.Count();
                var iterations = Math.Ceiling((decimal)total / AmountPerIteration);

                for (var i = 0; i < iterations; i++)
                {
                    var students = context
                        .Students
                        .Skip(i * AmountPerIteration)
                        .Take(AmountPerIteration)
                        .Where(x => string.IsNullOrWhiteSpace(x.Email))
                        .ToList();

                    students.ForEach(x => x.Email = "empty");
                    context.Students.UpdateRange(students);
                    context.SaveChanges();
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
