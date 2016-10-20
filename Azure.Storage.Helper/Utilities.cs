using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Euyuil.Azure.Storage.Helper
{
    public static class Utilities
    {
        public static TableContinuationToken ConvertPaginationTokenToTableContinuationToken(string paginationToken)
        {
            if (paginationToken == null) return null;

            var indexOfSlash = paginationToken.IndexOf('/');
            if (indexOfSlash < 0) throw new FormatException($"The format of the pagination token is invalid: {paginationToken}");

            return new TableContinuationToken
            {
                NextPartitionKey = paginationToken.Substring(0, indexOfSlash),
                NextRowKey = paginationToken.Substring(indexOfSlash + 1)
            };
        }

        public static string ConvertTableContinuationTokenToPaginationToken(TableContinuationToken tableContinuationToken)
        {
            return tableContinuationToken == null ? null :
                $"{tableContinuationToken.NextPartitionKey}/{tableContinuationToken.NextRowKey}";
        }
    }
}
