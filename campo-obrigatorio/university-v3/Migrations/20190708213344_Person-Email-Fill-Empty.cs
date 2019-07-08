using System;
using System.Linq;
using ContosoUniversity.Data;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContosoUniversity.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// The person email fill empty.
    /// </summary>
    public partial class PersonEmailFillEmpty : Migration
    {
        private const int AMOUNT_PER_ITERATION = 100;
        private const string PLACEHOLDER = "empty";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                var total = context.People.Count();
                var iterations = Math.Ceiling((decimal)total / AMOUNT_PER_ITERATION);

                for (var i = 0; i < iterations; i++)
                {
                    var students = context
                        .People
                        .OrderBy(x => x.ID)
                        .Skip(i * AMOUNT_PER_ITERATION)
                        .Take(AMOUNT_PER_ITERATION)
                        .Where(x => x.Email == null || x.Email == string.Empty)
                        .ToList();

                    students.ForEach(x => x.Email = PLACEHOLDER);
                    context.People.UpdateRange(students);
                    context.SaveChanges();
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                var total = context.People.Count();
                var iterations = Math.Ceiling((decimal)total / AMOUNT_PER_ITERATION);

                for (var i = 0; i < iterations; i++)
                {
                    var students = context
                        .People
                        .OrderBy(x => x.ID)
                        .Skip(i * AMOUNT_PER_ITERATION)
                        .Take(AMOUNT_PER_ITERATION)
                        .Where(x => x.Email == PLACEHOLDER)
                        .ToList();

                    students.ForEach(x => x.Email = null);
                    context.People.UpdateRange(students);
                    context.SaveChanges();
                }
            }
        }
    }
}
