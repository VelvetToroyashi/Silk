using System.Collections.Generic;
using System.Text;
using RoleMenuPlugin.Database;

namespace RoleMenuPlugin;

public static class FormatHelper
{
    public static string Format(IReadOnlyList<RoleMenuOptionModel> options)
    {
        var sb = new StringBuilder();

        sb.AppendLine("**Role Menu!**")
          .AppendLine("Use the button below to select your roles!")
          .AppendLine()
          .AppendLine("Available roles:");

        foreach (var role in options)
        {
            sb.Append($"<@&{role.RoleId}>");
            
            if (role.Description != null)
            {
                sb.Append($" - {role.Description}");
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
}