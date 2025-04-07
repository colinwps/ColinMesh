using ColinApp.Entities.Config;
using Consul;

var builder = WebApplication.CreateBuilder(args);

// ���ݻ���������Ӧ�������ļ�
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

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



var app = builder.Build();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
