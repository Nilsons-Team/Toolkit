using System.ComponentModel;

namespace Toolkit_Client.Modules
{
    public class InnerElementSearch
    {
        public interface ISearchElement
        {
            long Id { get; set; }
            string Name { get; set; }
        }

        public enum SearchElementType
        {
            NONE = 0,
            CATEGORY = 1,
            TAG = 2
        }

        public enum SearchFilterType : int {
            NONE                   = 1 << 0,
        
            HAS_DISCOUNT           = 1 << 1,
        
            PERPETUAL_PURCHASE     = 1 << 2,
            SUBSCRIPTION_PURCHASE  = 1 << 3,

            IS_SOFTWARE            = 1 << 4,
            IS_DEMO                = 1 << 5,
            IS_DLC                 = 1 << 6
        }

        public static string GetSortDirectionName(ListSortDirection direction) 
        {
            switch (direction) {
                case ListSortDirection.Ascending:   return "По возрастанию";
                case ListSortDirection.Descending:  return "По убыванию";
                default:                            return null;
            }
        }
    }
}
