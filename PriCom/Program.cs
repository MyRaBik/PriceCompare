using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Infrastructure.Jobs;
using Infrastructure.Parsers;
using Infrastructure.Persistence;
using Infrastructure.Security;
using MarketplaceParsers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ����������� � �� (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

Console.WriteLine("[INFO] �� ����������.");

// ��������� ��������� ������������
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ��������� ��������� Swagger + JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PriCom API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "������� JWT ����� (������: Bearer eyJ...)",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// JWT-��������������
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidAudience = jwtConfig["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// ������������ WebDriverPool (��� ������ ��������)
builder.Services.AddSingleton<WebDriverPool>(sp => new WebDriverPool(6));

builder.Services.AddSingleton(new GlobalParsingQueue(maxConcurrent: 6)); // 6 ���������


//��������� ����� ����� ��������� ������
builder.Services.AddHostedService<WebDriverPoolDisposer>();

// ������������ �������
builder.Services.AddScoped<YandexMarketSeleniumParser>();
builder.Services.AddScoped<OzonSeleniumParser>();
builder.Services.AddScoped<WildberriesSeleniumParser>();

builder.Services.AddScoped<MultiMarketplaceParser>(sp =>
{
    var parsers = new List<IProductParser>
    {
        sp.GetRequiredService<YandexMarketSeleniumParser>(),
        sp.GetRequiredService<OzonSeleniumParser>(),
        sp.GetRequiredService<WildberriesSeleniumParser>()
    };
    var queue = sp.GetRequiredService<GlobalParsingQueue>();
    return new MultiMarketplaceParser(parsers, queue);
});

builder.Services.AddScoped<IProductParser>(sp => sp.GetRequiredService<MultiMarketplaceParser>());

// �����������
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

// �������
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ISubscriptionRefresherService, SubscriptionRefresherService>(); //��� ���������� ��������
builder.Services.AddHostedService<DailySubscriptionRefreshJob>();


// ����������� � ������
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// ������ ����������
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PriCom API v1");
        c.RoutePrefix = "swagger"; // https://localhost:7000/swagger
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseDefaultFiles(); // ��������� ��� �������
app.UseStaticFiles();

app.UseAuthentication(); // ����������� �� UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");  // ��������� ��� �������

Console.WriteLine("[INFO] API �������.");
app.Run();
