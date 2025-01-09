namespace WF.Sample.Models
{
    public interface IPaging
    {
        int Count { get; set; }
        int Page { get; set; }
        int PageSize { get; set; }
    }
}
