using System;
using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Data.Entities
{
    /// <summary>
    /// Represents an invocation of any of Silk's commands.
    /// </summary>
    public class CommandInvocationEntity
    {
        public long Id { get; set; }

        /// <summary>
        /// When the command was invoked.
        /// </summary>
        [Required]
        public DateTime InvocationTime { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// What command was invoked, without arguments.
        /// </summary>
        [Required]
        public string CommandName { get; init; }
    }
}