using BlingFire;
using Microsoft.EntityFrameworkCore;
using RecipeLearning.Data;
using RecipeLearning.DataCollection.Data;
using System.Text;
using System.Text.Json.Serialization;

namespace RecipeLearning.DataParsing.Outputs;

public class OutputBertTensors
{
    internal const int ChunkSize = 128;

    private readonly RecipeContext db;

    public OutputBertTensors(RecipeContext db)
    {
        this.db = db;
    }

    public EventHandler<double>? OnPercentCompleted;

    public async Task CalculateEmbeddings(CancellationToken token = default)
    {
        var totalCount = await db.IngredientSnapshots.CountAsync(token);

        using var snapshotTokenizerContext = new SnapshotTokenizerContext();

        BertTensorInput bertTensorInput = new()
        {
            InputWordIds = new int[totalCount][],
            InputMask = new int[totalCount][],
            InputTypeIds = new int[totalCount][],
            Labels = new int[totalCount][],
        };

        int i = 0;
        foreach (var snapshot in db.IngredientSnapshots.AsNoTracking())
        {
            snapshotTokenizerContext.Tokenize(snapshot, i, bertTensorInput);
            OnPercentCompleted?.Invoke(this, (double)i / totalCount);
            i++;
        }

        await Save(bertTensorInput.InputWordIds, "InputWordIds.csv", token);
        await Save(bertTensorInput.InputMask, "InputMask.csv", token);
        await Save(bertTensorInput.InputTypeIds, "InputTypeIds.csv", token);
        await Save(bertTensorInput.Labels, "Labels.csv", token);
    }

    private static async Task Save(int[][] input, string csvName, CancellationToken token = default)
    {
        using StreamWriter outfile = new(csvName);
        foreach (var line in input)
        {
            await outfile.WriteLineAsync(string.Join(',', line));
            if (token.IsCancellationRequested) return;
        }
    }

    private readonly struct SnapshotTokenizerContext : IDisposable
    {
        private const string tokenModelPath = "bert_base_tok.bin";
        private readonly ulong tokenModelId;

        public SnapshotTokenizerContext()
        {
            tokenModelId = BlingFireUtils.LoadModel(tokenModelPath);
        }

        public void Tokenize(IngredientSnapshot snapshot, int i, in BertTensorInput bertTensorInput)
        {
            int[] ids = new int[ChunkSize],
                starts = new int[ChunkSize],
                ends = new int[ChunkSize],
                inputMask = new int[ChunkSize],
                inputTypes = Enumerable.Repeat(snapshot.Id, ChunkSize).ToArray(),
                labels = new int[ChunkSize];
            List<int> tags = new(ChunkSize);
            StringBuilder inputBuilder = new();

            var snapshotLabels = SnapshotLabeler.Label(snapshot);
            foreach (var snapshotLabel in snapshotLabels)
            {
                if (inputBuilder.Length > 0)
                {
                    inputBuilder.Append(' ');
                    tags.Add(snapshotLabel.TagId);
                }

                inputBuilder.Append(snapshotLabel.Value);
                var byteCount = Encoding.UTF8.GetByteCount(snapshotLabel.Value);
                tags.AddRange(Enumerable.Repeat(snapshotLabel.TagId, byteCount));
            }

            byte[] inBytes = Encoding.UTF8.GetBytes(inputBuilder.ToString());

            var outputCount = BlingFireUtils.TextToIdsWithOffsets(tokenModelId, inBytes, inBytes.Length, ids, starts, ends, ids.Length, 0);
            for (int j = 0; j < outputCount; ++j)
            {
                inputMask[j] = 1;
                labels[j] = tags[starts[j]];
            }

            bertTensorInput.InputMask[i] = inputMask;
            bertTensorInput.InputTypeIds[i] = inputTypes;
            bertTensorInput.InputWordIds[i] = ids;
            bertTensorInput.Labels[i] = labels;
        }

        public void Dispose()
        {
            _ = BlingFireUtils.FreeModel(tokenModelId);
        }
    }

    private class BertTensorInput
    {
        [JsonPropertyName("input_word_ids")]
        public int[][] InputWordIds { get; set; } = Array.Empty<int[]>();
        [JsonPropertyName("input_mask")]
        public int[][] InputMask { get; set; } = Array.Empty<int[]>();

        [JsonPropertyName("input_type_ids")]
        public int[][] InputTypeIds { get; set; } = Array.Empty<int[]>();

        [JsonPropertyName("labels")]
        public int[][] Labels { get; set; } = Array.Empty<int[]>();
    }
}
