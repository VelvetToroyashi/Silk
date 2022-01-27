using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Commands.Parsers;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Results;
using Remora.Results;
using RoleMenuPlugin.Database;
using Silk.Extensions.Remora;
using Silk.Interactivity;

namespace RoleMenuPlugin
{
	/// <summary>
	/// The command module responsible for creating, modifying, and deleting role menus.
	/// </summary>
	[Group("rolemenu")]
	[Description("Role menu related commands.")]
	public sealed class RoleMenuCommand : CommandGroup
	{
		public class CreateCommand : CommandGroup
		{
			private readonly Regex _discordRegex = new (@"^\<(?<ANIMATED>a)?\:(?:[A-z0-9]+)\:(?<ID>\d+)\>$");
			private readonly Regex _unicodeRegex = new (@"\uD83C\uDFF4\uDB40\uDC67\uDB40\uDC62(?:\uDB40\uDC77\uDB40\uDC6C\uDB40\uDC73|\uDB40\uDC73\uDB40\uDC63\uDB40\uDC74|\uDB40\uDC65\uDB40\uDC6E\uDB40\uDC67)\uDB40\uDC7F|(?:\uD83E\uDDD1\uD83C\uDFFF\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D)?|\u200D(?:\uD83D\uDC8B\u200D)?)\uD83E\uDDD1|\uD83D\uDC69\uD83C\uDFFF\u200D\uD83E\uDD1D\u200D(?:\uD83D[\uDC68\uDC69])|\uD83E\uDEF1\uD83C\uDFFF\u200D\uD83E\uDEF2)(?:\uD83C[\uDFFB-\uDFFE])|(?:\uD83E\uDDD1\uD83C\uDFFE\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D)?|\u200D(?:\uD83D\uDC8B\u200D)?)\uD83E\uDDD1|\uD83D\uDC69\uD83C\uDFFE\u200D\uD83E\uDD1D\u200D(?:\uD83D[\uDC68\uDC69])|\uD83E\uDEF1\uD83C\uDFFE\u200D\uD83E\uDEF2)(?:\uD83C[\uDFFB-\uDFFD\uDFFF])|(?:\uD83E\uDDD1\uD83C\uDFFD\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D)?|\u200D(?:\uD83D\uDC8B\u200D)?)\uD83E\uDDD1|\uD83D\uDC69\uD83C\uDFFD\u200D\uD83E\uDD1D\u200D(?:\uD83D[\uDC68\uDC69])|\uD83E\uDEF1\uD83C\uDFFD\u200D\uD83E\uDEF2)(?:\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF])|(?:\uD83E\uDDD1\uD83C\uDFFC\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D)?|\u200D(?:\uD83D\uDC8B\u200D)?)\uD83E\uDDD1|\uD83D\uDC69\uD83C\uDFFC\u200D\uD83E\uDD1D\u200D(?:\uD83D[\uDC68\uDC69])|\uD83E\uDEF1\uD83C\uDFFC\u200D\uD83E\uDEF2)(?:\uD83C[\uDFFB\uDFFD-\uDFFF])|(?:\uD83E\uDDD1\uD83C\uDFFB\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D)?|\u200D(?:\uD83D\uDC8B\u200D)?)\uD83E\uDDD1|\uD83D\uDC69\uD83C\uDFFB\u200D\uD83E\uDD1D\u200D(?:\uD83D[\uDC68\uDC69])|\uD83E\uDEF1\uD83C\uDFFB\u200D\uD83E\uDEF2)(?:\uD83C[\uDFFC-\uDFFF])|\uD83D\uDC68(?:\uD83C\uDFFB(?:\u200D(?:\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF]))|\u200D(?:\uD83D\uDC8B\u200D\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF])))|\uD83E\uDD1D\u200D\uD83D\uDC68(?:\uD83C[\uDFFC-\uDFFF])|[\u2695\u2696\u2708]\uFE0F|[\u2695\u2696\u2708]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD]))?|(?:\uD83C[\uDFFC-\uDFFF])\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF]))|\u200D(?:\uD83D\uDC8B\u200D\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFF])))|\u200D(?:\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D)?|\u200D(?:\uD83D\uDC8B\u200D)?)\uD83D\uDC68|(?:\uD83D[\uDC68\uDC69])\u200D(?:\uD83D\uDC66\u200D\uD83D\uDC66|\uD83D\uDC67\u200D(?:\uD83D[\uDC66\uDC67]))|\uD83D\uDC66\u200D\uD83D\uDC66|\uD83D\uDC67\u200D(?:\uD83D[\uDC66\uDC67])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFF\u200D(?:\uD83E\uDD1D\u200D\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFE])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFE\u200D(?:\uD83E\uDD1D\u200D\uD83D\uDC68(?:\uD83C[\uDFFB-\uDFFD\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFD\u200D(?:\uD83E\uDD1D\u200D\uD83D\uDC68(?:\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFC\u200D(?:\uD83E\uDD1D\u200D\uD83D\uDC68(?:\uD83C[\uDFFB\uDFFD-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|(?:\uD83C\uDFFF\u200D[\u2695\u2696\u2708]|\uD83C\uDFFE\u200D[\u2695\u2696\u2708]|\uD83C\uDFFD\u200D[\u2695\u2696\u2708]|\uD83C\uDFFC\u200D[\u2695\u2696\u2708]|\u200D[\u2695\u2696\u2708])\uFE0F|\u200D(?:(?:\uD83D[\uDC68\uDC69])\u200D(?:\uD83D[\uDC66\uDC67])|\uD83D[\uDC66\uDC67])|\uD83C\uDFFF\u200D[\u2695\u2696\u2708]|\uD83C\uDFFE\u200D[\u2695\u2696\u2708]|\uD83C\uDFFD\u200D[\u2695\u2696\u2708]|\uD83C\uDFFC\u200D[\u2695\u2696\u2708]|\uD83C\uDFFF|\uD83C\uDFFE|\uD83C\uDFFD|\uD83C\uDFFC|\u200D[\u2695\u2696\u2708])?|(?:\uD83D\uDC69(?:\uD83C\uDFFB\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D(?:\uD83D[\uDC68\uDC69])|\uD83D[\uDC68\uDC69])|\u200D(?:\uD83D\uDC8B\u200D(?:\uD83D[\uDC68\uDC69])|\uD83D[\uDC68\uDC69]))|(?:\uD83C[\uDFFC-\uDFFF])\u200D\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D(?:\uD83D[\uDC68\uDC69])|\uD83D[\uDC68\uDC69])|\u200D(?:\uD83D\uDC8B\u200D(?:\uD83D[\uDC68\uDC69])|\uD83D[\uDC68\uDC69])))|\uD83E\uDDD1(?:\uD83C[\uDFFB-\uDFFF])\u200D\uD83E\uDD1D\u200D\uD83E\uDDD1)(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC69\u200D\uD83D\uDC69\u200D(?:\uD83D\uDC66\u200D\uD83D\uDC66|\uD83D\uDC67\u200D(?:\uD83D[\uDC66\uDC67]))|\uD83D\uDC69(?:\u200D(?:\u2764(?:\uFE0F\u200D(?:\uD83D\uDC8B\u200D(?:\uD83D[\uDC68\uDC69])|\uD83D[\uDC68\uDC69])|\u200D(?:\uD83D\uDC8B\u200D(?:\uD83D[\uDC68\uDC69])|\uD83D[\uDC68\uDC69]))|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFF\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFE\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFD\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFC\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFB\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD]))|\uD83E\uDDD1(?:\u200D(?:\uD83E\uDD1D\u200D\uD83E\uDDD1|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFF\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFE\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFD\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFC\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C\uDFFB\u200D(?:\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD]))|\uD83D\uDC69\u200D\uD83D\uDC66\u200D\uD83D\uDC66|\uD83D\uDC69\u200D\uD83D\uDC69\u200D(?:\uD83D[\uDC66\uDC67])|\uD83D\uDC69\u200D\uD83D\uDC67\u200D(?:\uD83D[\uDC66\uDC67])|(?:\uD83D\uDC41\uFE0F?\u200D\uD83D\uDDE8|\uD83E\uDDD1(?:\uD83C\uDFFF\u200D[\u2695\u2696\u2708]|\uD83C\uDFFE\u200D[\u2695\u2696\u2708]|\uD83C\uDFFD\u200D[\u2695\u2696\u2708]|\uD83C\uDFFC\u200D[\u2695\u2696\u2708]|\uD83C\uDFFB\u200D[\u2695\u2696\u2708]|\u200D[\u2695\u2696\u2708])|\uD83D\uDC69(?:\uD83C\uDFFF\u200D[\u2695\u2696\u2708]|\uD83C\uDFFE\u200D[\u2695\u2696\u2708]|\uD83C\uDFFD\u200D[\u2695\u2696\u2708]|\uD83C\uDFFC\u200D[\u2695\u2696\u2708]|\uD83C\uDFFB\u200D[\u2695\u2696\u2708]|\u200D[\u2695\u2696\u2708])|\uD83D\uDE36\u200D\uD83C\uDF2B|\uD83C\uDFF3\uFE0F?\u200D\u26A7|\uD83D\uDC3B\u200D\u2744|(?:(?:\uD83C[\uDFC3\uDFC4\uDFCA]|\uD83D[\uDC6E\uDC70\uDC71\uDC73\uDC77\uDC81\uDC82\uDC86\uDC87\uDE45-\uDE47\uDE4B\uDE4D\uDE4E\uDEA3\uDEB4-\uDEB6]|\uD83E[\uDD26\uDD35\uDD37-\uDD39\uDD3D\uDD3E\uDDB8\uDDB9\uDDCD-\uDDCF\uDDD4\uDDD6-\uDDDD])(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC6F|\uD83E[\uDDDE\uDDDF])\u200D[\u2640\u2642]|(?:\u26F9|\uD83C[\uDFCB\uDFCC]|\uD83D\uDD75)(?:(?:\uFE0F|\uD83C[\uDFFB-\uDFFF])\u200D[\u2640\u2642]|\u200D[\u2640\u2642])|\uD83C\uDFF4\u200D\u2620|(?:\uD83C[\uDFC3\uDFC4\uDFCA]|\uD83D[\uDC6E\uDC70\uDC71\uDC73\uDC77\uDC81\uDC82\uDC86\uDC87\uDE45-\uDE47\uDE4B\uDE4D\uDE4E\uDEA3\uDEB4-\uDEB6]|\uD83E[\uDD26\uDD35\uDD37-\uDD39\uDD3C-\uDD3E\uDDB8\uDDB9\uDDCD-\uDDCF\uDDD4\uDDD6-\uDDDD])\u200D[\u2640\u2642]|[\xA9\xAE\u203C\u2049\u2122\u2139\u2194-\u2199\u21A9\u21AA\u231A\u231B\u2328\u23CF\u23ED-\u23EF\u23F1\u23F2\u23F8-\u23FA\u24C2\u25AA\u25AB\u25B6\u25C0\u25FB\u25FC\u25FE\u2600-\u2604\u260E\u2611\u2614\u2615\u2618\u2620\u2622\u2623\u2626\u262A\u262E\u262F\u2638-\u263A\u2640\u2642\u2648-\u2653\u265F\u2660\u2663\u2665\u2666\u2668\u267B\u267E\u267F\u2692\u2694-\u2697\u2699\u269B\u269C\u26A0\u26A7\u26AA\u26B0\u26B1\u26BD\u26BE\u26C4\u26C8\u26CF\u26D1\u26D3\u26E9\u26F0-\u26F5\u26F7\u26F8\u26FA\u2702\u2708\u2709\u270F\u2712\u2714\u2716\u271D\u2721\u2733\u2734\u2744\u2747\u2763\u27A1\u2934\u2935\u2B05-\u2B07\u2B1B\u2B1C\u2B55\u3030\u303D\u3297\u3299]|\uD83C[\uDC04\uDD70\uDD71\uDD7E\uDD7F\uDE02\uDE37\uDF21\uDF24-\uDF2C\uDF36\uDF7D\uDF96\uDF97\uDF99-\uDF9B\uDF9E\uDF9F\uDFCD\uDFCE\uDFD4-\uDFDF\uDFF5\uDFF7]|\uD83D[\uDC3F\uDCFD\uDD49\uDD4A\uDD6F\uDD70\uDD73\uDD76-\uDD79\uDD87\uDD8A-\uDD8D\uDDA5\uDDA8\uDDB1\uDDB2\uDDBC\uDDC2-\uDDC4\uDDD1-\uDDD3\uDDDC-\uDDDE\uDDE1\uDDE3\uDDE8\uDDEF\uDDF3\uDDFA\uDECB\uDECD-\uDECF\uDEE0-\uDEE5\uDEE9\uDEF0\uDEF3])\uFE0F|\uD83D\uDC41\uFE0F?\u200D\uD83D\uDDE8|\uD83E\uDDD1(?:\uD83C\uDFFF\u200D[\u2695\u2696\u2708]|\uD83C\uDFFE\u200D[\u2695\u2696\u2708]|\uD83C\uDFFD\u200D[\u2695\u2696\u2708]|\uD83C\uDFFC\u200D[\u2695\u2696\u2708]|\uD83C\uDFFB\u200D[\u2695\u2696\u2708]|\u200D[\u2695\u2696\u2708])|\uD83D\uDC69(?:\uD83C\uDFFF\u200D[\u2695\u2696\u2708]|\uD83C\uDFFE\u200D[\u2695\u2696\u2708]|\uD83C\uDFFD\u200D[\u2695\u2696\u2708]|\uD83C\uDFFC\u200D[\u2695\u2696\u2708]|\uD83C\uDFFB\u200D[\u2695\u2696\u2708]|\u200D[\u2695\u2696\u2708])|\uD83C\uDFF3\uFE0F?\u200D\uD83C\uDF08|\uD83D\uDC69\u200D\uD83D\uDC67|\uD83D\uDC69\u200D\uD83D\uDC66|\uD83D\uDE36\u200D\uD83C\uDF2B|\uD83C\uDFF3\uFE0F?\u200D\u26A7|\uD83D\uDE35\u200D\uD83D\uDCAB|\uD83D\uDE2E\u200D\uD83D\uDCA8|\uD83D\uDC15\u200D\uD83E\uDDBA|\uD83E\uDEF1(?:\uD83C\uDFFF|\uD83C\uDFFE|\uD83C\uDFFD|\uD83C\uDFFC|\uD83C\uDFFB)?|\uD83E\uDDD1(?:\uD83C\uDFFF|\uD83C\uDFFE|\uD83C\uDFFD|\uD83C\uDFFC|\uD83C\uDFFB)?|\uD83D\uDC69(?:\uD83C\uDFFF|\uD83C\uDFFE|\uD83C\uDFFD|\uD83C\uDFFC|\uD83C\uDFFB)?|\uD83D\uDC3B\u200D\u2744|(?:(?:\uD83C[\uDFC3\uDFC4\uDFCA]|\uD83D[\uDC6E\uDC70\uDC71\uDC73\uDC77\uDC81\uDC82\uDC86\uDC87\uDE45-\uDE47\uDE4B\uDE4D\uDE4E\uDEA3\uDEB4-\uDEB6]|\uD83E[\uDD26\uDD35\uDD37-\uDD39\uDD3D\uDD3E\uDDB8\uDDB9\uDDCD-\uDDCF\uDDD4\uDDD6-\uDDDD])(?:\uD83C[\uDFFB-\uDFFF])|\uD83D\uDC6F|\uD83E[\uDDDE\uDDDF])\u200D[\u2640\u2642]|(?:\u26F9|\uD83C[\uDFCB\uDFCC]|\uD83D\uDD75)(?:(?:\uFE0F|\uD83C[\uDFFB-\uDFFF])\u200D[\u2640\u2642]|\u200D[\u2640\u2642])|\uD83C\uDFF4\u200D\u2620|\uD83C\uDDFD\uD83C\uDDF0|\uD83C\uDDF6\uD83C\uDDE6|\uD83C\uDDF4\uD83C\uDDF2|\uD83D\uDC08\u200D\u2B1B|\u2764(?:\uFE0F\u200D(?:\uD83D\uDD25|\uD83E\uDE79)|\u200D(?:\uD83D\uDD25|\uD83E\uDE79))|\uD83D\uDC41\uFE0F?|\uD83C\uDFF3\uFE0F?|(?:\uD83C[\uDFC3\uDFC4\uDFCA]|\uD83D[\uDC6E\uDC70\uDC71\uDC73\uDC77\uDC81\uDC82\uDC86\uDC87\uDE45-\uDE47\uDE4B\uDE4D\uDE4E\uDEA3\uDEB4-\uDEB6]|\uD83E[\uDD26\uDD35\uDD37-\uDD39\uDD3C-\uDD3E\uDDB8\uDDB9\uDDCD-\uDDCF\uDDD4\uDDD6-\uDDDD])\u200D[\u2640\u2642]|\uD83C\uDDFF(?:\uD83C[\uDDE6\uDDF2\uDDFC])|\uD83C\uDDFE(?:\uD83C[\uDDEA\uDDF9])|\uD83C\uDDFC(?:\uD83C[\uDDEB\uDDF8])|\uD83C\uDDFB(?:\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDEE\uDDF3\uDDFA])|\uD83C\uDDFA(?:\uD83C[\uDDE6\uDDEC\uDDF2\uDDF3\uDDF8\uDDFE\uDDFF])|\uD83C\uDDF9(?:\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDED\uDDEF-\uDDF4\uDDF7\uDDF9\uDDFB\uDDFC\uDDFF])|\uD83C\uDDF8(?:\uD83C[\uDDE6-\uDDEA\uDDEC-\uDDF4\uDDF7-\uDDF9\uDDFB\uDDFD-\uDDFF])|\uD83C\uDDF7(?:\uD83C[\uDDEA\uDDF4\uDDF8\uDDFA\uDDFC])|\uD83C\uDDF5(?:\uD83C[\uDDE6\uDDEA-\uDDED\uDDF0-\uDDF3\uDDF7-\uDDF9\uDDFC\uDDFE])|\uD83C\uDDF3(?:\uD83C[\uDDE6\uDDE8\uDDEA-\uDDEC\uDDEE\uDDF1\uDDF4\uDDF5\uDDF7\uDDFA\uDDFF])|\uD83C\uDDF2(?:\uD83C[\uDDE6\uDDE8-\uDDED\uDDF0-\uDDFF])|\uD83C\uDDF1(?:\uD83C[\uDDE6-\uDDE8\uDDEE\uDDF0\uDDF7-\uDDFB\uDDFE])|\uD83C\uDDF0(?:\uD83C[\uDDEA\uDDEC-\uDDEE\uDDF2\uDDF3\uDDF5\uDDF7\uDDFC\uDDFE\uDDFF])|\uD83C\uDDEF(?:\uD83C[\uDDEA\uDDF2\uDDF4\uDDF5])|\uD83C\uDDEE(?:\uD83C[\uDDE8-\uDDEA\uDDF1-\uDDF4\uDDF6-\uDDF9])|\uD83C\uDDED(?:\uD83C[\uDDF0\uDDF2\uDDF3\uDDF7\uDDF9\uDDFA])|\uD83C\uDDEC(?:\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEE\uDDF1-\uDDF3\uDDF5-\uDDFA\uDDFC\uDDFE])|\uD83C\uDDEB(?:\uD83C[\uDDEE-\uDDF0\uDDF2\uDDF4\uDDF7])|\uD83C\uDDEA(?:\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDED\uDDF7-\uDDFA])|\uD83C\uDDE9(?:\uD83C[\uDDEA\uDDEC\uDDEF\uDDF0\uDDF2\uDDF4\uDDFF])|\uD83C\uDDE8(?:\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDEE\uDDF0-\uDDF5\uDDF7\uDDFA-\uDDFF])|\uD83C\uDDE7(?:\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEF\uDDF1-\uDDF4\uDDF6-\uDDF9\uDDFB\uDDFC\uDDFE\uDDFF])|\uD83C\uDDE6(?:\uD83C[\uDDE8-\uDDEC\uDDEE\uDDF1\uDDF2\uDDF4\uDDF6-\uDDFA\uDDFC\uDDFD\uDDFF])|[#\*0-9]\uFE0F?\u20E3|\uD83E\uDD3C(?:\uD83C[\uDFFB-\uDFFF])|\u2764\uFE0F?|(?:\uD83C[\uDFC3\uDFC4\uDFCA]|\uD83D[\uDC6E\uDC70\uDC71\uDC73\uDC77\uDC81\uDC82\uDC86\uDC87\uDE45-\uDE47\uDE4B\uDE4D\uDE4E\uDEA3\uDEB4-\uDEB6]|\uD83E[\uDD26\uDD35\uDD37-\uDD39\uDD3D\uDD3E\uDDB8\uDDB9\uDDCD-\uDDCF\uDDD4\uDDD6-\uDDDD])(?:\uD83C[\uDFFB-\uDFFF])|(?:\u26F9|\uD83C[\uDFCB\uDFCC]|\uD83D\uDD75)(?:\uFE0F|\uD83C[\uDFFB-\uDFFF])?|\uD83C\uDFF4|(?:[\u270A\u270B]|\uD83C[\uDF85\uDFC2\uDFC7]|\uD83D[\uDC42\uDC43\uDC46-\uDC50\uDC66\uDC67\uDC6B-\uDC6D\uDC72\uDC74-\uDC76\uDC78\uDC7C\uDC83\uDC85\uDC8F\uDC91\uDCAA\uDD7A\uDD95\uDD96\uDE4C\uDE4F\uDEC0\uDECC]|\uD83E[\uDD0C\uDD0F\uDD18-\uDD1F\uDD30-\uDD34\uDD36\uDD77\uDDB5\uDDB6\uDDBB\uDDD2\uDDD3\uDDD5\uDEC3-\uDEC5\uDEF0\uDEF2-\uDEF6])(?:\uD83C[\uDFFB-\uDFFF])|(?:[\u261D\u270C\u270D]|\uD83D[\uDD74\uDD90])(?:\uFE0F|\uD83C[\uDFFB-\uDFFF])|[\u261D\u270A-\u270D]|\uD83C[\uDF85\uDFC2\uDFC7]|\uD83D[\uDC08\uDC15\uDC3B\uDC42\uDC43\uDC46-\uDC50\uDC66\uDC67\uDC6B-\uDC6D\uDC72\uDC74-\uDC76\uDC78\uDC7C\uDC83\uDC85\uDC8F\uDC91\uDCAA\uDD74\uDD7A\uDD90\uDD95\uDD96\uDE2E\uDE35\uDE36\uDE4C\uDE4F\uDEC0\uDECC]|\uD83E[\uDD0C\uDD0F\uDD18-\uDD1F\uDD30-\uDD34\uDD36\uDD3C\uDD77\uDDB5\uDDB6\uDDBB\uDDD2\uDDD3\uDDD5\uDEC3-\uDEC5\uDEF0\uDEF2-\uDEF6]|\uD83C[\uDFC3\uDFC4\uDFCA]|\uD83D[\uDC6E\uDC70\uDC71\uDC73\uDC77\uDC81\uDC82\uDC86\uDC87\uDE45-\uDE47\uDE4B\uDE4D\uDE4E\uDEA3\uDEB4-\uDEB6]|\uD83E[\uDD26\uDD35\uDD37-\uDD39\uDD3D\uDD3E\uDDB8\uDDB9\uDDCD-\uDDCF\uDDD4\uDDD6-\uDDDD]|\uD83D\uDC6F|\uD83E[\uDDDE\uDDDF]|[\xA9\xAE\u203C\u2049\u2122\u2139\u2194-\u2199\u21A9\u21AA\u231A\u231B\u2328\u23CF\u23ED-\u23EF\u23F1\u23F2\u23F8-\u23FA\u24C2\u25AA\u25AB\u25B6\u25C0\u25FB\u25FC\u25FE\u2600-\u2604\u260E\u2611\u2614\u2615\u2618\u2620\u2622\u2623\u2626\u262A\u262E\u262F\u2638-\u263A\u2640\u2642\u2648-\u2653\u265F\u2660\u2663\u2665\u2666\u2668\u267B\u267E\u267F\u2692\u2694-\u2697\u2699\u269B\u269C\u26A0\u26A7\u26AA\u26B0\u26B1\u26BD\u26BE\u26C4\u26C8\u26CF\u26D1\u26D3\u26E9\u26F0-\u26F5\u26F7\u26F8\u26FA\u2702\u2708\u2709\u270F\u2712\u2714\u2716\u271D\u2721\u2733\u2734\u2744\u2747\u2763\u27A1\u2934\u2935\u2B05-\u2B07\u2B1B\u2B1C\u2B55\u3030\u303D\u3297\u3299]|\uD83C[\uDC04\uDD70\uDD71\uDD7E\uDD7F\uDE02\uDE37\uDF21\uDF24-\uDF2C\uDF36\uDF7D\uDF96\uDF97\uDF99-\uDF9B\uDF9E\uDF9F\uDFCD\uDFCE\uDFD4-\uDFDF\uDFF5\uDFF7]|\uD83D[\uDC3F\uDCFD\uDD49\uDD4A\uDD6F\uDD70\uDD73\uDD76-\uDD79\uDD87\uDD8A-\uDD8D\uDDA5\uDDA8\uDDB1\uDDB2\uDDBC\uDDC2-\uDDC4\uDDD1-\uDDD3\uDDDC-\uDDDE\uDDE1\uDDE3\uDDE8\uDDEF\uDDF3\uDDFA\uDECB\uDECD-\uDECF\uDEE0-\uDEE5\uDEE9\uDEF0\uDEF3]|[\u23E9-\u23EC\u23F0\u23F3\u25FD\u2693\u26A1\u26AB\u26C5\u26CE\u26D4\u26EA\u26FD\u2705\u2728\u274C\u274E\u2753-\u2755\u2757\u2795-\u2797\u27B0\u27BF\u2B50]|\uD83C[\uDCCF\uDD8E\uDD91-\uDD9A\uDE01\uDE1A\uDE2F\uDE32-\uDE36\uDE38-\uDE3A\uDE50\uDE51\uDF00-\uDF20\uDF2D-\uDF35\uDF37-\uDF7C\uDF7E-\uDF84\uDF86-\uDF93\uDFA0-\uDFC1\uDFC5\uDFC6\uDFC8\uDFC9\uDFCF-\uDFD3\uDFE0-\uDFF0\uDFF8-\uDFFF]|\uD83D[\uDC00-\uDC07\uDC09-\uDC14\uDC16-\uDC3A\uDC3C-\uDC3E\uDC40\uDC44\uDC45\uDC51-\uDC65\uDC6A\uDC79-\uDC7B\uDC7D-\uDC80\uDC84\uDC88-\uDC8E\uDC90\uDC92-\uDCA9\uDCAB-\uDCFC\uDCFF-\uDD3D\uDD4B-\uDD4E\uDD50-\uDD67\uDDA4\uDDFB-\uDE2D\uDE2F-\uDE34\uDE37-\uDE44\uDE48-\uDE4A\uDE80-\uDEA2\uDEA4-\uDEB3\uDEB7-\uDEBF\uDEC1-\uDEC5\uDED0-\uDED2\uDED5-\uDED7\uDEDD-\uDEDF\uDEEB\uDEEC\uDEF4-\uDEFC\uDFE0-\uDFEB\uDFF0]|\uD83E[\uDD0D\uDD0E\uDD10-\uDD17\uDD20-\uDD25\uDD27-\uDD2F\uDD3A\uDD3F-\uDD45\uDD47-\uDD76\uDD78-\uDDB4\uDDB7\uDDBA\uDDBC-\uDDCC\uDDD0\uDDE0-\uDDFF\uDE70-\uDE74\uDE78-\uDE7C\uDE80-\uDE86\uDE90-\uDEAC\uDEB0-\uDEBA\uDEC0-\uDEC2\uDED0-\uDED9\uDEE0-\uDEE7]");

