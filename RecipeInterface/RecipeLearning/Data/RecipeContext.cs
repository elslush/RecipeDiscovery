using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RecipeLearning.DataCollection.Data;
using RecipeLearning.DataMatching.Data;
using RecipeLearning.DataParsing.Data;
using RecipeLearning.RecipeSimilarities.Data;
using System.Data.SqlClient;

namespace RecipeLearning.Data;

public class RecipeContext : DbContext
{
    public RecipeContext(DbContextOptions options) : base(options) { }

    // Data Collection
    public DbSet<IngredientSnapshot> IngredientSnapshots { get; set; } = default!;

    public DbSet<IngredientTag> IngredientTags { get; set; } = default!;

    public DbSet<Recipe> Recipes { get; set; } = default!;

    public DbSet<Ingredient> Ingredients { get; set; } = default!;

    public DbSet<Nutrition> Nutritions { get; set; } = default!;

    public DbSet<Instruction> Instructions { get; set; } = default!;

    public DbSet<Substitution> Substitutions { get; set; } = default!; 

    // Data Parsing
    public DbSet<ParsedIngredient> ParsedIngredients { get; set; } = default!;

    public DbSet<ParsedSubstitution> ParsedSubstitutions { get; set; } = default!;

    // Data Matching
    public DbSet<MatchedIngredient> MatchedIngredients { get; set; } = default!;

    public DbSet<MatchedSubstitution> MatchedSubstitutions { get; set; } = default!;

    // Recipe Similarity
    public DbSet<RecipeSimilarity> RecipeSimilarities { get; set; } = default!;

