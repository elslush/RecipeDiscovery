namespace RecipeLearning.Import.Streams;

public class ProgressTextReader : TextReader
{
    private readonly StreamReader streamReader;
    private double lastProgress = double.MinValue;

    public ProgressTextReader(StreamReader streamReader) => this.streamReader = streamReader;
    public ProgressTextReader(Stream stream)
    {
        if (!stream.CanRead || stream.Length <= 0)
            throw new ArgumentException(null, nameof(stream));

        streamReader = new StreamReader(stream);
    }

    public event ProgressChangedEventHandler? ProgressChanged;

    private void UpdateProgress()
    {
        if (ProgressChanged is not null)
        {
            var newProgress = (double)streamReader.BaseStream.Position / streamReader.BaseStream.Length;
            if (newProgress > lastProgress)
            {
                lastProgress = newProgress;
                ProgressChanged.Invoke(this, new ProgressChangedEventArgs(lastProgress));
            }
        }
    }

    public Stream BaseStream => streamReader.BaseStream;

    public override string? ReadLine()
    {
        var line = streamReader.ReadLine();
        UpdateProgress();
        return line;
    }

    public override int Peek() => streamReader.Peek();

    public override int Read()
    {
        var amountRead = streamReader.Read();
        UpdateProgress();
        return amountRead;
    }

    public override int Read(char[] buffer, int index, int count)
    {
        int amountRead = streamReader.Read(buffer, index, count);
        UpdateProgress();
        return amountRead;
    }

    public override int ReadBlock(char[] buffer, int index, int count)
    {
        var amountRead = streamReader.ReadBlock(buffer, index, count);
        UpdateProgress();
        return amountRead;
    }

    public override string ReadToEnd()
    {
        var amountRead = streamReader.ReadToEnd();
        UpdateProgress();
        return amountRead;
    }

    public override void Close()
    {
        streamReader.Close();
        base.Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            streamReader.Close();
        base.Dispose(disposing);
    }
}
