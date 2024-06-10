
namespace Sardanapal.ViewModel.Models;

public class GridVM<T, S>
    where T : class
    where S : class
{
    public List<T> List { get; set; }
    public GridSearchModelVM<S> SearchModel { get; set; }

    public GridVM()
    {

    }

    public GridVM(GridSearchModelVM<S> searchModel)
    {
        SearchModel = searchModel;
    }
}