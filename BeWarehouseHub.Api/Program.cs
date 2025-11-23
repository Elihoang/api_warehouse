using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Core.Helpers;
using BeWarehouseHub.Core.Repositories;
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================== DATABASE ====================
var connString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));

// ==================== JWT CONFIG ====================
JwtHelper.Initialize(builder.Configuration); // Khởi tạo helper

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]!;

// ==================== AUTHENTICATION ====================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        
        ClockSkew = TimeSpan.Zero // không cho phép lệch giờ
    };

    // (Tùy chọn) Hiển thị lỗi chi tiết khi debug
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==================== DI REPOSITORY & SERVICE ====================
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<SupplierService>();

builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<StockService>();

builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<WarehouseService>();

builder.Services.AddScoped<IExportReceiptRepository, ExportReceiptRepository>();
builder.Services.AddScoped<ExportReceiptService>();

builder.Services.AddScoped<IImportReceiptRepository, ImportReceiptRepository>();
builder.Services.AddScoped<ImportReceiptService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

// ==================== CORS ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    // Hoặc nếu muốn bảo mật hơn:
    // policy.WithOrigins("https://your-frontend.com")
    //       .AllowAnyMethod()
    //       .AllowAnyHeader()
    //       .AllowCredentials();
});

// ==================== SWAGGER + CONTROLLERS ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BeWarehouseHub API", Version = "v1" });

    // JWT Bearer trong Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
            Array.Empty<string>()
        }
    });

    c.EnableAnnotations();
});

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// ==================== MIDDLEWARE PIPELINE ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeWarehouseHub API V1");
        c.RoutePrefix = string.Empty; // Swagger tại root: https://localhost:5001
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // hoặc tên policy bạn đặt

app.UseAuthentication(); // QUAN TRỌNG: Phải đứng TRƯỚC UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();