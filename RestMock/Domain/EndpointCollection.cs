namespace RestMock.Domain;

public class EndpointCollection
{
    private static readonly List<EndpointModel> _endpoints = new();

    public static void Add(EndpointModel request)
    {
        if (request == null) throw new ArgumentException(nameof(request));
        if (string.IsNullOrEmpty(request.Pattern)) throw new ArgumentException(nameof(request.Pattern));

        _endpoints.Add(request);
    }

    public static EndpointModel? Find(string method, string url)
    {
        return _endpoints.FirstOrDefault(e => e.Match(method, url));
    }

    public static void Remove(Guid guid)
    {
        var endpoint = _endpoints.FirstOrDefault(e => e.Id.Equals(guid)) ?? throw new ArgumentException(nameof(guid));

        _endpoints.Remove(endpoint);
    }

    public static EndpointModel? GetById(Guid guid)
    {
        return _endpoints.FirstOrDefault(e => e.Id.Equals(guid));
    }

    public static IEnumerable<EndpointModel> GetAll()
    {
        return _endpoints;
    }
}
