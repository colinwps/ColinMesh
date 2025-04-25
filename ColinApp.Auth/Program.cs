using ColinApp.Entities.Config;
using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 根据环境加载相应的配置文件
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidateLifetime = true
        };
    });


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 Kestrel 服务器的端口
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8802);
});

builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("ConsulConfig"));
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
{
    var config = builder.Configuration.GetSection("ConsulConfig").Get<ConsulConfig>();
    return new ConsulClient(cfg => { cfg.Address = new Uri(config.Address); });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var consulConfig = app.Configuration.GetSection("ConsulConfig").Get<ConsulConfig>();

var registration = new AgentServiceRegistration()
{
    ID = consulConfig.ServiceId,
    Name = consulConfig.ServiceName,
    Address = consulConfig.ServiceAddress,
    Port = consulConfig.ServicePort.Value,
    Tags = new[] { "auth" },
    Check = new AgentServiceCheck()
    {
        HTTP = $"http://{consulConfig.ServiceAddress}:{consulConfig.ServicePort}/health",
        Interval = TimeSpan.FromSeconds(10),
        Timeout = TimeSpan.FromSeconds(5),
        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
    }
};

// 注册服务
await consulClient.Agent.ServiceRegister(registration);

// 添加健康检查端点
app.MapGet("/health", () => Results.Ok("guth is healthy"));


app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
