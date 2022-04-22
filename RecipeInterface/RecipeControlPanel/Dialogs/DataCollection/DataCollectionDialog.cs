using DustInTheWind.ConsoleTools.Controls;
using DustInTheWind.ConsoleTools.Controls.Menus;
using DustInTheWind.ConsoleTools.Controls.Menus.MenuItems;
using DustInTheWind.ConsoleTools.Controls.Spinners;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeLearning.Data;
using RecipeLearning.DataCollection;
using RecipeLearning.Import;
using RecipeLearning.Import.Streams;
using System.Data.SqlClient;
using System.Diagnostics;

namespace RecipeControlPanel.Dialogs.DataCollection;

internal class DataCollectionDialog : IDialog, IDisposable
{
    private readonly ManualResetEvent oSignalEvent = new(false);
    private readonly ManualResetEventSlim finishEvent = new();
    private readonly RecipeContext db;

    private ScrollMenu scrollMenu = new()
    {
        //HorizontalAlignment = HorizontalAlignment.Left,
        EraseAfterClose = true,
    };
    private readonly ProgressBar progressBar = new()
    {
        LabelText = "Copied",
    };
    private readonly Spinner spinner = new()
    {
        Label = "Downloading From Cloud ",
    };

    private readonly RecipeImporter recipeImporter;
    private readonly IngredientsImporter ingredientsImporter;
    private readonly InstructionImporter instructionImporter;
    private readonly NutritionImporter nutritionImporter;
    private readonly SnapshotImporter snapshotImporter;
    private readonly TagImporter tagImporter;
    private readonly SubstitutionsImporter substitutionsImporter;

    private IImporter? importer;
    private readonly IDialog nextDialog;

    internal DataCollectionDialog(RecipeContext db, IDialog nextDialog)
    {
        this.db = db;
        this.nextDialog = nextDialog;
        recipeImporter = new(db, NullLogger.Instance);
        ingredientsImporter = new(db, NullLogger.Instance);
        instructionImporter = new(db, NullLogger.Instance);
        nutritionImporter = new(db, NullLogger.Instance);
        snapshotImporter = new(db, NullLogger.Instance);
        tagImporter = new(db, NullLogger.Instance);
        substitutionsImporter = new(db, NullLogger.Instance);

        RegisterEvents(recipeImporter);
        RegisterEvents(ingredientsImporter);
        RegisterEvents(instructionImporter);
        RegisterEvents(nutritionImporter);
        RegisterEvents(snapshotImporter);
        RegisterEvents(tagImporter);
        RegisterEvents(substitutionsImporter);
    }

    private void RegisterEvents(IImporter importer)
    {
        importer.OnStatusChanged += OnStatusChanged;
        importer.OnStreamProgressChanged += OnStreamProgressChanged;
        importer.OnSqlRowsCopied += OnSqlRowsCopied;
    }

    private void OnSqlRowsCopied(object? sender, SqlRowsCopiedEventArgs e)
    {
        progressBar.UnitOfMeasurement = $"% or {e.RowsCopied} rows to Database";
    }

    private void OnStreamProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        progressBar.Value = Convert.ToInt32(e.ProgressPercentage * 100);
    }

    private void OnStatusChanged(object? sender, ImportStatus importStatus)
    {
        switch (importStatus)
        {
            case ImportStatus.Downloading:
                Console.Clear();
                spinner.Display();
                break;
            case ImportStatus.Finished:
                finishEvent.Set();
                break;
            default:
                spinner.Close();
                progressBar.Display();
                break;
        }
    }

    private void SetImporter(IImporter? importer)
    {
        this.importer = importer;
        oSignalEvent.Set();
    }

    private async Task RefreshMenuItems()
    {
        scrollMenu = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            EraseAfterClose = true,
        };

        var recipeCount = await db.Recipes.CountAsync();
        var ingredientCount = await db.Ingredients.CountAsync();
        var instructionCount = await db.Instructions.CountAsync();
        var nutritionCount = await db.Nutritions.CountAsync();
        var snapshotCount = await db.IngredientSnapshots.CountAsync();
        var tagCount = await db.IngredientTags.CountAsync();
        var substitutionCount = await db.Substitutions.CountAsync();

        List<int> emptyStepNumbers = new(7);
        List<string> emptyNames = new(7);
        if (recipeCount < 1) { emptyStepNumbers.Add(1); emptyNames.Add("recipes"); }
        if (ingredientCount < 1) { emptyStepNumbers.Add(2); emptyNames.Add("ingredients"); }
        if (instructionCount < 1) { emptyStepNumbers.Add(3); emptyNames.Add("instructions"); }
        if (nutritionCount < 1) { emptyStepNumbers.Add(4); emptyNames.Add("nutrition"); }
        if (snapshotCount < 1) { emptyStepNumbers.Add(5); emptyNames.Add("snapshots"); }
        if (tagCount < 1) { emptyStepNumbers.Add(6); emptyNames.Add("tags"); }
        if (substitutionCount < 1) { emptyStepNumbers.Add(7); emptyNames.Add("substitutions"); }

        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step(s) {string.Join(',', emptyStepNumbers)}: Batch Import {string.Join(',', emptyNames)} into database ({recipeCount} in Database)", Command = new ActionCommand(() => SetImporter(recipeImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 1: Import recipes into database ({recipeCount} in Database)", Command = new ActionCommand(() => SetImporter(recipeImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 2: Import ingredients into database ({ingredientCount} in Database)", Command = new ActionCommand(() => SetImporter(ingredientsImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 3: Import instructions into database ({instructionCount} in Database)", Command = new ActionCommand(() => SetImporter(instructionImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 4: Import nutrition into database ({nutritionCount} in Database)", Command = new ActionCommand(() => SetImporter(nutritionImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 5: Import snapshots into database ({snapshotCount} in Database)", Command = new ActionCommand(() => SetImporter(snapshotImporter)), });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 6: Import tags into database ({tagCount} in Database)", Command = new ActionCommand(() => SetImporter(tagImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Step 7: Import substitutions into database ({substitutionCount} in Database)", Command = new ActionCommand(() => SetImporter(substitutionsImporter)) });
        scrollMenu.AddItem(new LabelMenuItem() { Text = $"Go Back", Command = new ActionCommand(() => SetImporter(null)) });
    }

    public async Task<IDialog?> Execute(CancellationToken token = default)
    {
        Console.Clear();
        spinner.Label = "Loading Menu Options ";
        spinner.Display();

        await RefreshMenuItems();

        spinner.Close();
        spinner.Label = "Downloading From Cloud ";

        Console.WriteLine("Which Table would you like to seed?");
        Console.WriteLine(string.Empty);
        scrollMenu.Display();

        oSignalEvent.WaitOne();
        oSignalEvent.Reset();

        Console.Clear();
        if (importer is null)
            return nextDialog;

        Stopwatch stopwatch = new();
        stopwatch.Start();
        await importer.Import(token);

        finishEvent.Wait(token);

        stopwatch.Stop();

        Console.WriteLine(string.Empty);
        Console.WriteLine($"Finished in: {stopwatch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine(string.Empty);
        Console.WriteLine("Press any key to continue.");

        Console.ReadKey();

        progressBar.Close();
        finishEvent.Reset();
        importer = null;

        return this;
    }

    public void Dispose()
    {
        spinner.Dispose();
    }
}
