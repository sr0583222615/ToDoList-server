using Microsoft.EntityFrameworkCore;
using TodoApi;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // הוספת שירותים למיכל השירותים
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://anotherdomain.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // מאפשר שימוש בקבצי Cookie ואימות
            });
        });

        builder.Services.AddDbContext<ToDoDbContext>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("ToDoDB"),
                new MySqlServerVersion(new Version(8, 0, 2))
            ));

        builder.Services.AddScoped<Service>();
        builder.Services.AddControllers();

        // הוספת Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        app.UseMiddleware<ErrorLoggingMiddleware>();

        // שימוש ב-Swagger במצב פיתוח
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
                options.RoutePrefix = string.Empty;
            });
        }

        // שימוש במדיניות CORS
        app.UseCors("AllowSpecificOrigins");

        // קביעת צינור בקשות HTTP
        app.UseAuthorization(); // אם יש צורך באימות
        app.MapControllers();

        // מסלולי API לדוגמה
        app.MapGet("/items", async (Service service) =>
        {
            var items = await service.GetItems();
            return Results.Ok(items);  // מחזיר את התוצאות
        }).Produces<IEnumerable<Item>>(StatusCodes.Status200OK);  // Swagger יזהה את סוג הנתונים

        app.MapPost("/items", async (Service service, Item item) =>
        {
            var result = await service.AddItems(item);

            if (result == "Item added successfully")
            {
                return Results.Created($"/items/{item.Id}", item); // 201 - Created
            }

            return Results.BadRequest(result); // 400 - BadRequest עם הודעת שגיאה
        });

        app.MapPut("/items", async (Service service, Item item) =>
        {
            return await service.UpdateItems(item);
        });

        app.MapDelete("/items/{id}", async (Service service, int id) =>
        {
            return await service.delete(id);
        });

        app.Run();
    }
}
