using FileUploadAzure;
using FileUploadAzure.Abstractions;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpoints();
builder.Services.AddServices();

builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.Section));

builder.Services.AddAzureClients(clientBuilder =>
{
    var blobOptions = builder
                        .Configuration
                        .GetSection(StorageOptions.Section)
                        .Get<StorageOptions>();
    clientBuilder
        .AddBlobServiceClient(blobOptions.ConnectionString)
        .ConfigureOptions(options =>
        {
            options.Retry.Mode = Azure.Core.RetryMode.Exponential;
            options.Retry.MaxRetries = 5;
            options.Retry.MaxDelay = TimeSpan.FromSeconds(120);
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

app.MapEndpoints();

app.Run();
 