			private const int MessageReadDelay = 3200; // The time, in ms to wait before editing messasges.
			
			private readonly ButtonComponent _addMenuInteractiveButton = new(ButtonComponentStyle.Primary,     "Add (Interactive)", CustomID: "rm-add-interactive");
			private readonly ButtonComponent _addMenuSimpleButton      = new (ButtonComponentStyle.Secondary,  "Add (Simple)",      CustomID: "rm-add-role-only");
			private readonly ButtonComponent _addMenuEditButton        = new (ButtonComponentStyle.Secondary,  "Edit Option",       CustomID: "rm-edit-options", IsDisabled: true);
    
			private readonly ButtonComponent _addMenuHelpButton   = new(ButtonComponentStyle.Primary,     "Help",      CustomID: "rm-help");
			private readonly ButtonComponent _addMenuFinishButton = new(ButtonComponentStyle.Success,     "Finish",    CustomID: "rm-finish", IsDisabled: true);
			private readonly ButtonComponent _addMenuCancelButton = new(ButtonComponentStyle.Danger,      "Cancel",    CustomID: "rm-cancel");
			
			private readonly ButtonComponent _backButton = new(ButtonComponentStyle.Secondary, "Back", CustomID: "rm-back");
			private readonly ButtonComponent _exitButton = new(ButtonComponentStyle.Secondary, "Exit", CustomID: "rm-exit");
			
			
			private readonly MessageContext             _context;
			private readonly IDiscordRestUserAPI        _users;
			private readonly IDiscordRestChannelAPI     _channels;
			private readonly IDiscordRestGuildAPI       _guilds;
			private readonly InteractivityExtension     _interactivity;
			private readonly ILogger<RoleMenuCommand>   _logger;
			private readonly IDiscordRestInteractionAPI _interactions;

