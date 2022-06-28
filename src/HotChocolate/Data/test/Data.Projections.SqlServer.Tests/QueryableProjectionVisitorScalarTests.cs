using System.Threading.Tasks;
using CookieCrumble;
using HotChocolate.Execution;
using HotChocolate.Types;

namespace HotChocolate.Data.Projections;

public class QueryableProjectionVisitorScalarTests
{
    private static readonly Foo[] _fooEntities =
    {
        new() { Bar = true, Baz = "a" }, new() { Bar = false, Baz = "b" }
    };

    private readonly SchemaCache _cache = new();

    [Fact]
    public async Task Create_NotSettable_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root{ notSettable }}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .AddResult(res1)
            .MatchAsync();
    }

    [Fact]
    public async Task Create_Computed_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root{ computed }}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .AddResult(res1)
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ProjectsTwoProperties_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root{ bar baz }}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .AddResult(res1)
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ProjectsOneProperty_Expression()
    {
        // arrange
        var tester = _cache.CreateSchema(_fooEntities);

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root{ baz }}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .AddResult(res1)
            .MatchAsync();
    }

    [Fact]
    public async Task Create_ProjectsOneProperty_WithResolver()
    {
        // arrange
        var tester = _cache.CreateSchema(
            _fooEntities,
            objectType: new ObjectType<Foo>(
                x => x
                    .Field("foo")
                    .Resolve(new[] { "foo" })
                    .Type<ListType<StringType>>()));

        // act
        var res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root{ baz foo }}")
                .Create());

        // assert
        await Snapshot
            .Create()
            .AddResult(res1)
            .MatchAsync();
    }

    [Fact]
    public async Task Test1()
    {
        // arrange
        IRequestExecutor tester = _cache.CreateSchema(_fooEntities);

        // act
        // assert
        IExecutionResult res1 = await tester.ExecuteAsync(
            QueryRequestBuilder.New()
                .SetQuery("{ root{ baz extended }}")
                .Create());

        res1.MatchSqlSnapshot();
    }

    public class Foo
    {
        public int Id { get; set; }

        public bool Bar { get; set; }

        public string? Baz { get; set; }

        public string Computed() => "Foo";

        public string? NotSettable { get; }

        public string Extended([Parent] Foo p) => (p.Baz ?? "") + p.Computed();
    }

    public class FooNullable
    {
        public int Id { get; set; }

        public bool? Bar { get; set; }

        public string? Baz { get; set; }
    }
}
