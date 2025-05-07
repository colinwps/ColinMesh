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


// 读取 JWT 配置
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>();

builder.Services.Configure<JwtOptions>(jwtSection);

// JWT 验证配置
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

builder.Services.AddAuthorization();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("ConsulConfig"));
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p =>
{
    var config = builder.Configuration.GetSection("ConsulConfig").Get<ConsulConfig>();
    return new ConsulClient(cfg => { cfg.Address = new Uri(config.Address); });
});

// 配置 Kestrel 服务器的端口
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8801);
});

// 添加 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // 允许任意来源
              .AllowAnyMethod()    // 允许任意方法
              .AllowAnyHeader();   // 允许任意请求头
    });
});



var app = builder.Build();


// 使用 CORS 策略
app.UseCors("AllowAll");

var consulClient = app.Services.GetRequiredService<IConsulClient>();
var consulConfig = app.Configuration.GetSection("ConsulConfig").Get<ConsulConfig>();

var registration = new AgentServiceRegistration()
{
    ID = consulConfig.ServiceId,
    Name = consulConfig.ServiceName,
    Address = consulConfig.ServiceAddress,
    Port = consulConfig.ServicePort.Value,
    Tags = new[] { "gateway" },
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
app.MapGet("/health", () => Results.Ok("Gateway is healthy"));


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapReverseProxy(); // 自动转发

app.MapControllers();

app.Run();
