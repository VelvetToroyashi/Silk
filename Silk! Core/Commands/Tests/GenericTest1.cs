using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkBot.Commands.Tests
{
    public class GenericTest1 : Item
    {
        public string Name { get; set; }
        public object SomeThing { get; set; }
        public int PlaceHolderInt { get; set; }
        public string SomeOtherProp { get; set; }
        public Dictionary<string, object> Props { get; }

        public Type DType => GetType();

        public void DeserializeProps()
        {
            var props = this.DType.GetProperties();
            for(int i = 0; i < Props.Count; i++)
            {
                if (!props[i].CanWrite) continue;
                    props.First(p => p.Name.ToLower().Contains(Props.Keys.ElementAt(i))).SetValue(this, Props.Values.ElementAt(i));
            }
        }
    }

    public interface Item 
    {
        public string Name { get; set; }
        public object SomeThing { get; set; }
        public int PlaceHolderInt { get; set; }
        public Dictionary<string, object> Props { get; }
        public Type DType { get; }
        public void DeserializeProps();
    }
}
