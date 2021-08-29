using UnityEngine;
using System;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using Umph.Core;

namespace Umph.Editor
{
    public class UmphComponentDropdown : AdvancedDropdown
    {
        public event Action<int> OnItemSelected;

        private UmphComponentMenu[] _items;

        public UmphComponentDropdown(UmphComponentMenu[] items, AdvancedDropdownState state) : base(state)
        {
            _items = items;

            minimumSize = new Vector2(100f, 200f);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item.id >= 0)
                OnItemSelected?.Invoke(item.id);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("");

            var subMenus = new Dictionary<string, AdvancedDropdownItem>();

            for (int i = 0; i < _items.Length; i++)
            {
                var displayData = _items[i];

                var nameParts = displayData.MenuPath.Split('/');
                AdvancedDropdownItem parent = root;
                for (int j = 0; j < nameParts.Length; j++)
                {
                    var subName = string.Join("/", nameParts, 0, j + 1);

                    if (!subMenus.TryGetValue(subName, out var subNode))
                    {
                        subNode = new AdvancedDropdownItem(nameParts[j]);
                        subMenus.Add(subName, subNode);
                        parent.AddChild(subNode);
                    }

                    parent = subNode;
                }

                var leafName = nameParts[nameParts.Length - 1];

                var item = subMenus[displayData.MenuPath];
                item.name = leafName;
                item.id = i;
            }

            return root;
        }
    }
}
