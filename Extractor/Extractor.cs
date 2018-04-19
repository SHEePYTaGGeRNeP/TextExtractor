using System;
using System.Collections.Generic;
using Xunit;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace LorenzoExtractor
{
    public class Extractor
    {
        public const string VERSION = "v0.1";
        public const string NEW_LINE_COMBOBOX = "New Line";
        public static double PROGRESS_MAX = 100;

        public enum SearchType
        {
            StartsWith, Contains, Regex
#if (DEBUG)
            , Test
#endif
        }
        public enum TrimSetting { None, TrimAll, TrimBegin, TrimEnd }
        public enum SplitSettings { Regex, RegexCaseSensitive, NormalCaseSensitive }


        public class SearchParameters
        {
            public StringComparison StringComparison { get; set; }
            public SearchType SearchType { get; set; }
            public TrimSetting TrimSetting { get; set; }
            public RegexOptions RegexOptions { get; set; }
            public SplitSettings SplitSettings { get; set; }

            public SearchParameters()
            {
            }
            public SearchParameters(StringComparison stringComparison, SearchType searchType, TrimSetting trimSetting, RegexOptions regexOptions, SplitSettings splitSettings)
            {
                this.StringComparison = stringComparison;
                this.SearchType = searchType;
                this.TrimSetting = trimSetting;
                this.RegexOptions = regexOptions;
                this.SplitSettings = splitSettings;
            }
        }

        private static ExtractorTask _extractorTask;
        private static Action<IEnumerable<string>, double> _callback;

        public static void StartSearch(string inputText, string seperatorsText, string pattern, SearchParameters searchParameters, Action<IEnumerable<string>, double> callback)
        {
            Init();
            _callback = callback;
            _extractorTask.Start(inputText, seperatorsText, pattern, searchParameters, OnTaskFinished);
        }
        public static void StartSearchFiles(string[] paths, string pattern, SearchParameters searchParameters, Action<IEnumerable<string>, double> callback)
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

        private static void OnTaskFinished(IEnumerable<string> output, double progress)
        {
            _callback(output, progress);
        }

        public static IEnumerable<string> Split(string inputText, string seperatorsText, SplitSettings split)
        {
            Assert.NotNull(inputText);
            Assert.NotNull(seperatorsText);
            string[] seperators = seperatorsText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < seperators.Length; i++)
                if (seperators[i] == NEW_LINE_COMBOBOX)
                    seperators[i] = Environment.NewLine;
            IEnumerable<string> result;
            switch (split)
            {
                case SplitSettings.Regex:
                    result = Regex.Split(inputText, String.Join("|", seperators), RegexOptions.IgnoreCase).Where(x => !String.IsNullOrWhiteSpace(x));
                    break;
                case SplitSettings.RegexCaseSensitive:
                    result = Regex.Split(inputText, String.Join("|", seperators), RegexOptions.None).Where(x => !String.IsNullOrWhiteSpace(x));
                    break;
                case SplitSettings.NormalCaseSensitive:
                    result = inputText.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    break;
                default: throw new Exception("Seperate() " + split);
            }
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
        public static string Trim(string input, TrimSetting trimSetting)
        {
            switch (trimSetting)
            {
                case TrimSetting.TrimAll: return input.Trim();
                case TrimSetting.TrimBegin: return input.TrimStart();
                case TrimSetting.TrimEnd: return input.TrimEnd();
                case TrimSetting.None: return input;
                default: throw new NotImplementedException("Trim() " + trimSetting);
            }
        }

        public static IEnumerable<string> StartsWith(IEnumerable<string> input, string pattern, StringComparison stringComparison, CancellationToken cancelToken)
        {
            return input.TakeWhile(s => !cancelToken.IsCancellationRequested).Where(s => s.StartsWith(pattern, stringComparison));
        }
        public static bool StartsWith(string input, string pattern, StringComparison stringComparison)
        {
            return input.StartsWith(pattern, stringComparison);
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
        public static bool StartsWithTest(string input, string pattern, StringComparison stringComparison)
        {
            Thread.Sleep(100);
            return input.StartsWith(pattern, stringComparison);
        }
        public static string[] Contains(IEnumerable<string> input, string pattern, StringComparison stringComparison, CancellationToken cancelToken)
        {
            return input.TakeWhile(s => !cancelToken.IsCancellationRequested).Where(s => s.IndexOf(pattern, stringComparison) >= 0).ToArray();
        }
        public static bool Contains(string input, string pattern, StringComparison stringComparison)
        {
            return input.IndexOf(pattern, stringComparison) >= 0;
        }
        public static string[] SearchRegex(IEnumerable<string> input, string pattern, RegexOptions regexOptions, CancellationToken cancelToken)
        {
            return input.TakeWhile(s => !cancelToken.IsCancellationRequested).Where(s => Regex.IsMatch(s, pattern, regexOptions)).ToArray();
        }
        public static bool SearchRegex(string input, string pattern, RegexOptions regexOptions)
        {
            return Regex.IsMatch(input, pattern, regexOptions);
        }
    }
}
