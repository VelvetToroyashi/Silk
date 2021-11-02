using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Economy.Core.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Silk.Economy.Core.Commands
{
	

	public class ProfileCommand : BaseCommandModule
	{
		private readonly HttpClient _client;

		private static readonly Font? _fontBig;
		private static readonly Font? _fontSmall;
		
		private const float 
			ProfileTextLeft = 320f,
			ProfileTextDown = 85f;

		private static readonly DrawingOptions _options = new()
		{
			TextOptions = new()
			{
				ApplyKerning = true,
			}
		};
		
		private static readonly DrawingOptions _smallOptions = new()
		{
			TextOptions = new()
			{
				ApplyKerning = true,
				WrapTextWidth = 700
			}
		};

		static ProfileCommand()
		{
			if (_fontBig is not null)
				return; 
			
			var fc = new FontCollection();
			fc.Install("./MomCakeFont.ttf");
			var font = fc.Families.First();

			_fontBig = font.CreateFont(60f);
			_fontSmall = font.CreateFont(35f);
		}
		
		public ProfileCommand(HttpClient client)
		{
			_client = client;
		}

		[Command]
		public async Task Profile(CommandContext ctx)
		{
			var str = await GenerateProfileImageAsync(ctx.Member);

			await ctx.RespondAsync(m => m.WithFile("image.png", str));
		}
		
		private async Task<Stream> GenerateProfileImageAsync(DiscordMember user)
		{
			var av = await Image.LoadAsync<Rgba32>(await _client.GetStreamAsync(user.GetAvatarUrl(ImageFormat.Png, 256)));
			av.Mutate(m => m.ApplyRoundedCorners(25.6f));
			
			var baseImage = new Image<Rgba32>(1000, 600, Rgba32.ParseHex("9188c2"));
			baseImage.Mutate(x => x.ApplyRoundedCorners(25f));
			baseImage.Mutate(m => m.DrawImage(av, new Point(50, 50), 1f));
			
			
			//TODO: Limit input to 170 characters for motto
			baseImage.Mutate(m => m
				.DrawText(_options, user.DisplayName, _fontBig, Color.White, new(ProfileTextLeft, ProfileTextDown))
				.DrawText(_smallOptions, "Motto: /* TODO: User Mottos */", _fontSmall, Color.White, new(ProfileTextLeft, ProfileTextDown + 75)));


			var ms = new MemoryStream();
			await baseImage.SaveAsPngAsync(ms);
			ms.Position = 0;
			return ms;
		}
		
	}
}