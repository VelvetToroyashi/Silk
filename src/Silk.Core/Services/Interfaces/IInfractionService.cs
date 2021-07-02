using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.Models;
using Silk.Core.Types;

namespace Silk.Core.Services.Interfaces
{
	public interface IInfractionService
	{
		public Task<InfractionResult> KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason);
		public Task<InfractionResult> BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null);
		public Task<InfractionResult> StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool autoEscalate = false);
		public ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId);
		public Task<InfractionResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration);
		public Task<InfractionStep> GetCurrentInfractionStepAsync(ulong guildId, IEnumerable<InfractionDTO> infractions);
		public Task<InfractionDTO> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, DateTime? expiration);
		public Task<InfractionResult> AddNoteAsync(ulong userId, ulong guildId, ulong noterId, string note);
		public Task<InfractionResult> UpdateNoteAsync(ulong userId, ulong guildId, ulong noterId, string newNote);

	}
}