			private readonly List<RoleMenuOptionModel> _options = new(25);
			
			public CreateCommand
			(
				MessageContext             context,
				IDiscordRestUserAPI        users,
				IDiscordRestChannelAPI     channels,
				IDiscordRestGuildAPI       guilds,
				InteractivityExtension     interactivity,
				ILogger<RoleMenuCommand>   logger,
				IDiscordRestInteractionAPI interactions
			)
			{
				_context          = context;
				_users            = users;
				_channels         = channels;
				_guilds           = guilds;
				_interactivity    = interactivity;
				_logger           = logger;
				_interactions     = interactions;
			}

			[Command("create")]
			[RequireDiscordPermission(DiscordPermission.ManageChannels)]
			public async Task<IResult> CreateAsync
			(
				[Description("The channel the role menu will be created in.\n" +
				             "This channel must be a text channel, and must allow sending messages.")]
				IChannel? channel = null
			)
			{
				if (channel is null)
				{
					var currentChannelResult = await _channels.GetChannelAsync(_context.ChannelID);

					if (currentChannelResult.IsSuccess)
					{
						channel = currentChannelResult.Entity;
					}
					else
					{
						//_logger.LogError("User appears to be in an invalid channel: {UserID}, {ChannelID}", _context.User.ID _context.ChannelID);
						return currentChannelResult;
					}
				}

				var channelValidationResult = await EnsureChannelPermissionsAsync(channel);

				if (!channelValidationResult.IsSuccess)
				{
					if (channelValidationResult.Error is not PermissionDeniedError)
						return channelValidationResult;

					return await _channels.CreateMessageAsync(_context.ChannelID, "Sorry, but I can't send messages to that channel!");
				}

				var messageResult = await _channels.CreateMessageAsync
					(
					 _context.ChannelID, "Silk! RoleMenu Creator V3",
				     components: new IMessageComponent[]
				     {
					     new ActionRowComponent(new IMessageComponent[]
					     {
						     _addMenuInteractiveButton,
						     _addMenuSimpleButton,
						     _addMenuEditButton,
					     }),
					     new ActionRowComponent(new IMessageComponent[]
					     {
						     _addMenuHelpButton,
						     _addMenuFinishButton,
						     _addMenuCancelButton,
					     })
				     }
					);

				if (!messageResult.IsSuccess)
					return await InformUserOfChannelErrorAsync();

				return await MenuLoopAsync(messageResult.Entity);
			}

