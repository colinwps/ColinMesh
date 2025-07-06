using ColinApp.Video.Entities;
using ColinApp.Video.SIPService;

var builder = WebApplication.CreateBuilder(args);

// 根据环境加载相应的配置文件
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<GB28181Config>(builder.Configuration.GetSection("GB28181"));

//builder.Services.AddHostedService<SipServerService>();
//builder.Services.AddSingleton<SipServerService>();

builder.Services.AddSingleton<SipServerService>();       // 可被注入
builder.Services.AddHostedService(provider => provider.GetRequiredService<SipServerService>()); // 用作后台服务



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
