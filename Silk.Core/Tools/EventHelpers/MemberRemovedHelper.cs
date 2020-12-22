using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;
using Silk.Core.Database.Models;
using Silk.Core.Services;

namespace Silk.Core.Tools.EventHelpers
{
    public class MemberRemovedHelper
    {

        private readonly DatabaseService _dbService;

        public MemberRemovedHelper(DatabaseService dbService) => _dbService = dbService;
        
        /// <summary>
        /// Event handler responsible for removing user from the database if they leave a guild.
        /// However a member with a case history will not be removed from the database.
        /// </summary>
        
        public Task OnMemberRemoved(DiscordClient c, GuildMemberRemoveEventArgs e)
        {
            
            // This will be handled if a ban has been added, in which case we don't do anything //
            if (e.Handled || e.Member.IsBot) return Task.CompletedTask;
            e.Handled = true;
            _ = Task.Run(async () =>
            {
                UserModel? user = await _dbService.GetGuildUserAsync(e.Guild.Id, e.Member.Id);
                if (user is null) return; // Doesn't exist in the DB. No point in continuing. //
                if(user.Infractions.Any()) return; // They have infractions, and shouldn't be removed from the DB. //

                await _dbService.RemoveUserAsync(user);
                Log.Logger.Information($"{e.Member.Username} was removed from {e.Guild.Name}!");
            });
            return Task.CompletedTask;
        }
    }
}