using System;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;
using System.Web;
using System.Net.Http.Json;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NModules
{
    public class RequestSender
    {
        private readonly string url;
        private ILogger output;

        public string Url => $"{url}{Query ?? ""}";
        public string Query { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public object Body { get; private set; }

        public HttpResponseMessage Response { get; private set; }

        public HttpMethod HttpMethod { get; private set; }

        public RequestSender((HttpMethod Method, string Url) request)
        {
            this.HttpMethod = request.Method;
            this.url = request.Url;
        }

        public RequestSender WithHeaders(Dictionary<string, string> headers)
        {
            Headers = headers;
            return this;
        }

        public RequestSender WithQuery<TQuery>(TQuery obj)
        {
            Query = obj.ToQueryParams();
            return this;
        }

        public RequestSender WithBody<TBody>(TBody body)
        {
            Body = body;
            return this;
        }

        public RequestSender PrintOutput(ILogger output)
        {
            this.output = output;
            return this;
        }

        public async Task<HttpResponseMessage> SendAsync()
        {
            var rm = new HttpRequestMessage(HttpMethod, Url);

            if (Headers != null)
            {
                Headers.ToList().ForEach(p => rm.Headers.Add(p.Key, p.Value));
            }

            if (Body != null)
            {
                rm.Content = JsonContent.Create(Body);
            }

            Response = await new HttpClient().SendAsync(rm);

            if (output != null)
            {
                output.PrintRequestPayload(this);
            }

            return Response;
        }
    }

    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
    }

    public class HttpResponse<TData> : HttpResponse
    {
        public TData Data { get; set; }
    }

    public static class Exts
    {
        public static Task<TData> ToDataAsync<TData>(this HttpResponseMessage message)
        {
            return message.Content.ReadFromJsonAsync<TData>();
        }

        public static async Task<HttpResponse<TData>> ToHttpResponseAsync<TData>(this HttpResponseMessage message)
        {
            return new HttpResponse<TData>
            {
                StatusCode = message.StatusCode,
                Data = await message.ToDataAsync<TData>()
            };
        }

        public static string ToQueryParams(this object objParams)
        {
            var queryParamsList = objParams.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetEncodeValue(objParams)).ToList()
            .Select(x => $"{x.Key}={x.Value}");

            return $"?{string.Join("&", queryParamsList)}";
        }

        public static string GetEncodeValue(this PropertyInfo property, object objParams)
        {
            var value = property.GetValue(objParams, null).ToString();
            return HttpUtility.UrlEncode(value);
        }

        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        public static string ToJsonPrettify(this HttpResponseMessage resp)
        {
            var data = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (data != null)
            {

                return (data.StartsWith("{") ? data : $"{{\"content\":\"{data}\"}}").ToJsonPrettify();
            }
            else
            {
                return "{}";
            }
        }

        public static string ToJsonPrettify(this string json)
        {
            var jDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
        }

        public static void PrintRequestPayload(this ILogger output, RequestSender registrationRequest)
        {
            output.WriteLine($"{registrationRequest.HttpMethod}: {registrationRequest.Url}");

            if (registrationRequest.Headers != null)
            {
                output.WriteLine("Headers:");
                output.WriteLine(registrationRequest.Headers?.ToJson() ?? "{}");
            }

            if (registrationRequest.Body != null)
            {
                output.WriteLine("Body:");
                output.WriteLine(registrationRequest.Body?.ToJson() ?? "{}");
            }

            output.WriteLine("Response:");
            output.WriteLine($"StatusCode: {registrationRequest.Response?.StatusCode}");
            output.WriteLine($"Data: {registrationRequest.Response?.ToJsonPrettify() ?? "{}"}");
        }
    }
}