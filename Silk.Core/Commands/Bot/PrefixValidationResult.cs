namespace Silk.Core.Commands.Bot
{
    public class PrefixValidationResult
    {
        public bool Valid { get; set; }
        public string Reason { get; set; } = string.Empty;

        public void Deconstruct(out bool valid, out string reason)
        {
            valid = Valid;
            reason = Reason;
        }
    }
}