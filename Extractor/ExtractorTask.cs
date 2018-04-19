using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LorenzoExtractor.Helpers;
using static LorenzoExtractor.Extractor;
using System.Windows.Forms;

namespace LorenzoExtractor
{
    class ExtractorTask
    {
        private InterlockedBool _isCancelRequested = false;

        private Thread _taskThread;

        public InterlockedBool IsRunning { get; private set; } = false;

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
            this.IsRunning = false;
        }

        public void Start(string inputText, string seperators, string pattern, SearchParameters searchParameters, Action<IEnumerable<string>, double> callback)
        {
            if (this.IsRunning)
                this.Stop();
            this.IsRunning = true;
            this._cts = new CancellationTokenSource();
            _taskThread = new Thread(() =>
            {
                try
                {
                    Console.WriteLine("started normal search thread");
                    callback(null, PROGRESS_MAX / 100);
                    IEnumerable<string> seperated = Split(inputText, seperators, searchParameters.SplitSettings);
                    if (this._isCancelRequested)
                        callback(null, PROGRESS_MAX);
                    else
                        callback(null, PROGRESS_MAX / 10);
                    IEnumerable<string> trimmed = Trim(seperated, searchParameters.TrimSetting);
                    if (this._isCancelRequested)
                        callback(null, PROGRESS_MAX);
                    else
                        callback(null, PROGRESS_MAX / 9);
                    IEnumerable<string> output;
                    switch (searchParameters.SearchType)
                    {
                        case SearchType.StartsWith:
                            output = StartsWith(trimmed, pattern, searchParameters.StringComparison, this._cts.Token);
                            break;
                        case SearchType.Contains:
                            output = Contains(trimmed, pattern, searchParameters.StringComparison, this._cts.Token);
                            break;
                        case SearchType.Regex:
                            output = SearchRegex(trimmed, pattern, searchParameters.RegexOptions, this._cts.Token);
                            break;
#if (DEBUG)
                        case SearchType.Test:
                            output = StartsWithTest(trimmed, pattern, searchParameters.StringComparison, this._cts.Token);
                            break;
#endif
                        default: throw new NotImplementedException("Start() " + searchParameters.SearchType);
                    }
                    Console.WriteLine("Finished searching");
                    callback(output, PROGRESS_MAX);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: \n" + ex.Message + "\n" + ex.StackTrace, "Unexpected exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    callback(null, PROGRESS_MAX);
                }
            })
            {
                Name = "Extractor Tasks Thread",
                IsBackground = true
            };

            _taskThread.Start();
        }

        public void Start(string[] paths, string pattern, Extractor.SearchParameters searchParameters, Action<IEnumerable<string>, double> callback)
        {
            if (this.IsRunning)
                this.Stop();
            this._filesReaderTask = new FilesReaderTask();
            this._filesReaderTask.Start(paths);
            _taskThread = new Thread(() =>
            {
                try
                {
                    Console.WriteLine("started file search thread");
                    callback(null, PROGRESS_MAX / 100);
                    List<string> output = new List<string>();
                    long nrOfReadBytes = 0;
                    double nextPerc = 0.1d; //we don't want to spam UI with progressbar updates.
                    while (true)
                    {
                        if ((this._filesReaderTask.Input.Count == 0 && this._filesReaderTask.Input.IsCompleted)
                                || this._isCancelRequested)
                            break;
                        if (!this._filesReaderTask.Input.TryTake(out string line, 1000))
                            continue;
                        nrOfReadBytes += line.Length * sizeof(char);

                        if (nrOfReadBytes / _filesReaderTask.TotalFileLengths >= nextPerc)
                        {
                            string[] copy = new string[output.Count];
                            output.CopyTo(copy, 0);
                            nextPerc += 0.1;
                            output.Clear();
                            callback(copy, nextPerc);
                        }
                        string trimmed = Trim(line, searchParameters.TrimSetting);
                        switch (searchParameters.SearchType)
                        {
                            case SearchType.StartsWith:
                                if (StartsWith(trimmed, pattern, searchParameters.StringComparison))
                                    output.Add(trimmed);
                                break;
                            case SearchType.Contains:
                                if (Contains(trimmed, pattern, searchParameters.StringComparison))
                                    output.Add(trimmed);
                                break;
                            case SearchType.Regex:
                                if (SearchRegex(trimmed, pattern, searchParameters.RegexOptions))
                                    output.Add(trimmed);
                                break;
#if (DEBUG)
                            case SearchType.Test:
                                if (StartsWithTest(trimmed, pattern, searchParameters.StringComparison))
                                    output.Add(trimmed);
                                break;
#endif
                            default: throw new NotImplementedException("Start() " + searchParameters.SearchType);
                        }
                    }
                    Console.WriteLine("Finished searching in files");
                    callback(output, PROGRESS_MAX);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: \n" + ex.Message + "\n" + ex.StackTrace, "Unexpected exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    callback(null, PROGRESS_MAX);
                }
            })
            {
                Name = "Extractor Tasks Thread",
                IsBackground = true
            };
            _taskThread.Start();
        }

    }
}
