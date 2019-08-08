using System.Linq;
using ContosoUniversity.Data;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ContosoUniversity.Migrations
{
    using System;

    /// <summary>
    /// The student email fill empty.
    /// </summary>
    public partial class PersonEmailFillEmpty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                const int AmountPerIteration = 100;
                var total = context.People.Count();
                var iterations = Math.Ceiling((decimal)total / AmountPerIteration);

                for (var i = 0; i < iterations; i++)
                {
                    var people = context
                        .People
                        .OrderBy(x => x.ID)
                        .Skip(i * AmountPerIteration)
                        .Take(AmountPerIteration)
                        .Where(x => x.Email == null || x.Email == string.Empty)
                        .ToList();

                    people.ForEach(x => x.Email = "empty");
                    context.People.UpdateRange(people);
                    context.SaveChanges();
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                const int AmountPerIteration = 100;
                var total = context.People.Count();
                var iterations = Math.Ceiling((decimal)total / AmountPerIteration);

                for (var i = 0; i < iterations; i++)
                {
                    var people = context
                        .People
                        .OrderBy(x => x.ID)
                        .Skip(i * AmountPerIteration)
                        .Take(AmountPerIteration)
                        .Where(x => x.Email == "empty")
                        .ToList();

                    people.ForEach(x => x.Email = null);
                    context.People.UpdateRange(people);
                    context.SaveChanges();
                }
            }
        }
    }
}
