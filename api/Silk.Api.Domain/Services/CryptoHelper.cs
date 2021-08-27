﻿using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Silk.Api.Domain.Services
{
	public sealed class CryptoHelper
	{
		public byte[] CreateSalt()
		{
			var buffer = new byte[42];
			using var rng = new RNGCryptoServiceProvider();
			rng.GetBytes(buffer, 0, 42);
			return buffer;
		}

		public byte[] GetRandomBytes(int amount)
		{
			using var crypto = new RNGCryptoServiceProvider();
			var buffer = new byte[amount];
			
			crypto.GetNonZeroBytes(buffer);

			return buffer;
		}
		
		public byte[] HashPassword(string password, byte[] salt)
		{
			var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

			argon2.Salt = salt;
			argon2.DegreeOfParallelism = 8; // four cores
			argon2.Iterations = 4;
			argon2.MemorySize = 16 * 1024; // 16MB //
			return argon2.GetBytes(42);
		}
		
		public bool Verify(string password, byte[] salt, byte[] hash)
		{
			var newHash = HashPassword(password, salt);
			return hash.SequenceEqual(newHash);
		}
		
	}
}