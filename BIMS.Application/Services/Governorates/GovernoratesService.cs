namespace BIMS.Application.Services.Governorates
{
    internal class GovernoratesService : IGovernoratesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GovernoratesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IEnumerable<Governorate> GetActiveGovernorates() => _unitOfWork.Governorates.FindAll(predicate: a => !a.IsDeleted, orderBy: a => a.Name, orderByDirection: OrderBy.Ascending);

    }
}
