using Google.Apis.Download;
using RecipeLearning.Import.Streams;
using System.Data.SqlClient;

namespace RecipeLearning.Import;

public interface IImporter
{
    public Task Import(CancellationToken token = default!);

    public event EventHandler<ImportStatus> OnStatusChanged;

    public event EventHandler<SqlRowsCopiedEventArgs> OnSqlRowsCopied;

    public event EventHandler<ProgressChangedEventArgs> OnStreamProgressChanged;

    public event EventHandler<IDownloadProgress> OnDownloadProgressChanged;
}
