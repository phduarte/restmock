namespace RestMock.Domain;

public class EndpointModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int StatusCode { get; set; } = 200;
    public string HttpMethod { get; init; } = "GET";
    public string Pattern { get; set; }
    public int ProcessingTime { get; set; }
    public object? ResponseBody { get; set; }
    public string ContentType { get; set; } = "application/json";

    public bool Match(string method, string url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        if (!method.Equals(HttpMethod, StringComparison.OrdinalIgnoreCase)) return false;

        var uri = new Uri(url);

        if (Pattern.Contains('?'))
        {
            var patternLocalPath = Pattern.Split('?')[0];

            if (patternLocalPath.Equals(uri.LocalPath, StringComparison.OrdinalIgnoreCase))
            {
                var patternFields = Pattern.Split('?')[1];

                var patternArgs = patternFields.Split('&');

                var requestedArgs = uri.Query.Replace("?", string.Empty).Split('&');

                foreach (var arg in patternArgs)
                {
                    var parametro = arg.Split('=')[0];
                    var tipo = arg.Split('=')[1].ToLower(); //[string]

                    var requestedArg = requestedArgs.FirstOrDefault(a => a.StartsWith(parametro, StringComparison.OrdinalIgnoreCase));

                    if (string.IsNullOrEmpty(requestedArg)) return false;

                    //var k2 = requestedArg.Split('=')[0];
                    var valor = requestedArg.Split('=')[1]; //123456

                    if (tipo.StartsWith('['))
                    {
                        if (tipo.Contains("guid") && !Guid.TryParse(valor, out _))
                        {
                            return false;
                        }
                        else if (tipo.Contains("int") && !int.TryParse(valor, out _))
                        {
                            return false;
                        }
                        else if ((tipo.Contains("date") || tipo.Contains("datetime")) && !DateTime.TryParse(valor, out _))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
        else
        {
            if (Pattern.Equals(uri.LocalPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}