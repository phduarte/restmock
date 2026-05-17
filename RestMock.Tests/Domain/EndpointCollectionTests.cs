using FluentAssertions;
using RestMock.Domain;
using Xunit;

namespace RestMock.Tests.Domain;

public class EndpointCollectionTests : IDisposable
{
    public EndpointCollectionTests() => EndpointCollection.Clear();
    public void Dispose() => EndpointCollection.Clear();

    private static EndpointModel MakeEndpoint(string method = "GET", string pattern = "/test") =>
        new() { HttpMethod = method, Pattern = pattern };

    // ── Add ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Add_ValidEndpoint_AppearsInGetAll()
    {
        var ep = MakeEndpoint();
        EndpointCollection.Add(ep);
        EndpointCollection.GetAll().Should().Contain(ep);
    }

    [Fact]
    public void Add_NullEndpoint_ThrowsArgumentException()
    {
        var act = () => EndpointCollection.Add(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_EmptyPattern_ThrowsArgumentException()
    {
        var act = () => EndpointCollection.Add(new EndpointModel { HttpMethod = "GET", Pattern = "" });
        act.Should().Throw<ArgumentException>();
    }

    // ── Find ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Find_NoEndpoints_ReturnsNull()
    {
        EndpointCollection.Find("GET", "http://localhost/test").Should().BeNull();
    }

    [Fact]
    public void Find_MatchingEndpoint_ReturnsEndpoint()
    {
        var ep = MakeEndpoint("GET", "/api/users");
        EndpointCollection.Add(ep);
        EndpointCollection.Find("GET", "http://localhost/api/users").Should().Be(ep);
    }

    [Fact]
    public void Find_NoMatchingEndpoint_ReturnsNull()
    {
        EndpointCollection.Add(MakeEndpoint("GET", "/api/users"));
        EndpointCollection.Find("POST", "http://localhost/api/users").Should().BeNull();
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public void GetById_ExistingId_ReturnsEndpoint()
    {
        var ep = MakeEndpoint();
        EndpointCollection.Add(ep);
        EndpointCollection.GetById(ep.Id).Should().Be(ep);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        EndpointCollection.GetById(Guid.NewGuid()).Should().BeNull();
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Fact]
    public void Remove_ExistingId_EndpointDisappears()
    {
        var ep = MakeEndpoint();
        EndpointCollection.Add(ep);
        EndpointCollection.Remove(ep.Id);
        EndpointCollection.GetAll().Should().NotContain(ep);
    }

    [Fact]
    public void Remove_NonExistingId_ThrowsArgumentException()
    {
        var act = () => EndpointCollection.Remove(Guid.NewGuid());
        act.Should().Throw<ArgumentException>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ExistingId_ChangesFields()
    {
        var ep = MakeEndpoint("GET", "/original");
        EndpointCollection.Add(ep);

        var updated = new EndpointModel { HttpMethod = "POST", Pattern = "/updated", StatusCode = 201 };
        EndpointCollection.Update(ep.Id, updated);

        var result = EndpointCollection.GetById(ep.Id)!;
        result.HttpMethod.Should().Be("POST");
        result.Pattern.Should().Be("/updated");
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public void Update_ExistingId_PreservesId()
    {
        var ep = MakeEndpoint();
        EndpointCollection.Add(ep);
        var originalId = ep.Id;

        EndpointCollection.Update(ep.Id, new EndpointModel { HttpMethod = "PUT", Pattern = "/new" });

        EndpointCollection.GetById(originalId).Should().NotBeNull();
    }

    [Fact]
    public void Update_NonExistingId_ThrowsArgumentException()
    {
        var act = () => EndpointCollection.Update(Guid.NewGuid(), MakeEndpoint());
        act.Should().Throw<ArgumentException>();
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public void GetAll_MultipleEndpoints_ReturnsAll()
    {
        var ep1 = MakeEndpoint("GET", "/a");
        var ep2 = MakeEndpoint("POST", "/b");
        EndpointCollection.Add(ep1);
        EndpointCollection.Add(ep2);
        EndpointCollection.GetAll().Should().BeEquivalentTo(new[] { ep1, ep2 });
    }

    [Fact]
    public void GetAll_Empty_ReturnsEmptySequence()
    {
        EndpointCollection.GetAll().Should().BeEmpty();
    }
}
