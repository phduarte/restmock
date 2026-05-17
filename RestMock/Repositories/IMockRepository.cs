using RestMock.Domain;

namespace RestMock.Repositories;

public interface IMockRepository
{
    IEnumerable<EndpointModel> LoadAll();
    void SaveAll(IEnumerable<EndpointModel> endpoints);
}
