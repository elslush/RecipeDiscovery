using Microsoft.EntityFrameworkCore;
using RecipeLearning.Data;
using RecipeControlPanel.Dialogs;

var connectionStringDialog = new ConnectionStringDialog();
string connectionString = connectionStringDialog.GetConnectionString();

var optionsBuilder = new DbContextOptionsBuilder<RecipeContext>();
optionsBuilder.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(10000));
using var db = new RecipeContext(optionsBuilder.Options);

Console.Clear();
Console.WriteLine("Ensuring Database is Created...");
await db.EnsureCreated();

IDialog? dialog = new ActionDialog(db);
while (dialog is not null)
    dialog = await dialog.Execute();