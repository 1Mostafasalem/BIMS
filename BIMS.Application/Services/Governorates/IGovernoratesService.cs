namespace BIMS.Application.Services.Governorates
{
    public interface IGovernoratesService
    {
        IEnumerable<Governorate> GetActiveGovernorates();
    }
}
