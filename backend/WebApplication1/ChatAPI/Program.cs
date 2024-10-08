using Amazon.SimpleNotificationService;
using ChatAPI;
using ChatAPI.Models;
using MongoDB.Driver;
using StreamChat.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var streamOptions = new StreamOptions();
builder.Configuration.GetSection("Stream").Bind(streamOptions);

var factory =
    new StreamClientFactory(streamOptions.ApiKey, streamOptions.ApiSecret);
builder.Services.AddSingleton<IStreamClientFactory>(factory);
builder.Services.AddSingleton(factory.GetUserClient());
builder.Services.AddSingleton(factory.GetChannelClient());

var mongoOptions = new MongoDbOptions();
builder.Configuration.GetSection("MongoDb").Bind(mongoOptions);

var client = new MongoClient(mongoOptions.ConnectionString);
var database = client.GetDatabase(mongoOptions.DatabaseName);
var userCollection = database.GetCollection<User>(mongoOptions.UserCollectionName);

builder.Services.AddSingleton<IMongoClient>(client);
builder.Services.AddSingleton<IMongoDatabase>(database);
builder.Services.AddSingleton<IMongoCollection<User>>(userCollection);
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
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

app.UseCors();

app.MapControllers();

app.Run();