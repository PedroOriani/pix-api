using Pix.Services;
using Pix.Repositories;
using Pix.Middlewares;
using Pix.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;
using Pix.Config;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts => {
    string host = builder.Configuration["Database:Host"] ?? string.Empty;
    string port = builder.Configuration["Database:Port"] ?? string.Empty;
    string username = builder.Configuration["Database:Username"] ?? string.Empty;
    string database = builder.Configuration["Database:Name"] ?? string.Empty;
    string password = builder.Configuration["Database:Password"] ?? string.Empty;

    string connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";
    opts.UseNpgsql(connectionString);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Queue

IConfigurationSection queueConfigurationSection = builder.Configuration.GetSection("QueueSettings");
builder.Services.Configure<QueueConfig>(queueConfigurationSection);

// Authentication



// Services
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<KeyService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ConcilliationService>();

//Repositories
builder.Services.AddScoped<HealthRepository>();
builder.Services.AddScoped<KeyRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<BankRepository>();
builder.Services.AddScoped<PaymentRepository>();

builder.Services.AddTransient<AuthenticationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Metrics
app.UseMetricServer();
app.UseHttpMetrics(options => options.AddCustomLabel("host", context => context.Request.Host.Host));

app.UseHttpsRedirection();

app.UseMiddleware<AuthenticationHandler>();
app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.Run();
