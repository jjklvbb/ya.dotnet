namespace WebApiProject.Models
{
    public record PagedResult<T>(IEnumerable<T> Items, int CurrentPage, int TotalPages, int TotalItems);
}
