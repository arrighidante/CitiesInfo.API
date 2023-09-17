using CitiesInfo.API;
using CitiesInfo.API.DbContexts;
using CitiesInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sentry.Extensibility;
using Serilog;
using Serilog.Events;
using System.Text;

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Sentry(o =>
//    {
//        o.Dsn = "https://bf3cfd799e334e6d9da42e9017bd9495@o4504963706322944.ingest.sentry.io/4505466241155072";
//        o.Debug = true;
//        o.TracesSampleRate = 1;
//        o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
//        o.MinimumEventLevel = LogEventLevel.Warning;
//    })
//    .CreateLogger();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Sentry(o =>
    {
        o.Dsn = "https://bf3cfd799e334e6d9da42e9017bd9495@o4504963706322944.ingest.sentry.io/4505466241155072";
        o.Debug = true;
        o.TracesSampleRate = 1;
        o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
        o.MinimumEventLevel = LogEventLevel.Warning;
    })
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseSentry();

// Not useful anymore since we're using Serilog
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

builder.Host.UseSerilog();


// Add services to the container.

builder.Services.AddControllers( options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

/*
  Comments for builder.Services
    - AddTransient -> Created each time they're requested. 
                      Best for lighweight, stateless services.
    - AddScoped -> Created once per request
    - AddSingleton -> Created the first time they're requested.
 */

// MAIL SERVICE
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif


builder.Services.AddSingleton<CitiesDataStore>();
builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultDevConnection"]));

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

// To map between entities and different DTOs
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    }
);


// POLICIES
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    });
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSentryTracing();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


app.Run();
