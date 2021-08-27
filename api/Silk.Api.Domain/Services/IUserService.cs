using System;
using System.Threading.Tasks;
using Silk.Api.Data.Entities;

namespace Silk.Api.Domain.Services
{
	public interface IUserService
	{
		/*
		 * TODO: CHANGE THIS CHANGE THIS CHANGE THIS
		 * Entity -> DTO
		 */
		public Task<User> GetUserByKey(Guid key); 
	}
}