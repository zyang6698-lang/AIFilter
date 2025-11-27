using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DeepSightAI
{
    /// <summary>
    /// 支持排序的BindingList，用于DataGridView列头点击排序
    /// </summary>
    /// <typeparam name="T">列表元素类型</typeparam>
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;

        public SortableBindingList() : base() { }

        public SortableBindingList(IList<T> list) : base(list) { }

        protected override bool SupportsSortingCore => true;

        protected override bool IsSortedCore => _isSorted;

        protected override ListSortDirection SortDirectionCore => _sortDirection;

        protected override PropertyDescriptor SortPropertyCore => _sortProperty;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;

            if (Items is List<T> list)
            {
                list.Sort((x, y) =>
                {
                    var xValue = prop.GetValue(x);
                    var yValue = prop.GetValue(y);

                    int result;
                    if (xValue == null && yValue == null)
                    {
                        result = 0;
                    }
                    else if (xValue == null)
                    {
                        result = -1;
                    }
                    else if (yValue == null)
                    {
                        result = 1;
                    }
                    else if (xValue is IComparable comparable)
                    {
                        result = comparable.CompareTo(yValue);
                    }
                    else
                    {
                        result = string.Compare(xValue.ToString(), yValue.ToString(), StringComparison.Ordinal);
                    }

                    return direction == ListSortDirection.Descending ? -result : result;
                });
            }

            _isSorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortProperty = null;
        }
    }
}

