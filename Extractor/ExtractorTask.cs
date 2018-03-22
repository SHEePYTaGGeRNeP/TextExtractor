using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using LorenzoExtractor.Helpers;
using static LorenzoExtractor.Extractor;
using System.Windows;

namespace LorenzoExtractor
{
    class ExtractorTask
    {
        private InterlockedBool _isCancelRequested = false;

        private Thread _taskThread;

        public bool IsRunning { get; private set; }

        private FilesReaderTask _filesReaderTask;

        private CancellationTokenSource _cts;

        public void Stop()
        {
            this._isCancelRequested = true;
            if (this._filesReaderTask != null)
                if (this._filesReaderTask.IsRunning)
                    this._filesReaderTask.Stop();
            this._cts.Cancel();
            this._taskThread?.Join();
        }

        public void Start(string inputText, string seperators, string pattern, SearchParameters searchParameters, Action<IEnumerable<string>> callback)
        {
            if (this.IsRunning)
                this.Stop();
            this._cts = new CancellationTokenSource();
            Thread taskThread = new Thread(() =>
            {
                Console.WriteLine("started normal search thread");
                IEnumerable<string> seperated = Seperate(inputText, seperators);
                if (this._isCancelRequested)
                    callback(Enumerable.Empty<string>());
                IEnumerable<string> trimmed = Trim(seperated, searchParameters.TrimSetting);
                if (this._isCancelRequested)
                    callback(Enumerable.Empty<string>());
                Console.WriteLine("finished trimming");
                IEnumerable<string> output;
                switch (searchParameters.SearchType)
                {
                    case SearchType.Starts_With:
                        output = StartsWith(trimmed, pattern, searchParameters.StringComparison, this._cts.Token);
                        break;
                    case SearchType.Contains:
                        output = Contains(trimmed, pattern, searchParameters.StringComparison, this._cts.Token);
                        break;
                    case SearchType.Regex:
                        output = SearchRegex(trimmed, pattern, searchParameters.RegexOptions, this._cts.Token);
                        break;
                    default: throw new Exception("New SearchType");
                }
                Console.WriteLine("Finished searching");
                callback(output);
            })
            {
                Name = "Extractor Tasks Thread",
                IsBackground = true
            };

            taskThread.Start();
        }

        public void Start(string[] paths, string pattern, Extractor.SearchParameters searchParameters, Action<IEnumerable<string>> callback)
        {
            if (this.IsRunning)
                this.Stop();
            this._cts = new CancellationTokenSource();

            this._filesReaderTask = new FilesReaderTask();
            this._filesReaderTask.Start(paths);
            Thread taskThread = new Thread(() =>
            {
                Console.WriteLine("started file search thread");
                List<string> output = new List<string>();
                while (true)
                {
                    if (this._filesReaderTask.Input.IsCompleted || this._isCancelRequested)
                        break;
                    if (this._filesReaderTask.Input.TryTake(out IEnumerable<string> fileContents, 1000))
                        continue;
                    IEnumerable<string> trimmed = Trim(fileContents, searchParameters.TrimSetting);
                    if (this._isCancelRequested)
                        callback(Enumerable.Empty<string>());
                    Console.WriteLine("finished trimming");
                    switch (searchParameters.SearchType)
                    {
                        case SearchType.Starts_With:
                            output.AddRange(StartsWith(trimmed, pattern, searchParameters.StringComparison, this._cts.Token));
                            break;
                        case SearchType.Contains:
                            output.AddRange(Contains(trimmed, pattern, searchParameters.StringComparison, this._cts.Token));
                            break;
                        case SearchType.Regex:
                            output.AddRange(SearchRegex(trimmed, pattern, searchParameters.RegexOptions, this._cts.Token));
                            break;
                        default: throw new Exception("New SearchType");
                    }
                }
                Console.WriteLine("Finished searching");
                callback(output);
            })
            {
                Name = "Extractor Tasks Thread",
                IsBackground = true
            };
            taskThread.Start();
            callback(null);
        }

    }
}
