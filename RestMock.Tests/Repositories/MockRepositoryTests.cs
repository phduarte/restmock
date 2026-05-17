using FluentAssertions;
using Newtonsoft.Json;
using RestMock.Domain;
using RestMock.Repositories;
using Xunit;

namespace RestMock.Tests.Repositories;

public class MockRepositoryTests : IDisposable
{
    private readonly string _filePath;
    private readonly MockRepository _repository;

    public MockRepositoryTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"restmock_test_{Guid.NewGuid():N}.json");
        _repository = new MockRepository(_filePath);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    private static EndpointModel MakeEndpoint(string method = "GET", string pattern = "/test") =>
        new() { HttpMethod = method, Pattern = pattern, StatusCode = 200 };

    // ── LoadAll ───────────────────────────────────────────────────────────────

    [Fact]
    public void LoadAll_FileDoesNotExist_ReturnsEmpty()
    {
        _repository.LoadAll().Should().BeEmpty();
    }

    [Fact]
    public void LoadAll_CorruptedFile_ReturnsEmpty()
    {
        File.WriteAllText(_filePath, "this is not valid json {{{{");
        _repository.LoadAll().Should().BeEmpty();
    }

    [Fact]
    public void LoadAll_EmptyArray_ReturnsEmpty()
    {
        File.WriteAllText(_filePath, "[]");
        _repository.LoadAll().Should().BeEmpty();
    }

    // ── SaveAll + LoadAll round-trip ──────────────────────────────────────────

    [Fact]
    public void SaveAndLoad_RoundTrips_SingleEndpoint()
    {
        var ep = MakeEndpoint("POST", "/api/orders");
        ep.StatusCode = 201;
        ep.ProcessingTime = 150;
        ep.ContentType = "application/json";
        ep.ResponseBody = "{}";

        _repository.SaveAll(new[] { ep });
        var loaded = _repository.LoadAll().ToList();

        loaded.Should().HaveCount(1);
        loaded[0].Id.Should().Be(ep.Id);
        loaded[0].HttpMethod.Should().Be("POST");
        loaded[0].Pattern.Should().Be("/api/orders");
        loaded[0].StatusCode.Should().Be(201);
        loaded[0].ProcessingTime.Should().Be(150);
        loaded[0].ContentType.Should().Be("application/json");
    }

    [Fact]
    public void SaveAndLoad_RoundTrips_MultipleEndpoints()
    {
        var endpoints = new[]
        {
            MakeEndpoint("GET", "/a"),
            MakeEndpoint("POST", "/b"),
            MakeEndpoint("DELETE", "/c"),
        };

        _repository.SaveAll(endpoints);
        var loaded = _repository.LoadAll().ToList();

        loaded.Should().HaveCount(3);
        loaded.Select(e => e.Pattern).Should().BeEquivalentTo("/a", "/b", "/c");
    }

    [Fact]
    public void SaveAll_WritesValidJson()
    {
        _repository.SaveAll(new[] { MakeEndpoint() });
        var content = File.ReadAllText(_filePath);
        var act = () => JsonConvert.DeserializeObject(content);
        act.Should().NotThrow();
    }

    [Fact]
    public void SaveAll_EmptyList_WritesEmptyArray()
    {
        _repository.SaveAll(Enumerable.Empty<EndpointModel>());
        _repository.LoadAll().Should().BeEmpty();
    }

    [Fact]
    public void SaveAll_OverwritesPreviousFile()
    {
        _repository.SaveAll(new[] { MakeEndpoint("GET", "/first") });
        _repository.SaveAll(new[] { MakeEndpoint("POST", "/second") });

        var loaded = _repository.LoadAll().ToList();
        loaded.Should().HaveCount(1);
        loaded[0].Pattern.Should().Be("/second");
    }
}
