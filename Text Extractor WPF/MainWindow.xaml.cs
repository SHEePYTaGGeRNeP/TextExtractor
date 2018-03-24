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
        private const string _START = "Start";
        private const string _CANCEL = "Cancel";

        // TODO: Directory / files
        
        public MainWindow()
        {
            this.InitializeComponent();
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

            this.cbSearchType.SelectedIndex = (int)Extractor.SearchType.StartsWith;
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
                    Extractor.CancelSearch();
                    this.btnStart.Content = _START;
                    return;
                }
                if (this.tbIn.Text.Length == 0 || this.tbSearchPattern.Text.Length == 0)
                {
                    MessageBox.Show("Please fill in the input fields");
                    return;
                }
                this.pbProgress.Value = this.pbProgress.Minimum;
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
                Extractor.StartSearch(this.tbIn.Text, this.tbSeperators.Text, this.tbSearchPattern.Text, searchParameters, this.OnFinished);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void OnFinished(IEnumerable<string> output)
        {
            void WorkAction()
            {
                this.tbOut.Text += String.Join(Environment.NewLine, output);
                this.btnStart.Content = _START;
                this.pbProgress.Value = this.pbProgress.Maximum;
            }

            this.tbOut.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action) WorkAction);
        }


        public class OutputBinding : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string _output;
            public string Output
            {
                get => this._output;
                set
                {
                    this._output = value;
                    this.OnPropertyChanged(nameof(this.Output));
                }
            }

            // Create the OnPropertyChanged method to raise the event
            protected void OnPropertyChanged(string name)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
