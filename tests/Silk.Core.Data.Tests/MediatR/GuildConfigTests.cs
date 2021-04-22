using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Respawn;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.Tests.MediatR
{
    public class GuildConfigTests
    {
        private const ulong GuildId = 10;
        private const string ConnectionString = "Server=localhost; Port=5432; Database=unit_test; Username=silk; Password=silk; Include Error Detail=true;";
        private readonly Checkpoint _checkpoint = new() {TablesToIgnore = new[] {"__EFMigrationsHistory"}, DbAdapter = DbAdapter.Postgres};
        private readonly IServiceCollection _provider = new ServiceCollection();

        private GuildContext _context;

        private IMediator _mediator;

        [OneTimeSetUp]
        public async Task GlobalSetUp()
        {
            _provider.AddDbContext<GuildContext>(o => o.UseNpgsql(ConnectionString), ServiceLifetime.Transient);
            _provider.AddMediatR(typeof(GuildContext));
            _mediator = _provider.BuildServiceProvider().GetRequiredService<IMediator>();

            _context = _provider.BuildServiceProvider().GetRequiredService<GuildContext>();
            _context.Database.Migrate();
        }

        [OneTimeTearDown]
        public async Task Cleanup()
        {
            _context.Guilds.RemoveRange(_context.Guilds);
            await _context.SaveChangesAsync();
            await _context.DisposeAsync();
        }

        [SetUp]
        public async Task SetUp()
        {
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            await _checkpoint.Reset(connection);
        }

        [Test]
        public async Task MediatR_Get_When_Guild_Is_Null_Returns_Null()
        {
            //Arrange
            GuildConfig? result;
            //Act
            result = await _mediator.Send(new GetGuildConfigRequest(GuildId));
            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task MediatR_Get_When_Guild_Is_Not_Null_Does_Not_Return_Null()
        {
            //Arrange
            GuildConfig? result;
            await _mediator.Send(new GetOrCreateGuildRequest(GuildId, ""));
            //Act
            result = await _mediator.Send(new GetGuildConfigRequest(GuildId));
            //Assert
            Assert.IsNotNull(result, "Config is null!");
        }
    }
}