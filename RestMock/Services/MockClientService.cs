using RestMock.Domain;
using RestMock.Repositories;

namespace RestMock.Services;

public class MockClientService
{
    private readonly IMockRepository _repository;

    public event Action? OnChange;

    public MockClientService(IMockRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<EndpointModel> GetAll() => EndpointCollection.GetAll();

    public EndpointModel? GetById(Guid id) => EndpointCollection.GetById(id);

    public void Add(EndpointModel endpoint)
    {
        EndpointCollection.Add(endpoint);
        _repository.SaveAll(EndpointCollection.GetAll());
        OnChange?.Invoke();
    }

    public void Remove(Guid id)
    {
        EndpointCollection.Remove(id);
        _repository.SaveAll(EndpointCollection.GetAll());
        OnChange?.Invoke();
    }

    public void Update(Guid id, EndpointModel updated)
    {
        EndpointCollection.Update(id, updated);
        _repository.SaveAll(EndpointCollection.GetAll());
        OnChange?.Invoke();
    }
}
