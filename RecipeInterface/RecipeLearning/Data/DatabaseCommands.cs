using System.Data.SqlClient;

namespace RecipeLearning.Data;

internal static class DatabaseCommands
{
    private const string createDatabaseCommand = "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{0}') EXEC('CREATE DATABASE[{0}]');",
        createDataCollectionSchemaCommand = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'DataCollection' ) EXEC('CREATE SCHEMA [DataCollection]');",
        createDataParsingSchemaCommand = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'DataParsing' ) EXEC('CREATE SCHEMA [DataParsing]');",
        createDataMatchingSchemaCommand = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'DataMatching' ) EXEC('CREATE SCHEMA [DataMatching]');",
        createRecipeSimilaritySchemaCommand = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'RecipeSimilarity' ) EXEC('CREATE SCHEMA [RecipeSimilarity]');";

    public static async Task Create(string databaseName, string? sqlConnectionString, CancellationToken token = default)
    {
        SqlConnectionStringBuilder builder = new(sqlConnectionString);
        builder.InitialCatalog = "master";

        using (SqlConnection createSqlConnection = new(builder.ConnectionString))
        {
            await createSqlConnection.OpenAsync(token);

            using SqlCommand createDatabase = new(string.Format(createDatabaseCommand, databaseName), createSqlConnection);
            await createDatabase.ExecuteNonQueryAsync(token);
        }

        using SqlConnection sqlConnection = new(sqlConnectionString);
        await sqlConnection.OpenAsync(token);

        using SqlCommand dataCollection = new(createDataCollectionSchemaCommand, sqlConnection);
        using SqlCommand dataParsing = new(createDataParsingSchemaCommand, sqlConnection);
        using SqlCommand dataMatching = new(createDataMatchingSchemaCommand, sqlConnection);
        using SqlCommand recipeSimilarity = new(createRecipeSimilaritySchemaCommand, sqlConnection);

        await dataCollection.ExecuteNonQueryAsync(token);
        await dataParsing.ExecuteNonQueryAsync(token);
        await dataMatching.ExecuteNonQueryAsync(token);
        await recipeSimilarity.ExecuteNonQueryAsync(token);
    }
}
