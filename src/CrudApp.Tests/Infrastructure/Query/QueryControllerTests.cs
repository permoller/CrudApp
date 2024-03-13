using CrudApp.Infrastructure.Query;
using CrudApp.Infrastructure.Testing;
using CrudApp.Tests.Infrastructure.Entities;
using CrudApp.Tests.Infrastructure.Http;
using Xunit.Abstractions;

namespace CrudApp.Tests.Infrastructure.Query;
public class QueryControllerTests : IntegrationTestsBase<QueryControllerTests.TestFixture>, IClassFixture<QueryControllerTests.TestFixture>
{
    public class TestFixture : WebAppFixture
    {
        public HttpClient Client = null!;
        public List<EntityId> DataIds = new();
        
        protected override async Task InitializeFixtureAsync()
        {
            await base.InitializeFixtureAsync();
        
            Client = CreateHttpClient(InitialUserId);

            async Task AddData(EntityId id, int nonNullableInt, int? nullableInt, string? testProp, string? ownedTestProp)
            {
                var data = new InfrastructureTestEntity(new InfrastructureTestOwnedEntity() { OwnedTestProp = ownedTestProp }) { Id = id, TestProp = testProp, NullableInt = nullableInt, NonNullableInt = nonNullableInt };
                DataIds.Add(await Client.PostEntityAsync(data));
            }
            await AddData(1, 1, 10, "Superman", "Clark Kent");
            await AddData(2, 1, 20, "Batman", "Bruce Wayne");
            await AddData(3, 2, 30, "Hulk", "Bruce Banner");
            await AddData(4, 3, null, "Iron Man", "Tony Stark");
            await AddData(5, 0, null, null, null);
            await AddData(6, 0, null, "", "");
        }
    }
    
    public QueryControllerTests(ITestOutputHelper testOutputHelper, TestFixture fixture) : base(testOutputHelper, fixture) { }

    [Fact]
    public async Task TestNoFilters()
    {
        var count = await Fixture.Client.Count<InfrastructureTestEntity>();
        Assert.Equal(Fixture.DataIds.Count, count);

        var items = await Fixture.Client.Query<InfrastructureTestEntity>();
        Assert.Equal(Fixture.DataIds, items.OrderBy(i => i.Id).Select(i => i.Id));

        var hulk = items.Single(i => i.Id == 3);
        Assert.Equal(2, hulk.NonNullableInt);
        Assert.Equal(30, hulk.NullableInt);
        Assert.Equal("Hulk", hulk.TestProp);
        Assert.Equal("Bruce Banner", hulk.NonNullableOwnedEntity.OwnedTestProp);
    }

    [Theory]
    [InlineData(null, "1,2,3,4,5,6")]
    [InlineData("", "1,2,3,4,5,6")]
    [InlineData("NonNullableInt EQ 2", "3")]
    [InlineData("NonNullableInt NE 2", "1,2,4,5,6")]
    [InlineData("NonNullableInt GT 2", "4")]
    [InlineData("NonNullableInt LT 2", "1,2,5,6")]
    [InlineData("NonNullableInt GE 2", "3,4")]
    [InlineData("NonNullableInt LE 2", "1,2,3,5,6")]
    [InlineData("NullableInt EQ 20", "2")]
    [InlineData("NullableInt NE 20", "1,3,4,5,6")]
    [InlineData("NullableInt GT 20", "3")]
    [InlineData("NullableInt LT 20", "1")]
    [InlineData("NullableInt GE 20", "2,3")]
    [InlineData("NullableInt LE 20", "1,2")]
    [InlineData("TestProp EQ Hulk", "3")]
    [InlineData("TestProp NE Hulk", "1,2,4,5,6")]
    [InlineData("NonNullableOwnedEntity.OwnedTestProp EQ Tony Stark", "4")]
    [InlineData("NonNullableInt EQ 1 AND NullableInt EQ 20", "2")]
    [InlineData("NonNullableInt EQ 1 AND NullableInt EQ 30", "")]
    // TODO: Filtering on null-values (not implemented yet)
    public async Task TestFilter(string filter, string expectedIdsCsv)
    {
        var filteringParams = new FilteringParams { Filter = filter };
        var expectedIds = expectedIdsCsv.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

        var count = await Fixture.Client.Count<InfrastructureTestEntity>(filteringParams);
        Assert.Equal(expectedIds.Count, count);

        var items = await Fixture.Client.Query<InfrastructureTestEntity>(filteringParams);
        Assert.Equal(expectedIds, items.OrderBy(i => i.Id).Select(i => i.Id.ToString()));
    }

    /// <summary>
    /// This test illustrates thing that does not work in the filter.
    /// If they start working (the test fails) you should think about if it is because you have made it work or you have broken somthing.
    /// </summary>
    [Theory]
    [InlineData("NonNullableInt EQ one", "Could not convert value 'one' to type 'Int32'.")] // using value that does not match property type
    [InlineData("TestProp EQ one AND two", "Invalid filter syntax.")] // using ' AND ' in value will make the parser think a new condition is starting
    [InlineData("NonNullableInt EQ 1 OR NullableInt EQ 30", "Could not convert value '1 OR NullableInt EQ 30' to type 'Int32'.")] // using OR is not supported, so it is assumed to be part of a string value
    [InlineData("TestProp GT Hulk", "Filter operator 'GT' not supported on type 'String'.")]
    [InlineData("TestProp LT Hulk", "Filter operator 'LT' not supported on type 'String'.")]
    [InlineData("TestProp GE Hulk", "Filter operator 'GE' not supported on type 'String'.")]
    [InlineData("TestProp LE Hulk", "Filter operator 'LE' not supported on type 'String'.")]
    [InlineData("PropThatDoesNotExists EQ 1", "Property 'PropThatDoesNotExists' not found on type 'InfrastructureTestEntity'.")]
    public async Task FilterLimitations(string filter, string expectedMessage)
    {
        var filteringParams = new FilteringParams { Filter = filter };

        var countException = await Assert.ThrowsAsync<ProblemDetailsApiException>(() => Fixture.Client.Count<InfrastructureTestEntity>(filteringParams));
        var countProblem = countException.ProblemDetails;
        Assert.NotNull(countProblem);
        Assert.Contains(expectedMessage, countProblem.Detail);

        var queryException = await Assert.ThrowsAsync<ProblemDetailsApiException>(() => Fixture.Client.Query<InfrastructureTestEntity>(filteringParams));
        var queryProblem = queryException.ProblemDetails;
        Assert.NotNull(queryProblem);
        Assert.Contains(expectedMessage, queryProblem.Detail);
    }
}
