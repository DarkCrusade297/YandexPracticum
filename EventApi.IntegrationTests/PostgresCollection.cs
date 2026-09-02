namespace EventApi.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresTestcontainerFixture>
{
    public const string Name = "Service PostgreSQL collection";
}
