using LorenzoExtractor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;
using System.Windows.Documents;
using System.Linq;
using System.Text;

namespace Text_Extractor_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string _VERSION = "v0.1";
        private const string _START = "Start";
        private const string _CANCEL = "Cancel";

        public MainWindow()
        {
            this.InitializeComponent();
            this.Init();
        }


        private void Init()
        {
            Extractor.PROGRESS_MAX = this.pbProgress.Maximum;
            foreach (string e in Enum.GetNames(typeof(Extractor.SearchType)))
                this.cbSearchType.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(StringComparison)))
                this.cbStringComparison.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(Extractor.TrimSetting)))
                this.cbTrim.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(Extractor.SplitSettings)))
                this.cbSplit.Items.Add(e);
            foreach (string e in Enum.GetNames(typeof(RegexOptions)))
                this.cbRegexOptions.Items.Add(e);

            this.cbSearchType.SelectedIndex = (int)Extractor.SearchType.Regex;
            this.cbTrim.SelectedIndex = (int)Extractor.TrimSetting.TrimAll;
            this.cbSplit.SelectedIndex = (int)Extractor.SplitSettings.Regex;
            this.cbStringComparison.SelectedIndex = (int)StringComparison.OrdinalIgnoreCase;
            this.cbRegexOptions.SelectedIndex = (int)RegexOptions.IgnoreCase;
            this.tbSeparators.Text = Extractor.NEW_LINE_COMBOBOX;
            this.btnStart.Content = _START;

            ChangeTitle(false, false);
        }

        private void HandleRequestNavigate(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink)sender;
            var uri = link.NavigateUri.ToString();
            Process.Start(uri);
            e.Handled = true;
        }



        private void CbSearchType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {            
            Extractor.SearchType searchType = (Extractor.SearchType)Enum.Parse(typeof(Extractor.SearchType), 
                this.cbSearchType.SelectedItem.ToString());
            switch (searchType)
            {
                case Extractor.SearchType.StartsWith:     
                case Extractor.SearchType.Contains:
                    this.spRegex.Visibility = Visibility.Collapsed;
                    this.spStringCompare.Visibility = Visibility.Visible;
                    break;
                default:
                case Extractor.SearchType.Regex:
                    this.spRegex.Visibility = Visibility.Visible;
                    this.spStringCompare.Visibility = Visibility.Collapsed;
                    break;
                case Extractor.SearchType.Test:
                    this.spRegex.Visibility = Visibility.Collapsed;
                    this.spStringCompare.Visibility = Visibility.Visible;
                    break;   
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
                dropdownValue = this.cbSplit.SelectedItem.ToString();
                Extractor.SplitSettings splitSettings = (Extractor.SplitSettings)Enum.Parse(typeof(Extractor.SplitSettings), dropdownValue);

                Extractor.SearchParameters searchParameters = new Extractor.SearchParameters(stringComparison,
                    searchType, trimSetting, regexOptions, splitSettings);

                this.btnStart.Content = _CANCEL;
                string[] split = this.tbIn.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (System.IO.File.Exists(split[0]))
                {
                    ChangeTitle(true, true);
                    Extractor.StartSearchFiles(split, this.tbSearchPattern.Text, searchParameters, this.OnUpdate);
                }
                else
                {
                    ChangeTitle(true, false);
                    Extractor.StartSearch(this.tbIn.Text, this.tbSeparators.Text, this.tbSearchPattern.Text, searchParameters, this.OnUpdate);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void OnUpdate(IEnumerable<string> output, double percDone)
        {
            void WorkAction()
            {
                this.pbProgress.Value = percDone;
                if (output != null)
                    this.tbOut.Text += String.Join(Environment.NewLine, output);
                if (percDone == this.pbProgress.Maximum)
                    this.btnStart.Content = _START;
            }
            this.tbOut.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind, (Action)WorkAction);
        }

        private void ChangeTitle(bool searching, bool paths)
        {
            var assembly = typeof(MainWindow).Assembly;
            if (searching)
                this.Title = String.Format("{0} - {1} - extractor{2}, GUI{3}", assembly.GetName().Name,
                    paths ? "Searching files" : "Searching text", Extractor.VERSION, _VERSION);
            else
                this.Title = String.Format("{0} - extractor{1}, GUI{2}", assembly.GetName().Name,
                    Extractor.VERSION, _VERSION);
        }               
    }
}