			private async Task<IResult> MenuLoopAsync(IMessage message)
			{
				while (true)
				{
					var selectionResult = await _interactivity.WaitForButtonAsync(_context.User, message, this.CancellationToken);
					
					if (!selectionResult.IsSuccess || !selectionResult.IsDefined(out var selection))
					{
						await _channels.DeleteMessageAsync(_context.ChannelID, _context.MessageID);
						await _channels.EditMessageAsync(_context.ChannelID, message.ID, "Cancelled!", components: Array.Empty<IMessageComponent>());
						return Result.FromSuccess(); // TODO: Return a proper error
					}
					
					// We set the timeout to 14 minutes to ensure we can still use the interaction to update our message.
					var cts   = new CancellationTokenSource(TimeSpan.FromMinutes(14));
					var token = cts.Token;
					
					//This is safe to do because the predicate ensures this information is present before returning a result.
					var t = selection.Data.Value.CustomID.Value switch
					{
						"rm-add-interactive" => await CreateInteractiveAsync(selection, token),
						"rm-simple"      => await CreateSimpleAsync(selection, token),
						//"rm-edit"        => await EditAsync(selection, token),
						"rm-help"		   => Result.FromSuccess(), // Ignored, handled in a handler.
						//"rm-finish"      => await FinishAsync(message, selection, token) 
						//"rm-cancel"      => await CancelAsync(message, selection, token)
						_ => Result.FromSuccess() // An exception should be thrown here, as it's outside what should be possible.
					};

					if (t is not Result<RoleMenuOptionModel> || !t.IsSuccess)
						return t;
					
					await ShowMainMenuAsync(selection, _options.Count);
				}
			}
			
