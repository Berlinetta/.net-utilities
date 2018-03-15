namespace DALUtility.Data
{
    using System;
    using System.Collections.Generic;

    public class FilterCondition
    {
        public String PropertyName { get; set; }

        public FilterOperate FilterOperate { get; set; }

        public List<object> PropertyValues { get; set; }
    }
}