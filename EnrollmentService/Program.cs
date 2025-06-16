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

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Enrollment Service API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu token JWT aqui}"
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
});

// Banco de dados
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios e serviços
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentBusinessService, EnrollmentBusinessService>();
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<ICourseService, CourseExternalService>();
builder.Services.AddHttpContextAccessor();
// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
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

// CORREÇÃO: Configuração de autorização mais explícita
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Student");
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
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();