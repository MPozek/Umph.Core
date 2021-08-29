
using UnityEditor;
using System.Linq;
using System;

namespace Umph.Editor
{
    public class SubtypeCache
    {
        public class SubtypesInfo
        {
            public Type[] subtypes;
        }

        private readonly Type _baseType;
        private readonly SubtypesInfo _subtypes;

        public int Count => _subtypes.subtypes.Length;

        public Type GetSubtypeAt(int index)
        {
            return _subtypes.subtypes[index];
        }

        public SubtypeCache(System.Type baseType)
        {
            _baseType = baseType;

            _subtypes = new SubtypesInfo();
            _subtypes.subtypes = TypeCache.GetTypesDerivedFrom(_baseType).Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) == false && t.IsAbstract == false).ToArray();
        }

        internal int GetTypeIndex(Type managedType)
        {
            return Array.IndexOf(_subtypes.subtypes, managedType);
        }
    }
}
