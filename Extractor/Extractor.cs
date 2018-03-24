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

        public enum SearchType { StartsWith, Contains, Regex, Test }
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
            CancelSearch();
            _extractorTask = new ExtractorTask();
        }
        public static void CancelSearch()
        {
            if (_extractorTask == null) return;
            if (_extractorTask.IsRunning)
                _extractorTask.Stop();
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

        public static IEnumerable<string> StartsWith(IEnumerable<string> input, string pattern, StringComparison stringComparison, CancellationToken cancelToken)
        {
            return input.TakeWhile(s => !cancelToken.IsCancellationRequested).Where(s => s.StartsWith(pattern, stringComparison));
        }
        public static string[] StartsWithTest(IEnumerable<string> input, string pattern, StringComparison stringComparison, CancellationToken cancelToken)
        {
            List<string> output = new List<string>();
            foreach (string s in input)
            {
                Thread.Sleep(100);
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
            return input.TakeWhile(s => !cancelToken.IsCancellationRequested).Where(s => s.IndexOf(pattern, stringComparison) >= 0).ToArray();
        }
        public static string[] SearchRegex(IEnumerable<string> input, string pattern, RegexOptions regexOptions, CancellationToken cancelToken)
        {
            return input.TakeWhile(s => !cancelToken.IsCancellationRequested).Where(s => Regex.IsMatch(s, pattern, regexOptions)).ToArray();
        }
    }
}
