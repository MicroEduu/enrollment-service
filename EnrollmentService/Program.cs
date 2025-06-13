using Microsoft.EntityFrameworkCore;
using EnrollmentService.Data;
using EnrollmentService.Repositories;
using EnrollmentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - usando SQLite
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// Business Service
builder.Services.AddScoped<IEnrollmentBusinessService, EnrollmentBusinessService>();

// External Services com HttpClient
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<ICourseService, CourseExternalService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();