namespace DALUtility.Data
{
    using System;

    public class SearchCondition
    {
        public String ColumnName { get; set; }

        public Int32 Order { get; set; }

        public Object Value1 { get; set; }

        public Object Value2 { get; set; }

        public FilterOperate FilterOperate { get; set; }

        public RelatedOperate RelatedOperate { get; set; }
    }
}