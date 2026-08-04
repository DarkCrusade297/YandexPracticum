namespace EventManagerSystem.Tests;

[CollectionDefinition("Postgres collection")]
public sealed class PostgresCollection : ICollectionFixture<PostgresTestcontainerFixture>
{
}
