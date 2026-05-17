using Newtonsoft.Json;
using RestMock.Domain;

namespace RestMock.Repositories;

public class MockRepository : IMockRepository
{
    private readonly string _filePath;

    public MockRepository(IWebHostEnvironment env)
        : this(Path.Combine(env.ContentRootPath, "mocks.json")) { }

    internal MockRepository(string filePath)
    {
        _filePath = filePath;
    }

    public IEnumerable<EndpointModel> LoadAll()
    {
        if (!File.Exists(_filePath))
            return Enumerable.Empty<EndpointModel>();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<EndpointModel>>(json) ?? new();
        }
        catch
        {
            return Enumerable.Empty<EndpointModel>();
        }
    }

    public void SaveAll(IEnumerable<EndpointModel> endpoints)
    {
        var json = JsonConvert.SerializeObject(endpoints, Formatting.Indented);
        File.WriteAllText(_filePath, json);
    }
}
