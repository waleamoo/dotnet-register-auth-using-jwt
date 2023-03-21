using Twilio.Clients;
using Twilio.Http;
using HttpClient = System.Net.Http.HttpClient;

namespace ExamRoomBackend.Services
{
    public class TwilioClient : ITwilioRestClient
    {
        private readonly TwilioRestClient _innerClient;
        public string AccountSid => _innerClient.AccountSid;

        public string Region => _innerClient.Region;

        public Twilio.Http.HttpClient HttpClient => _innerClient.HttpClient;

        public Response Request(Request request) => _innerClient.Request(request);

        public Task<Response> RequestAsync(Request request) => _innerClient.RequestAsync(request);

        public TwilioClient(IConfiguration configuration, HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Add("X-Custom-Header", "CustomTwilioRestClient-Demo");
            _innerClient = new TwilioRestClient(configuration["Twilio:AccountSid"], configuration["Twilio:AuthToken"], httpClient: new SystemNetHttpClient(httpClient));
        }

    }
}
