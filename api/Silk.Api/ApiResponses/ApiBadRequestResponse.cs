using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Silk.Api.ApiResponses
{
	public sealed class ApiBadRequestResponse 
	{
		[JsonProperty("errors")]
		public IEnumerable<string> Errors { get; }

		public ApiBadRequestResponse(ModelStateDictionary modelState)
		{
			if (modelState.IsValid)
			{
				throw new ArgumentException("ModelState must be invalid", nameof(modelState));
			}

			Errors = modelState.SelectMany(x => x.Value!.Errors)
				.Select(x => x.ErrorMessage).ToArray();
		}

		public ApiBadRequestResponse(IEnumerable<string> errors) 
		{
			Errors = errors;
			
		}
	}
}