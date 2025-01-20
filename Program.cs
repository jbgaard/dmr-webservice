using DMRWebScrapper_service.Code;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MongoDB client, read connection string from environment variable
builder.Services.AddSingleton(new MongoClient(Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "mongodb://localhost:27017"));

// Add DMRProxy as singleton
builder.Services.AddSingleton<DMRProxy>();

// Add VehicleViewService as singleton
builder.Services.AddSingleton<VehicleViewService>();

// Add PoliceReportService as singleton
builder.Services.AddSingleton<PoliceReportService>();

// Add AppwriteService as singleton
builder.Services.AddSingleton<AppwriteService>();

// Add cors
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(
		builder =>
		{
			builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader();
		});
});

var app = builder.Build();

// Use cors
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
