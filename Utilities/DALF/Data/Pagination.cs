namespace DAL.Fundamentals.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Pagination information
    /// </summary>
    public class Pagination
    {
        /// <summary>
        /// Page index
        /// </summary>
        public Int32 PageIndex { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public Int32 PageSize { get; set; }

        /// <summary>
        /// Sort name
        /// </summary>
        public String SortName { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// filters
        /// </summary>
        public List<FilterCondition> Filters { get; set; }

        /// <summary>
        /// Search conditions
        /// </summary>
        public List<SearchCondition> SearchConditions{ get; set; } 
        
        /// <summary>
        /// Advanced Query String
        /// </summary>
        public String AdvancedQuery { get; set; }

    }
}