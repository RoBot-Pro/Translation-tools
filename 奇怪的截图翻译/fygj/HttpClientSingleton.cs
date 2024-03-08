using System.Net.Http;

public static class HttpClientSingleton
{
    private static readonly HttpClient instance = new HttpClient();

    public static HttpClient Instance
    {
        get
        {
            return instance;
        }
    }
}
