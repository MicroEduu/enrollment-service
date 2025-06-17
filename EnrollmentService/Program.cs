using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using EnrollmentService.Data;
using EnrollmentService.Repositories;
using EnrollmentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HttpContextAccessor for accessing HTTP context in services
builder.Services.AddHttpContextAccessor();

// Swagger com suporte a JWT e documentação completa
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Enrollment Service API", 
        Version = "v1",
        Description = "API for managing student enrollments in courses",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@company.com"
        }
    });

    // Configuração de segurança JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    // Incluir comentários XML para documentação
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurações adicionais para melhor apresentação
    c.EnableAnnotations();
    c.DescribeAllParametersInCamelCase();
    
    // Tags para organizar endpoints
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
});

// Banco de dados
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios e serviços
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentBusinessService, EnrollmentBusinessService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<ICourseService, CourseExternalService>();

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured");
}
var secretKeyBytes = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            // IMPORTANTE: Mapear o claim 'role' para o ClaimTypes.Role padrão
            RoleClaimType = "role",
            NameClaimType = "nameid"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    // Garantir que o claim 'role' seja também adicionado como ClaimTypes.Role
                    var roleClaim = identity.FindFirst("role");
                    if (roleClaim != null && !identity.HasClaim(ClaimTypes.Role, roleClaim.Value))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                        Console.WriteLine($"✅ Role '{roleClaim.Value}' adicionada aos claims padrão");
                    }

                    // Log dos claims para debug
                    Console.WriteLine("=== TOKEN VALIDADO ===");
                    foreach (var claim in identity.Claims)
                    {
                        Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                    }
                }
                return Task.CompletedTask;
            },
            
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Falha na autenticação: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            
            OnChallenge = context =>
            {
                Console.WriteLine($"🔐 Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            },

            OnMessageReceived = context =>
            {
                var token = context.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"🎫 Token recebido: {token.Substring(0, Math.Min(50, token.Length))}...");
                }
                return Task.CompletedTask;
            }
        };
    });

// Configuração de autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Student");
    });
    options.AddPolicy("TeacherOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Teacher");
    });
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });
    options.AddPolicy("AdminOrTeacher", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "Teacher");
    });
});

// CORS liberado
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

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Enrollment Service API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Enrollment Service API";
        c.DisplayRequestDuration();
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Log startup information
Console.WriteLine("🚀 Enrollment Service API iniciada!");
Console.WriteLine($"📝 Swagger UI: {(app.Environment.IsDevelopment() ? "http://localhost:5263/swagger" : "Disponível apenas em desenvolvimento")}");
Console.WriteLine("📋 Endpoints disponíveis:");
Console.WriteLine("  POST /api/Enrollment - Matricular aluno");
Console.WriteLine("  GET /api/Enrollment/courses/{id}/students - Listar alunos do curso");
Console.WriteLine("  GET /api/Enrollment/students/{id}/courses - Listar cursos do aluno");

app.Run();