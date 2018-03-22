using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;
using LorenzoExtractor.Helpers;
using LorenzoExtractor;
using System.Threading.Tasks;

namespace Text_Extractor_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;

        private const string _START = "Start";
        private const string _CANCEL = "Cancel";


        // TODO: Cancelation (Maybe only remove check on cancel in New)
        // TODO: Directory / files


        public MainWindow()
        {
            InitializeComponent();
            this.Init();
        }

        private void Init()
        {
            foreach (string e in Enum.GetNames(typeof(Extractor.SearchType)))
                this.cbSearchType.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(StringComparison)))
                this.cbStringComparison.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(Extractor.TrimSetting)))
                this.cbTrim.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(RegexOptions)))
                this.cbRegexOptions.Items.Add(e);

            this.cbSearchType.SelectedIndex = (int)Extractor.SearchType.Starts_With;
            this.cbTrim.SelectedIndex = (int)Extractor.TrimSetting.TrimAll;
            this.cbStringComparison.SelectedIndex = (int)StringComparison.OrdinalIgnoreCase;
            this.cbRegexOptions.SelectedIndex = (int)RegexOptions.IgnoreCase;
            this.tbSeperators.Text = Extractor.NEW_LINE_COMBOBOX;
            this.btnStart.Content = _START;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.btnStart.Content as string == _CANCEL)
                {
                    if (this._cts != null)
                    {
                        this._cts.Cancel();
                    }
                    this.btnStart.Content = _START;
                    return;
                }
                if (this.tbIn.Text.Length == 0 || this.tbSearchPattern.Text.Length == 0)
                {
                    MessageBox.Show("Please fill in the input fields");
                    return;
                }
                if (this._cts != null)
                {
                    this._cts.Cancel();
                }
                this.pbProgress.Value = this.pbProgress.Minimum;
                this._cts = new CancellationTokenSource();
                this.tbOut.Clear();
                StringComparison stringComparison = (StringComparison)Enum.Parse(typeof(StringComparison),
                this.cbStringComparison.SelectedItem.ToString());
                string dropdownValue = this.cbSearchType.SelectedItem.ToString();
                Extractor.SearchType searchType = (Extractor.SearchType)Enum.Parse(typeof(Extractor.SearchType), dropdownValue);
                dropdownValue = this.cbTrim.SelectedItem.ToString();
                Extractor.TrimSetting trimSetting = (Extractor.TrimSetting)Enum.Parse(typeof(Extractor.TrimSetting), dropdownValue);
                dropdownValue = this.cbRegexOptions.SelectedItem.ToString();
                RegexOptions regexOptions = (RegexOptions)Enum.Parse(typeof(RegexOptions), dropdownValue);

                Extractor.SearchParameters searchParameters = new Extractor.SearchParameters(stringComparison,
                    searchType, trimSetting, regexOptions);

                this.btnStart.Content = _CANCEL;
                //await this.Old(searchParameters);
                this.New(searchParameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        private async Task Old(Extractor.SearchParameters searchParameters)
        {
            IEnumerable<string> seperated = Extractor.Seperate(this.tbIn.Text, this.tbSeperators.Text);
            string[] result = await Extractor.SearchAsync(seperated, this.tbSearchPattern.Text, searchParameters, this._cts.Token);
            this.tbOut.Text = String.Join(Environment.NewLine, result);
            this.btnStart.Content = _START;
            this.pbProgress.Value = this.pbProgress.Maximum;
            Console.WriteLine("Click done");
        }
        private void New(Extractor.SearchParameters searchParameters)
        {
            Extractor.StartSearch(this.tbIn.Text, this.tbSeperators.Text, this.tbSearchPattern.Text, searchParameters, this.OnFinished);
        }

        private void OnFinished(IEnumerable<string> output)
        {
            Action workAction = delegate
            {
                this.tbOut.Text += String.Join(Environment.NewLine, output);
                this.btnStart.Content = _START;
                this.pbProgress.Value = this.pbProgress.Maximum;
            };
            this.tbOut.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, workAction);
        }


        public class OutputBinding : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string _output;
            public string Output
            {
                get { return this._output; }
                set
                {
                    this._output = value;
                    this.OnPropertyChanged(nameof(this.Output));
                }
            }

            // Create the OnPropertyChanged method to raise the event
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