			private async Task<IResult> CreateSimpleAsync(IInteractionCreate selection, CancellationToken ct)
			{
				
				var rolesResult = await _guilds.GetGuildRolesAsync(selection.GuildID.Value, ct);

				if (!rolesResult.IsSuccess)
					return rolesResult;
				
				Select:

				var select = new SelectMenuComponent
					(
					 "rm-select-role",
					 _options.Select((s, i) =>
					          {
						          var userFriendlyRoleName = rolesResult.Entity.FirstOrDefault(r => r.ID.Value == s.RoleId)?.Name;
					              
						          if (userFriendlyRoleName is null)
						          {
							          userFriendlyRoleName = "Unknown Role";
							          _logger.LogWarning("RoleMenu role has gone missing on {Guild}", selection.GuildID.Value);
						          }

						          return new SelectOption(userFriendlyRoleName, i.ToString());
					          })
					         .ToArray()
					);
				
				var optionInputResult = _interactivity.WaitForSelectAsync(selection.Member.Value.User.Value, selection.Message.Value, ct);
				var cancelInputResult = _interactivity.WaitForButtonAsync(selection.Member.Value.User.Value, selection.Message.Value, ct);

				await Task.WhenAny(optionInputResult, cancelInputResult);
				
				_logger.LogInformation("Input returned. Cancelled: {Cancelled} Index: {Index}",
				                       cancelInputResult.IsCompleted && cancelInputResult.Result.IsDefined(),
				                       optionInputResult.IsCompleted ? optionInputResult.Result.IsDefined(out var index) ? index : -1 : -1);
				
				
				return Result.FromSuccess();
			}

