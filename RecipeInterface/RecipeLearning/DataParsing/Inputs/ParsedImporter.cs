using Google.Apis.Download;
using Google.Apis.Services;
using Google.Apis.Drive.v3;
using Microsoft.EntityFrameworkCore;
using Sylvan.Data;
using Sylvan.Data.Csv;
using System.IO.Compression;
using System.Data.SqlClient;
using RecipeLearning.Import.Streams;
using RecipeLearning.Import;

namespace RecipeLearning.DataParsing.Inputs;

public abstract class ParsedImporter<T> : IImporter where T : class
{
    private static readonly string tempFolder = Path.Combine(Path.GetTempPath(), "RecipeLearning");

    private readonly DbContext db;
    private readonly string? tableName;
    private readonly SqlBulkCopyColumnMapping[] columnMappings;

    private readonly CsvDataReaderOptions options;
    private readonly FileRetriever fileRetriever;

    internal ParsedImporter(DbContext db, FileRetriever fileRetriever, params SqlBulkCopyColumnMapping[] sqlColumns)
    {
        this.db = db;
        this.fileRetriever = fileRetriever;
        columnMappings = sqlColumns;

        var entityType = db.Model.FindEntityType(typeof(T));
        tableName = entityType?.GetSchemaQualifiedTableName();

        options = GetOptions(db, tableName, sqlColumns);
    }

    public event EventHandler<ImportStatus>? OnStatusChanged;

    public event EventHandler<SqlRowsCopiedEventArgs>? OnSqlRowsCopied;

    public event EventHandler<ProgressChangedEventArgs>? OnStreamProgressChanged;

    public event EventHandler<IDownloadProgress>? OnDownloadProgressChanged;

    private static CsvDataReaderOptions GetOptions(DbContext db, string? tableName, SqlBulkCopyColumnMapping[] sqlColumns)
    {

        string sqlColumnsNames = sqlColumns.Length > 0 ? string.Join(',', sqlColumns.Select(column => column.DestinationColumn)) : "*";

        using SqlConnection sqlConnection = new(db.Database.GetConnectionString());
        sqlConnection.Open();

        using var sqlCommand = new SqlCommand($"select top 0 {sqlColumnsNames} from {tableName}", sqlConnection);
        using var reader = sqlCommand.ExecuteReader();

        return new CsvDataReaderOptions
        {
            Schema = new CsvSchema(reader.GetColumnSchema()),
        };
    }

    protected abstract IEnumerable<T> ParseReader(CsvDataReader csvDataReader);

    public async Task Import(CancellationToken token = default!)
    {
        if (await db.Database.EnsureCreatedAsync(token) || true)
        {
            if (!fileRetriever.DoesFileExist)
            {
                Directory.CreateDirectory(tempFolder);

                if (!fileRetriever.DoesZipFileExist)
                {
                    OnStatusChanged?.Invoke(this, ImportStatus.Downloading);
                    await fileRetriever.DownloadZip((e) => OnDownloadProgressChanged?.Invoke(this, e), token);
                }

                OnStatusChanged?.Invoke(this, ImportStatus.Unzipping);
                fileRetriever.Unzip();
            }

            OnStatusChanged?.Invoke(this, ImportStatus.Copying);

            using SqlConnection sqlConnection = new(db.Database.GetConnectionString());
            await sqlConnection.OpenAsync(token);

            using var progressReader = new ProgressTextReader(fileRetriever.OpenFile());
            progressReader.ProgressChanged += (sender, e) => OnStreamProgressChanged?.Invoke(sender, e);

            using var csv = await CsvDataReader.CreateAsync(progressReader, options);

            var parsedValues = ParseReader(csv).AsDataReader();

            using var bcp = new SqlBulkCopy(sqlConnection)
            {
                BulkCopyTimeout = 0,
                DestinationTableName = tableName,
                NotifyAfter = 1000,
            };
            bcp.SqlRowsCopied += (sender, e) => OnSqlRowsCopied?.Invoke(sender, e);

            foreach (var columnMapping in columnMappings)
                bcp.ColumnMappings.Add(columnMapping);

            await bcp.WriteToServerAsync(parsedValues, token);
            OnStatusChanged?.Invoke(this, ImportStatus.Finished);
        }
    }

    public readonly struct FileRetriever
    {
        private static readonly DriveService storageService = new(new BaseClientService.Initializer()
        {
            ApplicationName = "RecipeLearning",
            ApiKey = "AIzaSyCLqs5-5TS0Hl2t4aifJiGfewhUr4vDtBY",
        });

        private readonly string googleDriveID, zipName, fileName;

        public FileRetriever(string googleDriveID, string zipName, string fileName)
        {
            this.googleDriveID = googleDriveID;
            this.zipName = Path.Combine(tempFolder, zipName);
            this.fileName = Path.Combine(tempFolder, fileName);
        }

        public StreamReader OpenFile() => File.OpenText(fileName);

        public bool DoesFileExist => File.Exists(fileName);

        public bool DoesZipFileExist => File.Exists(zipName);

        public async Task DownloadZip(Action<IDownloadProgress>? onDownloadProgress = null, CancellationToken token = default)
        {

            var getRequest = storageService.Files.Get(googleDriveID);
            using var fileStream = new FileStream(zipName, FileMode.Create, FileAccess.Write);

            if (onDownloadProgress is not null)
                getRequest.MediaDownloader.ProgressChanged += onDownloadProgress;

            await getRequest.DownloadAsync(fileStream, token);
        }

        public void Unzip()
        {
            using FileStream zipToOpen = new(zipName, FileMode.Open);
            using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Read);
            archive.ExtractToDirectory(tempFolder);
        }
    }
}
