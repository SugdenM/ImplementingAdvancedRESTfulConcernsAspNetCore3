using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using CourseLibrary.API.Services;

namespace CourseLibrary.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (mappingDictionary == null)
                throw new ArgumentNullException(nameof(mappingDictionary));

            if (string.IsNullOrWhiteSpace(orderBy))
                return source;

            // the orderBy string is separated by ",", so we split it
            var orderByAfterSplit = orderBy.Split(',');

            var orderByString = string.Empty;

            // apply each orderby clause in reverse order - otherwise, the
            // IQueryable will be ordered in the wrong order
            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                // trim the orderBy clause, as it might contain leading
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var
                var trimmedOrderByClause = orderByClause.Trim();
                
                // if the sort option ends with " desc", we order
                // descending otherwise ascending
                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                // remove " asc" or " desc" from the orderByClause, so we
                // get the property name to look for in the mapping dictionary.
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertName = indexOfFirstSpace == -1
                    ? trimmedOrderByClause
                    : trimmedOrderByClause.Remove(indexOfFirstSpace);

                // find the matching property
                if (!mappingDictionary.ContainsKey(propertName))
                    throw new ArgumentException($"Key mapping for {propertName} is missing");

                // get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertName];

                if (propertyMappingValue == null)
                    throw new ArgumentNullException("propertyMappingValue");

                // Run through the property names
                // so the orderBy clauses are applied in the correct order
                foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
                {
                    // revert sort order if necessary
                    if (propertyMappingValue.Revert)
                        orderDescending = !orderDescending;

                    orderByString = orderByString +
                                    (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                                    + destinationProperty
                                    + (orderDescending ? " descending" : " ascending");
                }

            }

            return source.OrderBy(orderByString);
        }
    }
}