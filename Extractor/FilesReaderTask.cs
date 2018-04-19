using System.Collections.Generic;
using System.Text;
using System.Threading;
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

        public BlockingCollection<string> Input { get; private set; } = new BlockingCollection<string>(10000);

        public long TotalFileLengths { get; private set; }

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
            TotalFileLengths = 0;
            foreach (string path in paths)
                using (FileStream fs = File.OpenRead(path))
                    TotalFileLengths += fs.Length;
            foreach (string path in paths)
            {
                if (this._isCancelRequested)
                    break;
                foreach (string line in File.ReadLines(path, Encoding.UTF8))
                {
                    this.Input.Add(line);    
                }
            }
            this.Input.CompleteAdding();
        }
    }
}
