namespace WF.Sample.Models
{
    public interface IPaging
    {
        int Count { get;}
        int PageNumber { get;}
        int PageSize { get;}
    }
}
