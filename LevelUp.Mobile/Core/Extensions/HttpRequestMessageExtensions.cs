namespace LevelUp.Mobile.Core.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();
                clone.Content = new StringContent(content,
                    System.Text.Encoding.UTF8,
                    request.Content.Headers.ContentType?.MediaType);
            }

            return clone;
        }
    }
}
