namespace Soap.Utility.Objects.Blended
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Delta
    {
        public static Result<T1> Calc<T, T1>(IEnumerable<T> newList, IEnumerable<T> existingList, Func<T, T1> selector)
        {
            
            var newListBySelector = newList.Select(selector).ToList();
            var existingListBySelector = existingList.Select(selector).ToList();

            var removed = new PrintableList<T1>();
            removed.AddRange(existingListBySelector.Except(newListBySelector).ToList());

            var added = new PrintableList<T1>();
            added.AddRange(newListBySelector.Except(existingListBySelector).ToList());

            existingListBySelector.RemoveAll(x => removed.Contains(x));
            var unchanged = new PrintableList<T1>();
            unchanged.AddRange(existingListBySelector.ToList());

            return new Result<T1>
            {
                Added = added,
                Removed = removed,
                Unchanged = unchanged
            };
        }

        public class PrintableList<T> : List<T>
        {
            public string ToEnumeratedString()
            {
                return this.Select(x => x.ToString()).Aggregate((x, y) => $"{x},{y}");
            }
        }

        public class Result<T1>
        {
            public PrintableList<T1> Added { get; internal set; }

            public PrintableList<T1> Removed { get; internal set; }

            public PrintableList<T1> Unchanged { get; internal set; }
        }
    }
}