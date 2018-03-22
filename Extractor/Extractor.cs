using System;
using System.Collections.Generic;
using Xunit;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using LorenzoExtractor.Helpers;
using System.Collections.Concurrent;

namespace LorenzoExtractor
{
    public class Extractor
    {
        public const string NEW_LINE_COMBOBOX = "New Line";

        public enum SearchType { Starts_With, Contains, Regex }
        public enum TrimSetting { None, TrimAll, TrimBegin, TrimEnd }

        public class SearchParameters
        {
            public StringComparison StringComparison { get; set; }
            public SearchType SearchType { get; set; }
            public TrimSetting TrimSetting { get; set; }
            public RegexOptions RegexOptions { get; set; }

            public SearchParameters()
            {
            }
            public SearchParameters(StringComparison stringComparison, SearchType searchType, TrimSetting trimSetting, RegexOptions regexOptions)
            {
                this.StringComparison = stringComparison;
                this.SearchType = searchType;
                this.TrimSetting = trimSetting;
                this.RegexOptions = regexOptions;
            }
        }

        private static ExtractorTask _extractorTask;
        private static Action<IEnumerable<string>> _callback;


        public static void StartSearch(string inputText, string seperatorsText, string pattern, SearchParameters searchParameters, Action<IEnumerable<string>> callback)
        {
            Init();
            _callback = callback;
            _extractorTask.Start(inputText, seperatorsText, pattern, searchParameters, OnTaskFinished);
        }
        public static void StartSearchFiles(string[] paths, string pattern, SearchParameters searchParameters, Action<IEnumerable<string>> callback)
        {
            Init();
            _callback = callback;
            _extractorTask.Start(paths, pattern, searchParameters, OnTaskFinished);
        }
        private static void Init()
        {
            if (_extractorTask != null)
                if (_extractorTask.IsRunning)
                    _extractorTask.Stop();
            _extractorTask = new ExtractorTask();
        }

        private static void OnTaskFinished(IEnumerable<string> output)
        {
            _callback(output);
        }

        public static IEnumerable<string> Seperate(string inputText, string seperatorsText)
        {
            Assert.NotNull(inputText);
            Assert.NotNull(seperatorsText);
            string[] seperators = seperatorsText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < seperators.Length; i++)
                if (seperators[i] == NEW_LINE_COMBOBOX)
                    seperators[i] = Environment.NewLine;
            string[] result = inputText.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            return result;
        }

        public static async Task<string[]> SearchAsync(IEnumerable<string> input, string pattern, SearchParameters searchParameters, CancellationToken cancelToken)
        {
            Task<string[]> task = Task.Run(() =>
            {
                try
                {
                    IEnumerable<string> trimmed = Trim(input, searchParameters.TrimSetting);
                    switch (searchParameters.SearchType)
                    {
                        case SearchType.Starts_With:
                            return StartsWith(trimmed, pattern, searchParameters.StringComparison, cancelToken);
                        case SearchType.Contains:
                            return Contains(trimmed, pattern, searchParameters.StringComparison, cancelToken);
                        case SearchType.Regex:
                            return SearchRegex(trimmed, pattern, searchParameters.RegexOptions, cancelToken);
                        default: return null;
                    }
                }
                catch (OperationCanceledException oce)
                {
                    Console.WriteLine(oce is TaskCanceledException ? "Running Task was canceled." : "Scheduled Task was canceled");
                    return null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }, cancelToken);
            await Task.WhenAll(new[] { task });
            Console.WriteLine("Task completed");
            return task.Result;
        }
        public static IEnumerable<string> Trim(IEnumerable<string> input, TrimSetting trimSetting)
        {
            switch (trimSetting)
            {
                case TrimSetting.TrimAll: return input.Select(x => x.Trim()).ToArray();
                case TrimSetting.TrimBegin: return input.Select(x => x.TrimStart()).ToArray();
                case TrimSetting.TrimEnd: return input.Select(x => x.TrimEnd()).ToArray();
                case TrimSetting.None: return input.ToArray();
                default: throw new NotImplementedException("Trim() " + trimSetting);
            }
        }

        public static string[] StartsWith(IEnumerable<string> input, string pattern, StringComparison stringComparison, CancellationToken cancelToken)
        {
            List<string> output = new List<string>();
            foreach (string s in input)
            {
                if (cancelToken.IsCancellationRequested)
                    break;
                if (!s.StartsWith(pattern, stringComparison))
                    continue;
                output.Add(s);
            }
            return output.ToArray();
        }
        public static string[] Contains(IEnumerable<string> input, string pattern, StringComparison stringComparison, CancellationToken cancelToken)
        {
            List<string> output = new List<string>();
            foreach (string s in input)
            {
                if (cancelToken.IsCancellationRequested)
                    break;
                if (s.IndexOf(pattern, stringComparison) < 0)
                    continue;
                output.Add(s);
            }
            return output.ToArray();
        }
        public static string[] SearchRegex(IEnumerable<string> input, string pattern, RegexOptions regexOptions, CancellationToken cancelToken)
        {
            List<string> output = new List<string>();
            foreach (string s in input)
            {
                if (cancelToken.IsCancellationRequested)
                    break;
                if (!Regex.IsMatch(s, pattern, regexOptions))
                    continue;
                output.Add(s);
            }
            return output.ToArray();
        }
    }
}