    public DbSet<CombinedIngredient> CombinedIngredients { get; set; } = default!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<double>()
            .HaveConversion<DecimalConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Data Collection
        modelBuilder.Entity<IngredientSnapshot>(
                eb =>
                {
                    eb.HasKey(nameof(IngredientSnapshot.Id));
                    eb.ToTable(nameof(IngredientSnapshots), schema: "DataCollection");
                    eb.Property(b => b.Input).HasColumnType("nvarchar(1200)").HasDefaultValue(string.Empty);
                    eb.Property(b => b.Name).HasColumnType("nvarchar(200)");
                    eb.Property(b => b.Quantity).HasColumnType("nvarchar(200)");
                    eb.Property(b => b.RangeEnd).HasColumnType("nvarchar(200)");
                    eb.Property(b => b.Unit).HasColumnType("nvarchar(200)");
                    eb.Property(b => b.Comment).HasColumnType("nvarchar(400)");
                    eb.Property(b => b.CleanedInput).HasComputedColumnSql("DataCollection.CleanText([Input])", stored: true);
                });
        modelBuilder.Entity<IngredientTag>(
            eb =>
            {
                eb.HasKey(nameof(IngredientTag.Id));
                eb.ToTable(nameof(IngredientTags), schema: "DataCollection");
                eb.Property(b => b.Id).ValueGeneratedNever();
                eb.Property(b => b.Tag).HasColumnType("nvarchar(10)");
            });
        modelBuilder.Entity<Recipe>(
            eb =>
            {
                eb.HasKey(nameof(Recipe.RecipeID));
                eb.ToTable(nameof(Recipes), schema: "DataCollection");
                eb.Property(b => b.RecipeID).HasColumnType("uniqueidentifier").HasConversion<Guid>();
                eb.Property(b => b.Name).HasColumnType("nvarchar(500)");
                eb.Property(b => b.Url).HasColumnType("nvarchar(500)");
                //eb.HasMany(b => b.Ingredients).WithOne().HasForeignKey(ingredient => ingredient.RecipeID);
            });
        modelBuilder.Entity<Instruction>(
            eb =>
            {
                eb.HasKey(nameof(Instruction.InstructionId));
                eb.ToTable(nameof(Instructions), schema: "DataCollection");
                eb.Property(b => b.RecipeID).HasColumnType("uniqueidentifier").HasConversion<Guid>();
                eb.Property(b => b.Text).HasColumnType("nvarchar(2000)");
                eb.Property(b => b.Sequence).HasColumnType("smallint");
                eb.HasOne<Recipe>().WithMany().HasForeignKey(p => p.RecipeID);
            });
        modelBuilder.Entity<Ingredient>(
            eb =>
            {
                eb.HasKey(nameof(Ingredient.IngredientID));
                eb.ToTable(nameof(Ingredients), schema: "DataCollection");
                eb.Property(b => b.RecipeID).HasColumnType("uniqueidentifier").HasConversion<Guid>();
                eb.Property(b => b.Description).HasColumnType("nvarchar(1200)");
                eb.Property(b => b.CleanedInput).HasComputedColumnSql("DataCollection.CleanText([Description])", stored: true);
            });
        modelBuilder.Entity<Nutrition>(
            eb =>
            {
                eb.HasKey(nameof(Nutrition.NutritionID));
                eb.ToTable(nameof(Nutrition), schema: "DataCollection");
                eb.Property(b => b.NutritionID).ValueGeneratedNever();
                eb.Property(b => b.Name).HasColumnType("nvarchar(60)");
                eb.Property(b => b.HouseholdDesc1).HasColumnType("nvarchar(100)");
                eb.Property(b => b.HouseholdDesc2).HasColumnType("nvarchar(100)");
                eb.Property(b => b.Water).HasDefaultValue(0).HasPrecision(10, 2);
                eb.Property(b => b.Calories).HasDefaultValue(0).HasPrecision(10, 2); 
                eb.Property(b => b.Protein).HasDefaultValue(0).HasPrecision(10, 2);
                eb.Property(b => b.Fat).HasDefaultValue(0).HasPrecision(10, 2); 
                eb.Property(b => b.Ash).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Carbohydrate).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Fiber).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Sugar).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Calcium).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Iron).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Magnesium).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Phosphorus).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Potassium).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Sodium).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Zinc).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Copper).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Manganese).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Selenium).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminC).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Thiamin).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Riboflavin).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Niacin).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.PantothenicAcid).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminB6).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Folate).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.FolicAcid).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.FoodFolate).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.DietaryFolateEquiv).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Choline).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminB12).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminA).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminARentinolEquiv).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Rentinol).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.AlphaCarotene).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.BetaCarotene).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.BetaCryptoxanthin).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Lycopene).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.LuteinZeazathin).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminE).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminD).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminDIU).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.VitaminK).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.SaturatedFat).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.MonounsaturatedFat).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.PolyunsaturatedFat).HasDefaultValue(0).HasPrecision(10, 2);  
                eb.Property(b => b.Cholesterol).HasDefaultValue(0).HasPrecision(10, 2);
                eb.Property(b => b.RefusePercentage).HasDefaultValue(0);
                eb.Property(b => b.CleanedInput).HasComputedColumnSql("DataCollection.CleanText([Name])", stored: true);
            });
        modelBuilder.Entity<Substitution>(
            eb =>
            {
                eb.HasKey(nameof(Substitution.SubstitutionID));
                eb.ToTable(nameof(Substitutions), schema: "DataCollection");
                eb.Property(b => b.Substitution1).HasColumnType("nvarchar(1500)");
                eb.Property(b => b.CleanedSubstitution1).HasComputedColumnSql("DataCollection.CleanText([Substitution1])", stored: true);
                eb.Property(b => b.Substitution2).HasColumnType("nvarchar(1500)");
                eb.Property(b => b.CleanedSubstitution2).HasComputedColumnSql("DataCollection.CleanText([Substitution2])", stored: true);
                eb.Property(b => b.Webpage).HasColumnType("nvarchar(150)");
            });

        //Data Parsing
        modelBuilder.Entity<ParsedIngredient>(
            eb =>
            {
                eb.HasKey(nameof(ParsedIngredient.ParsedIngredientID));
                eb.ToTable(nameof(ParsedIngredients), schema: "DataParsing");
                eb.Property(b => b.Name).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Quantity).HasColumnType("float").HasDefaultValue(1);
                eb.Property(b => b.Unit).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Comment).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Other).HasColumnType("nvarchar(150)");
                eb.HasOne<Ingredient>().WithMany().HasForeignKey(p => p.IngredientID);
            });
        modelBuilder.Entity<ParsedSubstitution>(
            eb =>
            {
                eb.HasKey(nameof(ParsedSubstitution.ParsedSubstitutionID));
                eb.ToTable(nameof(ParsedSubstitutions), schema: "DataParsing");
                eb.Property(b => b.Substitution1Name).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution1Quantity).HasColumnType("float").HasDefaultValue(1);
                eb.Property(b => b.Substitution1Unit).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution1Comment).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution1Other).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution2Name).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution2Quantity).HasColumnType("float").HasDefaultValue(1);
                eb.Property(b => b.Substitution2Unit).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution2Comment).HasColumnType("nvarchar(150)");
                eb.Property(b => b.Substitution2Other).HasColumnType("nvarchar(150)");
                eb.HasOne<Substitution>().WithMany().HasForeignKey(p => p.SubstitutionID);
            });

        // Data Matching
        modelBuilder.Entity<MatchedIngredient>(
            eb =>
            {
                eb.HasKey(nameof(MatchedIngredient.MatchedIngredientID));
                eb.ToTable(nameof(MatchedIngredients), schema: "DataMatching");
                eb.Property(b => b.Probability).HasDefaultValue(0);
                eb.HasOne<Nutrition>().WithMany().HasForeignKey(p => p.NutritionID);
                eb.HasOne<Ingredient>().WithMany().HasForeignKey(p => p.IngredientID);
            });
        modelBuilder.Entity<MatchedSubstitution>(
            eb =>
            {
                eb.HasKey(nameof(MatchedSubstitution.MatchedSubstitutionID));
                eb.ToTable(nameof(MatchedSubstitutions), schema: "DataMatching");
                eb.Property(b => b.Probability1).HasDefaultValue(0);
                eb.Property(b => b.Probability2).HasDefaultValue(0);
                eb.HasOne<Nutrition>().WithMany().HasForeignKey(p => p.Nutrition1ID).OnDelete(DeleteBehavior.NoAction);
                eb.HasOne<Nutrition>().WithMany().HasForeignKey(p => p.Nutrition2ID).OnDelete(DeleteBehavior.NoAction);
                eb.HasOne<Substitution>().WithMany().HasForeignKey(p => p.Substitution1ID).OnDelete(DeleteBehavior.NoAction);
                eb.HasOne<Substitution>().WithMany().HasForeignKey(p => p.Substitution2ID).OnDelete(DeleteBehavior.NoAction);
            });

        // Recipe Similarity
        modelBuilder.Entity<RecipeSimilarity>(
            eb =>
            {
                eb.HasKey(nameof(RecipeSimilarity.RecipeSimilarityID));
                eb.ToTable(nameof(RecipeSimilarities), schema: "RecipeSimilarity");
                eb.Property(b => b.RecipeID).HasColumnType("uniqueidentifier").HasConversion<Guid>();
                eb.Property(b => b.SimilarRecipeID).HasColumnType("uniqueidentifier").HasConversion<Guid>();
                eb.HasOne<Recipe>().WithMany().HasForeignKey(similarity => similarity.RecipeID).OnDelete(DeleteBehavior.NoAction);
                eb.HasOne<Recipe>().WithMany().HasForeignKey(similarity => similarity.SimilarRecipeID).OnDelete(DeleteBehavior.NoAction);
            });
        modelBuilder.Entity<CombinedIngredient>(
            eb =>
            {
                eb.HasKey(nameof(CombinedIngredient.IngredientID));
                //eb.HasNoKey();
                eb.ToView(nameof(CombinedIngredients), schema: "RecipeSimilarity");
            });
    }

    private class DecimalConverter : ValueConverter<double, decimal>
    {
        public DecimalConverter() : base(
            v => Convert.ToDecimal(v),
            v => decimal.ToDouble(v)
        ) { }
    }

    public async Task EnsureCreated(CancellationToken token = default)
    {
        if (!await Database.CanConnectAsync(token))
        {
            var sqlConnectionString = Database.GetConnectionString();

            await DatabaseCommands.Create(Database.GetDbConnection().Database, sqlConnectionString, token);
            await AssemblyCommands.CreateAssembly(sqlConnectionString, token);

            using (SqlConnection sqlConnection = new(sqlConnectionString))
            {
                await sqlConnection.OpenAsync(token);
                foreach (var innerCommandString in Database.GenerateCreateScript().Split("GO"))
                {
                    using SqlCommand creatTableCommand = new(innerCommandString, sqlConnection);
                    await creatTableCommand.ExecuteNonQueryAsync(token);
                }
            }

            await CombinedViewCommands.CreateCombinedViews(sqlConnectionString, token);

            await CountViewCommands.CreateCountViews(sqlConnectionString, token,
                Model.FindEntityType(typeof(Recipe))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(Ingredient))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(Instruction))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(Nutrition))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(IngredientSnapshot))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(IngredientTag))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(Substitution))?.GetSchemaQualifiedTableName(),
                Model.FindEntityType(typeof(IngredientSnapshot))?.GetSchemaQualifiedTableName()
            );

            await IndexCommands.CreateIndicies(sqlConnectionString, token);
        }
    }
}
