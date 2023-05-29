using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Cors;
using TodoApi;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();



//הגדרת הקורס שתהיה אפשרות לגשת מהסרביס לAPI
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.WithOrigins("http://localhost:3000")
        .AllowAnyHeader() 
        .AllowAnyMethod();
}));

builder.Services.AddSingleton<ToDoDbContext>();//הזרקה של הסרביס

//נותן אפשרות להשתמש בסוואגר
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
    });
});

var app = builder.Build();

app.UseCors("MyPolicy");
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});



app.MapGet("/", () => "Hello World!");

app.MapGet("/items", (ToDoDbContext context) =>
{
    return context.Items.ToList();
});
app.MapPost("/items", async(ToDoDbContext context, Item item)=>{
    context.Add(item);
    await context.SaveChangesAsync();
    return item;
});

app.MapPut("/items/{id}", async(ToDoDbContext context, [FromBody]Item item, int id)=>{
    var existItem = await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();
    existItem.IsComplete = item.IsComplete;
    await context.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/items/{id}", async(ToDoDbContext context, int id)=>{
    var existItem = await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();
    context.Items.Remove(existItem);
    await context.SaveChangesAsync();
    return Results.NoContent();
});
app.Run();