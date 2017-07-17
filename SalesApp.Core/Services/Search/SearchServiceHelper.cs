using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Services.Search
{
    class SearchServiceHelper<TResult> where TResult : ISearchResult
    {
        private ILog _logger;

        private ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                    _logger.Initialize(this.GetType().FullName);
                }
                return _logger;
            }
        }

        public async Task<List<TResult>> SearchAsync(string query, Func<string, Task<TResult[]>> searchFunction)
        {
            query = query.Trim();
            string[] queries = query.Split(' ');

            List<TResult> finalResult = new List<TResult>();
            string cummulativeQuery = string.Empty;
            foreach (string s in queries)
            {
                if (cummulativeQuery != string.Empty && !s.IsBlank())
                {
                    cummulativeQuery += " ";
                }

                cummulativeQuery += s;
                finalResult = await this.GetAppendedResultsAsync(finalResult, cummulativeQuery, searchFunction);
                if (s != cummulativeQuery)
                {
                    finalResult = await this.GetAppendedResultsAsync(finalResult, s, searchFunction);
                }
            }

            for (int i = 0; i < finalResult.Count; i++)
            {
                finalResult[i].Score = this.GetLevenshteinDistance(query, finalResult[i].DisplayText);
            }

            return finalResult.OrderBy(result => result.Score).ToList();
        }

        private async Task<List<TResult>> GetAppendedResultsAsync(List<TResult> currentResults, string query, Func<string, Task<TResult[]>> searchFunction)
        {
            TResult[] result = await DoSearchAsync(currentResults, query, searchFunction);
            if (result.Length == 0)
            {
                return currentResults;
            }
            currentResults.AddRange(result);
            return currentResults;
        }

        private async Task<TResult[]> DoSearchAsync(List<TResult> currentResults, string query, Func<string, Task<TResult[]>> searchFunction)
        {
            Logger.Debug("Querying ~".GetFormated(query));
            TResult[] result = await searchFunction(query);
            if (result == null)
            {
                result = new TResult[0];
            }

            if (currentResults.Count > 0 && result.Length > 0)
            {
                List<string> uniqueValuesInCurrentResults = currentResults.Select(item => item.UniqueValue).ToList();
                List<string> uniqueValuesInNewSearchResults = result.Select(item => item.UniqueValue).ToList();

                List<string> newUniques =
                    uniqueValuesInNewSearchResults.Where(val => !uniqueValuesInCurrentResults.Contains(val)).ToList();

                result = result.Where(item => newUniques.Contains(item.UniqueValue)).ToArray();
                return result;
            }
            else if (currentResults.Count == 0 && result.Length == 0)
            {
                return new TResult[0];
            }
            else if (currentResults.Count == 0)
            {
                return result;
            }
            else if (result.Length == 0)
            {
                return new TResult[0];
            }

            return new TResult[0];
        }

        public int GetLevenshteinDistance(string query, string displayText)
        {
            int queryLength = query.Length;
            int displayTextLength = displayText.Length;
            int[,] d = new int[queryLength + 1, displayTextLength + 1];

            // Step 1
            if (queryLength == 0)
            {
                return displayTextLength;
            }

            if (displayTextLength == 0)
            {
                return queryLength;
            }

            // Step 2
            for (int i = 0; i <= queryLength; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= displayTextLength; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= queryLength; i++)
            {
                //Step 4
                for (int j = 1; j <= displayTextLength; j++)
                {
                    // Step 5
                    int cost = (displayText[j - 1] == query[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[queryLength, displayTextLength];
        }

    }
}