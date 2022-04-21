using Microsoft.SqlServer.Server;
using System.Data.SqlClient;

namespace DatabaseFunctions
{
    public class Functions
    {
        [SqlFunction(DataAccess = DataAccessKind.None, IsDeterministic = true)]
        public static string CleanText(string item)
        {
            return IngredientCleaner.CleanText(item);
        }
    }
}