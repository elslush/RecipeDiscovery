namespace RecipeControlPanel.Dialogs;

internal interface IDialog
{
    Task<IDialog?> Execute(CancellationToken token = default);
}
