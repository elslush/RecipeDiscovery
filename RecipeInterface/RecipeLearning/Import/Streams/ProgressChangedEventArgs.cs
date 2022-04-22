namespace RecipeLearning.Import.Streams;

public delegate void ProgressChangedEventHandler(object? sender, ProgressChangedEventArgs e);

public class ProgressChangedEventArgs : EventArgs
{
    public ProgressChangedEventArgs(double progressPercentage) => ProgressPercentage = progressPercentage;

    public double ProgressPercentage { get; }
}