			private async Task<IResult> CreateInteractiveAsync(IInteraction interaction, CancellationToken ct)
			{
				const string DescriptionInputMessage = "What role would you like for the role menu?\n" +
				                                       "Type `cancel` to cancel. (Press the help button if you're stuck!)";
				
				await _interactions.CreateInteractionResponseAsync
					(
					 interaction.ID,
					 interaction.Token,
					 new InteractionResponse
						(
                         InteractionCallbackType.ChannelMessageWithSource,
                         new InteractionCallbackData
	                         (
	                          Content: DescriptionInputMessage, 
	                          Flags: InteractionCallbackDataFlags.Ephemeral
	                         ) 
					    ),
					 ct: ct
					 );

				var option = new RoleMenuOptionModel();
				
				// Parse role
				var roleResult = await GetRoleInputAsync(interaction, ct, option);

				if (roleResult is not Result<RoleMenuOptionModel> rmresult)
					return roleResult;
				
				option = rmresult.Entity;

				var emojiResult = await GetEmojiInputAsync(interaction, ct, option);
				
				if (emojiResult is not Result<RoleMenuOptionModel> emresult)
					return emojiResult;

				var descriptionResult = await GetDescriptionInputAsync(interaction, ct, option);
				
				if (descriptionResult is not Result<RoleMenuOptionModel> drresult)
					return descriptionResult;
				
				_options.Add(drresult.Entity);
				
				return drresult;
			}
			
