using ColinApp.Entities.Config;
using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ���ݻ���������Ӧ�������ļ�
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


// ��ȡ JWT ����
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>();

builder.Services.Configure<JwtOptions>(jwtSection);

// JWT ��֤����
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

// ���� Kestrel �������Ķ˿�
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8801);
});

// ��� CORS ����
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // ����������Դ
              .AllowAnyMethod()    // �������ⷽ��
              .AllowAnyHeader();   // ������������ͷ
    });
});



var app = builder.Build();


// ʹ�� CORS ����
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

// ע�����
await consulClient.Agent.ServiceRegister(registration);

// ��ӽ������˵�
app.MapGet("/health", () => Results.Ok("Gateway is healthy"));


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapReverseProxy(); // �Զ�ת��

app.MapControllers();

app.Run();
