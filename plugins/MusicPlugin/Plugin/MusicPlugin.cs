using System.Threading.Tasks;
using YumeChan.PluginBase;

namespace MusicPlugin
{
	public class MusicPlugin : Plugin
	{
		public override string DisplayName { get; } = "Silk! Music Plugin";

		public override async Task LoadAsync()
		{
			await base.LoadAsync();
		}
	}
}