			private async Task<IResult> GetEmojiInputAsync(IInteraction interaction, CancellationToken ct, RoleMenuOptionModel option)
			{
				async Task<IResult> EditResponseAsync(string content)
					=> await _interactions
					   .EditOriginalInteractionResponseAsync
							(
							 interaction.ApplicationID,
							 interaction.Token,
							 content,
							 ct: ct
							);

				var editResult = await EditResponseAsync("What emoji would you like to use? Type `cancel` to cancel and `skip` to skip.");

				while (true)
				{
					var emojiResult = await _interactivity.WaitForMessageAsync
						(
						 c => 
							 c.Author.ID == interaction.Member.Value.User.Value.ID &&
							 c.ChannelID == _context.ChannelID,
						 ct
						);

					if (!emojiResult.IsSuccess)
					{
						_logger.LogError("Failed to get emoji input on {Guild}, Error: {Error}", _context.GuildID.Value, emojiResult.Error.TryUnpack());
						
						var errorResult = await EditResponseAsync("Sorry, but something went wrong! These errors are tracked automatically.");
						
						if (!errorResult.IsSuccess)
							return editResult;
						
						await Task.Delay(5000, ct);

						return emojiResult;
					}

					if (emojiResult.Entity?.Content is null or "cancel")
					{
						await EditResponseAsync("Cancelled!");

						await Task.Delay(2000, ct);
						
						// We can abuse `Result.Success` here because we type check for Result<T>
						// and return the result if that check fails. Because we returning success, we don't trip any error checks.
						return Result.FromSuccess(); 
					}

					if (emojiResult.Entity.Content is "skip")
					{
						await EditResponseAsync("Skipped!");
						
						await Task.Delay(2000, ct);
						
						return Result<RoleMenuOptionModel>.FromSuccess(option);
					}
					
					string extracted;
					Match  match;

					if ((match = _discordRegex.Match(emojiResult.Entity.Content)).Success)
						extracted = match.Groups["ID"].Value;
					else if ((match = _unicodeRegex.Match(emojiResult.Entity.Content)).Success)
						extracted = match.Value;
					else
						continue;
					
					option = option with { EmojiName = extracted };
					
					return Result<RoleMenuOptionModel>.FromSuccess(option);
				}
			}

			private async Task<IResult> GetDescriptionInputAsync(IInteraction interaction, CancellationToken ct, RoleMenuOptionModel option)
			{
				async Task<IResult> EditResponseAsync(string content)
					=> await _interactions
					   .EditOriginalInteractionResponseAsync
							(
							 interaction.ApplicationID,
							 interaction.Token,
							 content,
							 ct: ct
							);
				
				await EditResponseAsync("What description would you like to use? Type `cancel` to cancel and `skip` to skip.");
				
				while (true)
				{
					var descriptionResult = await _interactivity.WaitForMessageAsync
						(
						 c => 
							 c.Author.ID == interaction.Member.Value.User.Value.ID &&
							 c.ChannelID == _context.ChannelID,
						 ct
						);
					
					if (!descriptionResult.IsSuccess)
					{
						_logger.LogError("Failed to get emoji input on {Guild}, Error: {Error}", _context.GuildID.Value, descriptionResult.Error!.TryUnpack());
						
						var errorResult = await EditResponseAsync("Sorry, but something went wrong! These errors are tracked automatically.");
						
						if (!errorResult.IsSuccess)
							return descriptionResult;
						
						await Task.Delay(5000, ct);

						return descriptionResult;
					}
					
					if (descriptionResult.Entity?.Content is null or "cancel")
					{
						await EditResponseAsync("Cancelled!");

						await Task.Delay(2000, ct);
					
						// We can abuse `Result.Success` here because we type check for Result<T>
						// and return the result if that check fails. Because we returning success, we don't trip any error checks.
						return Result.FromSuccess(); 
					}

					if (descriptionResult.Entity.Content is "skip")
					{
						return Result<RoleMenuOptionModel>.FromSuccess(option);
					}

					option = option with { Description = descriptionResult.Entity.Content };
					return Result<RoleMenuOptionModel>.FromSuccess(option);
				}
			}
			
