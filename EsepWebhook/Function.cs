using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    private static readonly HttpClient Client = new();

    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");

        dynamic? json = JsonConvert.DeserializeObject<dynamic>(input.ToString()!);
        string? issueUrl = (string?)json?.issue?.html_url;
        if (string.IsNullOrEmpty(issueUrl))
            return "ignored";

        string payload = JsonConvert.SerializeObject(new { text = $"Issue Created: {issueUrl}" });
        var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = Client.Send(request);
        using var reader = new StreamReader(response.Content.ReadAsStream());
        return reader.ReadToEnd();
    }
}
