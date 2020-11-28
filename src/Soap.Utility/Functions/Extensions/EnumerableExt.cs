namespace Soap.Utility.Functions.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExt
    {
        public static bool HasDuplicates(this List<string> e)
        {
            return e.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).Any();
        }
        
        public static List<string> GetDuplicatesOrNull(this List<string> e)
        {
            var enumerable = e.GroupBy(x => x).Where(g => g.Count() > 1);
            if (enumerable.Any()) return enumerable.Select(y => y.Key).ToList();
            else return null;
        }
    }
}
