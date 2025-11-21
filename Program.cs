var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
// Error-handling middleware (first)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync($"{{ \"error\": \"Internal server error.\" }}");
        // Optionally log the exception
    }
});

// Authentication middleware (next)
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }
    var validToken = "Bearer mysecrettoken"; // Replace with real validation
    if (token != validToken)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }
    await next();
});

// Logging middleware (last)
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var path = context.Request.Path;
    await next();
    var statusCode = context.Response.StatusCode;
    Console.WriteLine($"{method} {path} responded {statusCode}");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
