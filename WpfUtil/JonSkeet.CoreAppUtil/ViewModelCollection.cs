namespace JonSkeet.CoreAppUtil;

public static class ViewModelCollections
{
    public static ViewModelCollection<TViewModel, TModel> ToViewModelCollection<TViewModel, TModel>
        (this IList<TModel> models, Func<TModel, TViewModel> vmFactory)
        where TViewModel : ViewModelBase<TModel> =>
        new(models, vmFactory);
}

public abstract class ViewModelCollection<TViewModel>(IEnumerable<TViewModel> viewModels)
    : SelectableCollection<TViewModel>(viewModels) where TViewModel : ViewModelBase
{
    public abstract void UpdateModels();
}

public class ViewModelCollection<TViewModel, TModel>(IList<TModel> models, Func<TModel, TViewModel> vmFactory)
    : ViewModelCollection<TViewModel>(models.Select(vmFactory))
    where TViewModel : ViewModelBase<TModel>
{
    public override void UpdateModels()
    {
        models.Clear();
        foreach (var vm in this)
        {
            models.Add(vm.Model);
        }
    }
}