			private async Task<IResult> GetRoleInputAsync(IInteraction interaction, CancellationToken ct, RoleMenuOptionModel option)
			{
				async Task<IResult> EditResponseAsync(string content)
					=> await _interactions
					   .EditOriginalInteractionResponseAsync
							(
							 interaction.ApplicationID,
							 interaction.Token,
							 content,
							 ct: ct
							);
				
				
				
				while (true)
				{
					await EditResponseAsync("What role would you like to add? Please mention the role directly! (e.g. @Super Cool Role)");
					
					var roleInput = await _interactivity.WaitForMessageAsync
						(message =>
							!string.IsNullOrEmpty(message.Content)  &&
							message.ChannelID == _context.ChannelID &&
							message.Author.ID == _context.User.ID   &&
							message.MentionedRoles.Any() || message.Content.Equals("cancel", StringComparison.Ordinal),
						 ct);

					if (!roleInput.IsSuccess)
						return roleInput;

					if (roleInput.Entity?.Content.ToLower() is null or "cancel")
					{
						var res = await EditResponseAsync("Cancelled!");

						await Task.Delay(2000, ct);
						return res;
					}

					var roleID = roleInput.Entity.MentionedRoles.First();	

					if (_options.Any(r => r.RoleId == roleID.Value))
					{
						var errorResult = await EditResponseAsync("Sorry, but that role is already in use!");
						
						if (errorResult.IsSuccess)
							return errorResult;

						await Task.Delay(2000, ct);
						continue;
					}

					var selfUser   = await _users.GetCurrentUserAsync(ct);
					var selfMember = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, selfUser.Entity.ID, ct);
					var guildRoles = await _guilds.GetGuildRolesAsync(_context.GuildID.Value, ct);

					var selfRoles = selfMember.Entity.Roles.Select(x => guildRoles.Entity.First(y => y.ID == x)).ToArray();
					var role      = guildRoles.Entity.First(x => x.ID == roleID);

					if (role.ID == _context.GuildID.Value)
					{
						var errorResult = await EditResponseAsync("Heh, everyone already has the everyone role!");

						if (!errorResult.IsSuccess)
							return errorResult;

						await Task.Delay(2000, ct);
						continue;
					}

					if (role.Position >= selfRoles.Max(x => x.Position))
					{
						var errorResult = await EditResponseAsync("Sorry, but that role is above my highest role, and I cannot assign it!");

						if (!errorResult.IsSuccess)
							return errorResult;

						await Task.Delay(2000, ct);
						continue;
					}

					option = option with { RoleId = role.ID.Value };

					return Result<RoleMenuOptionModel>.FromSuccess(option);
				}
			}

			private async Task<IResult> EnsureChannelPermissionsAsync(IChannel channel)
			{
				var selfResult = await _users.GetCurrentUserAsync();

				if (!selfResult.IsDefined(out var self))
					return selfResult;
				
				var selfMemberResult = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, self.ID);
				
				if (!selfMemberResult.IsDefined(out var member))
					return selfMemberResult;
				
				var rolesResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

				if (!rolesResult.IsDefined(out var roles))
					return rolesResult;

				var permissions = DiscordPermissionSet.ComputePermissions
					(
					 self.ID,
					 roles.First(r => r.ID == _context.GuildID.Value),
					 roles.Where(r => member.Roles.Contains(r.ID)).ToArray(),
					 channel.PermissionOverwrites.Value
					);

				if (!permissions.HasPermission(DiscordPermission.SendMessages))
					return Result.FromError(new PermissionDeniedError());
				
				return Result.FromSuccess();
			}

			private async Task<IResult> ShowMainMenuAsync(IInteraction interaction, int optionCount)
			{
				var addFullButtonWithState = _addMenuInteractiveButton	with { IsDisabled = optionCount >= 25 };
				var addButtonWithState     = _addMenuSimpleButton		with { IsDisabled = optionCount >= 25 };
				var editButtonWithState    = _addMenuEditButton			with { IsDisabled = optionCount <=  0 };
				var finishButtonWithState  = _addMenuFinishButton		with { IsDisabled = optionCount <=  0 };


				var result = await _interactions.EditOriginalInteractionResponseAsync
					(
					 interaction.ApplicationID,
					 interaction.Token,
					 "Silk! Role Menu Creator V3",
					 components: new IMessageComponent[]
					 {
						 new ActionRowComponent(new IMessageComponent[]
						 {
							 addFullButtonWithState,
							 addButtonWithState,
							 editButtonWithState,
						 }),
						 new ActionRowComponent(new IMessageComponent[]
						 {
							 _addMenuHelpButton,
							 finishButtonWithState,
							 _addMenuCancelButton,
						 })
					 });

				return result;
			}

			private async Task<IResult> InformUserOfChannelErrorAsync()
			{
				var channelResult = await _users.CreateDMAsync(_context.User.ID);

				if (!channelResult.IsDefined(out var DM))
					return channelResult;

				return await _channels.CreateMessageAsync(DM.ID, "Sorry, but I don't have permission to speak in the channel you ran the command in!");
			}
		}
	}
}