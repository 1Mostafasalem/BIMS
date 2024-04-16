namespace BIMS.Application.Services.Areas
{
    public interface IAreaService
    {
        IEnumerable<Area> GetActiveAreasByGovernorateId(int id);
    }
}
