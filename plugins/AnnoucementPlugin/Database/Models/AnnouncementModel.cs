using System.ComponentModel.DataAnnotations;

namespace AnnoucementPlugin.Database
{
	public sealed class AnnouncementModel
	{
		public int Id { get; set; }
		
		public Role AnnouncementsRole { get; set; }
		
		[MaxLength(4000)]
		public string AnnouncementMessage { get; set; }
	}
}