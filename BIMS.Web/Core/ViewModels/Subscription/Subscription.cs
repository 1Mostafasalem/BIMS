namespace BIMS.Web.Core.ViewModels.Subscription
{
	public class SubscriptionViewModel
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime CreatedOn { get; set; } = DateTime.Now;
		public string Status
		{
			get
			{
				return DateTime.Today > EndDate ? SubscriptionStatus.Expired : DateTime.Today < StartDate ? string.Empty : SubscriptionStatus.Active;
			}
		}
	}
}