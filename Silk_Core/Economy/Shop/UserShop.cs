using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SilkBot.Commands.Economy.Shop
{
    public class UserShop 
    {
        public ulong        OwnerId   { get; init; }
        public DateTime     Created   { get; init; }
        public int          ItemsSold { get; set;  }
        public bool         IsPremium { get; set;  }
        public bool         IsPrivate { get; set;  }
        
        
    }
}