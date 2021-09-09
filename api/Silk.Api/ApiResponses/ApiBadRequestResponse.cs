using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Silk.Api.ApiResponses
{
	public class ApiBadRequestResponse : ApiResponseBase
	{
		[JsonProperty("errors")]
		public IEnumerable<string> Errors { get; }

		public ApiBadRequestResponse(ModelStateDictionary modelState) : base(400)
		{
			if (modelState.IsValid)
			{
				throw new ArgumentException("ModelState must be invalid", nameof(modelState));
			}

			Errors = modelState.SelectMany(x => x.Value!.Errors)
				.Select(x => x.ErrorMessage).ToArray();
		}

		public ApiBadRequestResponse(IEnumerable<string> errors) : base(400)
		{
			Errors = errors;
		}
	}
}