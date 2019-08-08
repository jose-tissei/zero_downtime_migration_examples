# ZERO_DOWNTIME_MIGRATION_EXAMPLES

## **Exemplo:** Campo Obrigatório

Neste exemplo utilizamos uma aplicação que representa o cadastro de professores e alunos de uma universidade, onde temos uma entidade base `Person` e entidades filhas `Instructor` e `Student`, ambas são armazenadas na tabela `Person` e são diferenciadas através do campo `Discriminator`.

Nosso objetivo é adicionar um novo campo `E-mail` na entidade `Person`, que deve ser obrigatório para `Instructor` e `Student`, para garantir `Downtime Zero` e `Rollback Imediato`, toda nova versão deve funcionar em conjunto com a versão antiga, logo para que isso seja possivel devemos dividir nossas alterações em 4 versões(ou deploys).

**Referências:** Migrating to Microservice Databases - From Relational Monolith to Distributed Data, Edson Yanaga. (Livro está contido no repositório em /referencias)

Obs.: O livro é gratuito!

Segue abaixo quais os passos executados em cada uma dessas versões:

### ContosoUniversity - V1

Primeiro vamos criar uma carga com 10.000 professores inserindo os dados em batchs de 100 registros por vez para evitar locks na base de dados, para isso foi criada a migration `20190328124431_Instructor-Seed.cs`.

```csharp
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
```

### ContosoUniversity - V2

Para garantir a retrocompatibilidade com a versão anterior e garantir que possamos fazer um rollback imediato primeiro devemos adicionar o campo email como opcional(nullable) na entidade `Person` e também adicionar os inputs para esse campo nos formulários assim como também na listagem de professores, para isso foi criada a migration `20190708181647_Person-Email.cs`.

```csharp
    public partial class PersonEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Person",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Person");
        }
    }
```

Para gerar a migration acima foi adicionado a classe `Person.cs` o campo Email.
```csharp
        [StringLength(100)]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
```

### ContosoUniversity - V3

Antes de tornar o campo obrigatório precisamos garantir que todos os registros do banco tenham um valor para o campo `Email`, para isso foi criada a migration `20190708213344_Person-Email-Fill-Empty.cs` que é responsavél por setar um valor padrão para esse campo em todas as entidades, para garantir que não serão gerados locks de grande duração na base de dados a rotina responsável por setar este valor padrão é executada em batches contendo 100 registros por vez, em conjunto também devemos garantir que todas as entidades manipuladas pelo usuário também insiram um valor padrão caso o campo `Email` esteja nulo, logo a classe `Person.cs` foi alterada para garantir esse comportamento.


`20190708213344_Person-Email-Fill-Empty.cs`
```csharp
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
```


`Person.cs`
```csharp
        [StringLength(100)]
        [Display(Name = "E-mail")]
        public string Email 
        { 
            get => email;
            set => email = string.IsNullOrEmpty(value) ? "Valor Padrão" : value;
        }

        private string email;

```

### ContosoUniversity - V4

Agora é só setar a entidade `Person` com o `Email` obrigatório, para isso foi gerada uma nova migration e revertemos a regra que setava o valor padrão no `Email` e adicionamos a anotação de `Required`.

`20190808140246_Email-Required.cs`
```csharp
    public partial class EmailRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Person",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Person",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);
        }
    }
```

`Person.cs`
```csharp
        [Required]
        [StringLength(100)]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
```
