using Microsoft.EntityFrameworkCore.Migrations;

namespace ContosoUniversity.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using ContosoUniversity.Data;
    using ContosoUniversity.Models;

    public partial class InstructorSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                for (var i = 0; i < 100; i++)
                {
                    var entities = new List<Instructor>();

                    for (var j = 0; j < 100; j++)
                    {
                        entities.Add(new Instructor
                        {
                            FirstMidName = $"Instrutor_{j}", 
                            LastName = $"Seed_060819_{i}", 
                            HireDate = DateTime.Now
                        });
                    }

                    context.Instructors.AddRange(entities);
                    context.SaveChanges();
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            using (var context = new DesignTimeSchoolContext().CreateDbContext(new string[] { }))
            {
                const int AmountPerIteration = 100;
                var total = context.Instructors.Count();
                var iterations = Math.Ceiling((decimal)total / AmountPerIteration);

                for (var i = 0; i < iterations; i++)
                {
                    var entities = context
                        .Instructors
                        .OrderBy(x => x.ID)
                        .Where(x => x.LastName.Contains("Seed_060819"))
                        .Take(AmountPerIteration)
                        .ToList();

                    context.Instructors.RemoveRange(entities);
                    context.SaveChanges();
                }
            }
        }
    }
}
