# Add a new Module to the ABP App

In this post we will see how to develop a modular abp application. We will add a new module to default abp app and then use the same database to store the modules data and the identity data.

## Creating the abp application and run migrations

```bash
abp new Amazing.Software -t -csf
```

## Run Migrations

Change directory to Amazing.Software/src/Amazing.SOftware.DbMigrator and run the migration project

```bash
dotnet run
```

This will apply the migrations to the db and we can run the `MainApp.Web` project. This will host the UI and API..

## Add a new Module

Now we will add a new module to our Amazing.Software app. Navigate to the solution folder of the `Amazing.Software` and run the following command

```bash
abp add-module Amazing.Software.ModuleA --new --add-to-solution-file
```

This command will create a new module and add the new module to the solution.

Now you can run the `Amazing.Software.Web` and see the Api and UI available in the app.

## Add new Entity to the ModuleA

We will create a new Entity inside the `Amazing.Software.ModuleA.Domain` called `TodoOne`.

## 1. Create an [Entity](https://docs.abp.io/en/abp/latest/Entities)

First step is to create an Entity. Create the Entity in the `Amazing.Software.ModuleA.Domain` project.

```cs
public class TodoOne : Entity<Guid>
{
    public string Content { get; set; }
    public bool IsDone { get; set; }
}
```

## 2. Add Entity to [ef core](https://docs.abp.io/en/abp/latest/Entity-Framework-Core)

Next is to add Entity to the EF Core. you will find the DbContext in the `Amazing.Software.ModuleA.EntityFrameworkCore` project. Add the DbSet to the DbContext

```cs
public DbSet<TodoOne> TodoOnes { get; set; }
```

## 3. Configure Entity in [ef core](https://docs.abp.io/en/abp/latest/Entity-Framework-Core#configurebyconvention-method)

Configuration is done in the `DbContextModelCreatingExtensions` class inside the `ConfigureModuleA` method. This should be available in the `Amazing.Software.ModuleA.EntityFrameworkCore` project. `ConfigureModuleA` is invoked in the `AmazingSoftwareDbContext`.

```cs
builder.Entity<TodoOne>(b =>
{
    b.ToTable(options.TablePrefix + "TodoOnes", options.Schema);
    b.ConfigureByConvention(); //auto configure for the base class props
});
```

## 4. Adding Migrations

Now the Entity is configured we can add the migrations.

Go the `Amazing.Software.EntityFrameworkCore` project in the terminal and create migrations.

To create migration run this command:

```bash
dotnet ef migrations add created_todoone
```

Verify the migrations created in the migrations folder.

To update the database run this command

```bash
dotnet ef database update
```

## 5. Create a Entity Dto

Dto are placed in `Amazing.Software.ModuleA.Application.Contracts` project

```cs
public class TodoOneDto : EntityDto<Guid>
{
    public string Content { get; set; }
    public bool IsDone { get; set; }
}
```

## 6. Map Entity to Dto

Abp uses AutoMapper to map Entity to Dto. you can find the `ApplicationAutoMapperProfile` file which is used by the AutoMapper in the `Amazing.Software.ModuleA.Application` project.

```cs
CreateMap<TodoOne, TodoOneDto>();
CreateMap<TodoOneDto, TodoOne>();
```

## 7. Create an [Application Services](https://docs.abp.io/en/abp/latest/Application-Services)

Application service are created in the `Amazing.Software.ModuleA.Application` project

```cs
public class TodoOneAppService : ModuleAAppService
{
    private readonly IRepository<TodoOne, Guid> todoOneRepository;

    public TodoOneAppService(IRepository<TodoOne, Guid> todoOneRepository)
    {
        this.todoOneRepository = todoOneRepository;
    }

    public async Task<List<TodoOneDto>> GetAll()
    {
        return ObjectMapper.Map<List<TodoOne>, List<TodoOneDto>>(await todoOneRepository.GetListAsync());
    }

    public async Task<TodoOneDto> CreateAsync(TodoOneDto todoOneDto)
    {
        var TodoOne = ObjectMapper.Map<TodoOneDto, TodoOne>(todoOneDto);
        var createdTodoOne = await todoOneRepository.InsertAsync(TodoOne);
        return ObjectMapper.Map<TodoOne, TodoOneDto>(createdTodoOne);
    }

    public async Task<TodoOneDto> UpdateAsync(TodoOneDto todoOneDto)
    {
        var TodoOne = ObjectMapper.Map<TodoOneDto, TodoOne>(todoOneDto);
        var createdTodoOne = await todoOneRepository.UpdateAsync(TodoOne);
        return ObjectMapper.Map<TodoOne, TodoOneDto>(createdTodoOne);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var TodoOne = await todoOneRepository.FirstOrDefaultAsync(x=> x.Id == id);
        if(TodoOne != null)
        {
            await todoOneRepository.DeleteAsync(TodoOne);
            return true;
        }
        return false;
    }
}
```

## 8. Update `AddAbpDbContext` method in the `ModuleAEntityFrameworkCoreModule`

```cs
options.AddDefaultRepositories(includeAllEntities: true);
```

## 9. Update the `ConfigureServices` in the `ModuleAWebModule` in the `ModuleA.Web`

```cs
Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(ModuleAApplicationModule).Assembly);
            });
```

## 10. Test you api

Run the `Amazing.Software.Web` project and navigate to `https://localhost:<port>/swagger/` you will see the todo apis. You can test your API there.
