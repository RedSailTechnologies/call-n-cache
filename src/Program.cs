using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOutputCache();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
});

builder.Services.UseHttpClientMetrics();
builder.Services.AddHealthChecks().ForwardToPrometheus();

var app = builder.Build();

app.UseResponseCaching();
app.UseOutputCache();
app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", async () =>
{
    HttpClient client = new HttpClient();

    try
    {
        var headers = builder.Configuration.GetSection("Headers").Get<Dictionary<string, string>>();
        if (headers != null && headers.Count > 0)
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var base64EncodedData = builder.Configuration.GetValue<string>("Base64Payload");
        if (base64EncodedData == null)
        {
            throw new Exception("Payload cannot be null.");
        }
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        var postBody = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

        var uri = builder.Configuration.GetValue<string>("Uri");
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = new StringContent(postBody, Encoding.UTF8, "application/json");

        var result = await client.SendAsync(request);
        string jsonString = await result.Content.ReadAsStringAsync();
        var jsonObject = JsonSerializer.Deserialize<object>(jsonString);

        return Results.Ok(jsonObject);
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine("\nException Caught!");
        Console.WriteLine("Message: {0} ", e.Message);
        return Results.BadRequest(e.Message);
    }
}).CacheOutput();

app.MapMetrics("/metrics");
app.MapHealthChecks("/health");

app.Run();
