using System;

namespace Umph.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UmphComponentMenu : Attribute
    {
        public string MenuPath = null;
        public string Name = null;

        public UmphComponentMenu(string name)
        {
            Name = name;
        }

        public UmphComponentMenu(string name, string menuPath)
        {
            Name = name;
            MenuPath = menuPath;
        }
    }
}
