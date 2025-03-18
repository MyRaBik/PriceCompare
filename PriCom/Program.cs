using Application.Interfaces;
using Application.Services;
using Infrastructure;
using Infrastructure.Parsers;
using Infrastructure.Persistence;
using MarketplaceParsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ����������� � �� (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

Console.WriteLine("[INFO] �� ����������.");

// ��������� ��������� ������������
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ��������� CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// ������������ WebDriverPool (��� ������ ��������)
builder.Services.AddSingleton<WebDriverPool>(sp => new WebDriverPool(3));

// **������������ �������**
builder.Services.AddScoped<YandexMarketSeleniumParser>();
builder.Services.AddScoped<OzonSeleniumParser>();
builder.Services.AddScoped<WildberriesSeleniumParser>();

// MultiMarketplaceParser ������ �������� ������ ��������
builder.Services.AddScoped<MultiMarketplaceParser>(sp =>
{
    var parsers = new List<IProductParser>
    {
        sp.GetRequiredService<YandexMarketSeleniumParser>(),
        sp.GetRequiredService<OzonSeleniumParser>(),
        sp.GetRequiredService<WildberriesSeleniumParser>()
    };
    return new MultiMarketplaceParser(parsers);
});

// **���������� MultiMarketplaceParser ��� IProductParser**
builder.Services.AddScoped<IProductParser>(sp => sp.GetRequiredService<MultiMarketplaceParser>());

// **������������ �����������**
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// **������������ �������**
builder.Services.AddScoped<IProductService, ProductService>();

// ������ ����������
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("[INFO] API �������.");
app.Run();
