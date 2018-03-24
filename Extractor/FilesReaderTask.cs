using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using LorenzoExtractor.Helpers;
using System.Collections.Concurrent;

namespace LorenzoExtractor
{
    class FilesReaderTask
    {
        private InterlockedBool _isCancelRequested = false;

        private Thread _readThread;

        public InterlockedBool IsRunning { get; private set; } = false;

        public BlockingCollection<IEnumerable<string>> Input { get; private set; } = new BlockingCollection<IEnumerable<string>>(3);

        public void Stop()
        {
            this._isCancelRequested = true;
            this._readThread?.Join();
            this.IsRunning = false;
        }

        public void Start(string[] paths)
        {
            if (this.IsRunning)
                this.Stop();
            this.IsRunning = true;
            this._readThread = new Thread(() => this.ReadFiles(paths))
            {
                Name = "Read files",
                IsBackground = true
            };
            this._readThread.Start();
        }

        public void ReadFiles(string[] paths)
        {
            foreach (string path in paths)
            {
                if (this._isCancelRequested)
                    break;
               this.Input.Add(File.ReadLines(path, Encoding.UTF8));
            }
            this.Input.CompleteAdding();
        }
    }
}
