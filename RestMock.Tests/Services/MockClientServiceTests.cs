using FluentAssertions;
using Moq;
using RestMock.Domain;
using RestMock.Repositories;
using RestMock.Services;
using Xunit;

namespace RestMock.Tests.Services;

public class MockClientServiceTests : IDisposable
{
    private readonly Mock<IMockRepository> _repoMock = new();
    private readonly MockClientService _service;

    public MockClientServiceTests()
    {
        EndpointCollection.Clear();
        _service = new MockClientService(_repoMock.Object);
    }

    public void Dispose() => EndpointCollection.Clear();

    private static EndpointModel MakeEndpoint(string pattern = "/test") =>
        new() { HttpMethod = "GET", Pattern = pattern };

    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_Endpoint_AppearsInGetAll()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);
        _service.GetAll().Should().Contain(ep);
    }

    [Fact]
    public void Add_Endpoint_FiresOnChangeEvent()
    {
        var fired = false;
        _service.OnChange += () => fired = true;
        _service.Add(MakeEndpoint());
        fired.Should().BeTrue();
    }

    [Fact]
    public void Add_Endpoint_CallsSaveAll()
    {
        _service.Add(MakeEndpoint());
        _repoMock.Verify(r => r.SaveAll(It.IsAny<IEnumerable<EndpointModel>>()), Times.Once);
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public void Remove_Endpoint_DisappearsFromGetAll()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);
        _service.Remove(ep.Id);
        _service.GetAll().Should().NotContain(ep);
    }

    [Fact]
    public void Remove_Endpoint_FiresOnChangeEvent()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);

        var fired = false;
        _service.OnChange += () => fired = true;
        _service.Remove(ep.Id);

        fired.Should().BeTrue();
    }

    [Fact]
    public void Remove_Endpoint_CallsSaveAll()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);
        _repoMock.Invocations.Clear();

        _service.Remove(ep.Id);
        _repoMock.Verify(r => r.SaveAll(It.IsAny<IEnumerable<EndpointModel>>()), Times.Once);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_Endpoint_ChangesFieldsInGetAll()
    {
        var ep = MakeEndpoint("/original");
        _service.Add(ep);

        _service.Update(ep.Id, new EndpointModel { HttpMethod = "POST", Pattern = "/updated", StatusCode = 201 });

        var result = _service.GetAll().First(e => e.Id == ep.Id);
        result.Pattern.Should().Be("/updated");
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public void Update_Endpoint_FiresOnChangeEvent()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);

        var fired = false;
        _service.OnChange += () => fired = true;
        _service.Update(ep.Id, new EndpointModel { HttpMethod = "GET", Pattern = "/new" });

        fired.Should().BeTrue();
    }

    [Fact]
    public void Update_Endpoint_CallsSaveAll()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);
        _repoMock.Invocations.Clear();

        _service.Update(ep.Id, new EndpointModel { HttpMethod = "GET", Pattern = "/new" });
        _repoMock.Verify(r => r.SaveAll(It.IsAny<IEnumerable<EndpointModel>>()), Times.Once);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingEndpoint_ReturnsIt()
    {
        var ep = MakeEndpoint();
        _service.Add(ep);
        _service.GetById(ep.Id).Should().Be(ep);
    }

    [Fact]
    public void GetById_NonExistingEndpoint_ReturnsNull()
    {
        _service.GetById(Guid.NewGuid()).Should().BeNull();
    }
}
