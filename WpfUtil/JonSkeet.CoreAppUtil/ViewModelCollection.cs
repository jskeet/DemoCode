namespace JonSkeet.CoreAppUtil;

public static class ViewModelCollections
{
    public static ViewModelCollection<TViewModel, TModel> ToViewModelCollection<TViewModel, TModel>
        (this IList<TModel> models, Func<TModel, TViewModel> vmFactory)
        where TViewModel : ViewModelBase<TModel> =>
        new(models, vmFactory);
}

public abstract class ViewModelCollection<TViewModel> : SelectableCollection<TViewModel> where TViewModel : ViewModelBase
{
    public ViewModelCollection(IEnumerable<TViewModel> viewModels) : base(viewModels)
    {
    }

    public abstract void UpdateModels();
}

public class ViewModelCollection<TViewModel, TModel> : ViewModelCollection<TViewModel>
    where TViewModel : ViewModelBase<TModel>
{
    private readonly IList<TModel> models;

    public ViewModelCollection(IList<TModel> models, Func<TModel, TViewModel> vmFactory)
        : base(models.Select(vmFactory))
    {
        this.models = models;
    }

    public override void UpdateModels()
    {
        models.Clear();
        foreach (var vm in this)
        {
            models.Add(vm.Model);
        }
    }
}
