namespace WebApiProject.Models
{
    public record PagedResult<T>(IEnumerable<T> Items, int CurrentPage, int CurrentPageItems, int TotalItems);
}
