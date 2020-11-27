using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkBot.Commands.Economy.Shop
{
    public class UserShop 
    {
        public ulong OwnerId { get; set; }
        public DateTime IntialyOpened { get; init; }
        public int ItemsSold { get; set; }
    }
}