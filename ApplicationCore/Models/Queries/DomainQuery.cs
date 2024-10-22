using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;

namespace AGL.Api.ApplicationCore.Models.Queries
{
    public class DomainQuery
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? SortKey { get; set; }
        [FromQuery(Name = "Filters[]")]
        public List<string> Filters { get; set; } = new List<string>();
        public System.ComponentModel.ListSortDirection SortDirection { get; set; }
        private List<QueryFilter> _QueryFilters { get; set; }
        public List<QueryFilter> QueryFilters
        {
            get
            {
                if (_QueryFilters == null)
                {
                    List<QueryFilter> queryFilters = new List<QueryFilter>();
                    foreach (var query in Filters)
                    {
                        if (query != null)
                        {
                            var queryString = query.Split(',');
                            queryFilters.Add(new QueryFilter
                            {
                                Field = queryString[0],
                                Value = queryString[1] ?? ""
                            });
                        }
                    }

                    _QueryFilters = queryFilters;
                }


                return _QueryFilters;
            }
            set
            {
                _QueryFilters = value;
            }
        }
        public string GetQueryString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"?{nameof(PageIndex)}={PageIndex}");
            stringBuilder.Append($"&{nameof(PageSize)}={PageSize}");
            stringBuilder.Append($"&{nameof(SortKey)}={SortKey}");
            stringBuilder.Append($"&{nameof(SortDirection)}={SortDirection}");
            foreach (var query in Filters)
            {
                stringBuilder.Append($"&{nameof(this.Filters)}[]={query}");
            }


            return stringBuilder.ToString();
        }
    }
    public class QueryFilter
    {
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
