using System.Threading.Tasks;
using AnnoucementPlugin.Database;
using AnnoucementPlugin.Services;
using Microsoft.EntityFrameworkCore;
using YumeChan.PluginBase;

namespace AnnoucementPlugin
{
    public sealed class AnnouncementPlugin : Plugin
    {

        private readonly AnnouncementService _announcement;
        private readonly AnnouncementContext _database;
        public AnnouncementPlugin(AnnouncementService announcement, AnnouncementContext database)
        {
            _announcement = announcement;
            _database     = database;
        }
        public override string DisplayName => "Announcement Plugin";


        public override async Task LoadAsync()
        {
            await base.LoadAsync();
            await _database.Database.MigrateAsync();
            _announcement.Start();
        }
    }
}