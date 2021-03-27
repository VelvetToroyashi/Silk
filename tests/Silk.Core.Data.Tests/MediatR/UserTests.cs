using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Silk.Core.Data.MediatR.Unified.Users;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.Tests.MediatR
{
    public class UserTests
    {
        private const ulong UserId = 1234567890;
        private const ulong GuildId = 0;
        private const string ConnectionString = "Server=localhost; Port=5432; Database=silk; Username=silk; Password=silk;";

        private IMediator _mediator;
        private readonly IServiceCollection _provider = new ServiceCollection();
        private GuildContext _context;

        [OneTimeSetUp]
        public void SetUp()
        {
            _provider.AddMediatR(typeof(GuildContext));
            _mediator = _provider.BuildServiceProvider().GetRequiredService<IMediator>();

            _context = new(new DbContextOptionsBuilder<GuildContext>().UseNpgsql(ConnectionString).Options);
            _context.Database.EnsureCreated();

            _context.Guilds.Add(new() {Id = GuildId});
            _context.SaveChanges();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task MediatR_User_Add_Adds_Properly()
        {
            // Arrange
            User expected = new()
            {
                Id = UserId,
                GuildId = GuildId,
                DatabaseId = 0,
            };

            User? result;

            //Act
            await _mediator.Send(new AddUserRequest(GuildId, UserId));
            result = _context.Users.FirstOrDefault(u => u.Id == UserId && u.GuildId == GuildId);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }
    }
}