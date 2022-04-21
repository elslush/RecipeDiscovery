using System.Data.SqlClient;

namespace RecipeLearning.Data;

internal static class CountViewCommands
{
    private const string createCountView = @"
CREATE VIEW [DataCollection].[_dta_mv_{1}] WITH SCHEMABINDING
 AS 
SELECT  count_big(*) as _col_1 FROM {0} 
",
        createCountIndex = @"
SET ARITHABORT ON
SET CONCAT_NULL_YIELDS_NULL ON
SET QUOTED_IDENTIFIER ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
SET NUMERIC_ROUNDABORT OFF

CREATE UNIQUE CLUSTERED INDEX [_dta_index__dta_mv_0_c_13_1317579732__K{0}] ON [DataCollection].[_dta_mv_{0}]
(
	[_col_1] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
";

    public static async Task CreateCountViews(string? sqlConnectionString, CancellationToken token = default, params string?[] tables)
    {
        foreach (var task in tables.Select(
                (table, i) => CreateCountView(table, i, sqlConnectionString, token)
            ))
        {
            await task;
        }
    }

    private static async Task CreateCountView(string? table, int i, string? sqlConnectionString, CancellationToken token = default)
    {
        using SqlConnection sqlConnection = new(sqlConnectionString);
        await sqlConnection.OpenAsync(token);

        using SqlCommand countViewCommand = new(string.Format(createCountView, table, i), sqlConnection);
        using SqlCommand countIndexCommand = new(string.Format(createCountIndex, i), sqlConnection);

        await countViewCommand.ExecuteNonQueryAsync(token);
        await countIndexCommand.ExecuteNonQueryAsync(token);
    }
}
