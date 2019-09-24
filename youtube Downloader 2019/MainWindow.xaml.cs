using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace youtube_Downloader_2019
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FolderBrowserDialog FBD = new FolderBrowserDialog();
        public MainWindow()
        {
            InitializeComponent();
            Info.Visibility = Visibility.Collapsed;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadButton.IsEnabled = false;
            //afficher le folderbrowser lorsque on clique sur le boutton

            if (FBD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (tbURL.Text != "")
                {
                    if (FBD.SelectedPath != @"C:\")
                    {
                        if ((mp3check.IsChecked == true) && (vidcheck.IsChecked == false))
                        {
                            pbDownloadProgress.Visibility = Visibility.Visible;
                            tbInfoProgress.Visibility = Visibility.Visible;
                            tbInfoProgress.Text = "Patienté...";
                            await downloadmp3();

                            //  pbDownloadProgressmp3.Visibility = Visibility.Visible;
                            tbInfoProgress.Text = "Terminé...";
                        }
                        else if ((mp3check.IsChecked == false) && (vidcheck.IsChecked == true))

                        {
                            pbDownloadProgress.Visibility = Visibility.Visible;
                            tbInfoProgress.Visibility = Visibility.Visible;

                            tbInfoProgress.Text = "Patienté...";
                            await DownloadVideo();
                            pbDownloadProgress.Visibility = Visibility.Visible;
                            tbInfoProgress.Visibility = Visibility.Visible;

                            tbInfoProgress.Text = "Terminé...";
                        }
                        else if ((mp3check.IsChecked == true) && (vidcheck.IsChecked == true))
                        {
                            pbDownloadProgress.Visibility = Visibility.Visible;

                            tbInfoProgress.Visibility = Visibility.Visible;

                            tbInfoProgress.Text = "Patienté...";
                            await downloadmp3();
                            await DownloadVideo();
                            tbInfoProgress.Text = "Terminé...";
                        }

                        if (ExtensionList.SelectedValue != null)
                        {
                            /* DownloadButton.Background = Brushes.LightGreen;
                            DownloadButton.Content = "Video pobrano pomyślnie!";
                             pbDownloadProgress.Visibility = Visibility.Hidden;
                             tbInfoProgress.Visibility = Visibility.Hidden;*/
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("No file extension selected!");
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid path to save file!");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("You did not enter the URL path to the file!");
                }
            }

            DownloadButton.IsEnabled = true;

        }

        private async void LoadInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            Info.Visibility = Visibility.Collapsed;
            IEnumerable<string> listOfExtension = new List<string>();
            IEnumerable<string> listFormat = new List<string>();
            var client = new YoutubeClient();
            var id = "";

            try
            {
                id = YoutubeClient.ParseVideoId(tbURL.Text);
                var video = await client.GetVideoAsync(id.ToString());
                var videoForm = await client.GetVideoMediaStreamInfosAsync(id.ToString());


                pbDownloadProgress.Visibility = Visibility.Visible;

                tbInfoProgress.Visibility = Visibility.Visible;
                tbInfoProgress.Text = "Patienté!";


                var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id.ToString());
                listOfExtension = streamInfoSet.GetAll().Select(s => s.Container.GetFileExtension()).Distinct();


                ExtensionList.ItemsSource = listOfExtension;

                lblTitle.Text = video.Title;
                lblDescription.Text = video.Description;
                lblAuthor.Text = video.Author;
                lblDuration.Text = video.Duration.ToString();
                Info.Visibility = Visibility.Visible;

                DownloadButton.Background = Brushes.Transparent;

                pbDownloadProgress.Visibility = Visibility.Hidden;
                tbInfoProgress.Visibility = Visibility.Hidden;
            }
            catch (FormatException)
            {
                System.Windows.MessageBox.Show("ERROR.",
                    "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

        }

        private string GetFullPath(string path, string title, string ext)
        {
            string newTitle = "";
            for (int i = 0; i < title.Length; i++)
            {
                if (title[i].Equals("|") || title[i].Equals("[") || title[i].Equals("]") || title[i].Equals("~") ||
                    title[i].Equals("#") || title[i].Equals("&") || title[i].Equals("*") || title[i].Equals("{") ||
                    title[i].Equals("}") || title[i].Equals(":") || title[i].Equals("<") || title[i].Equals(">") ||
                    title[i].Equals("?") || title[i].Equals("%") || title[i].Equals("*") || title[i].Equals("\""))
                {
                    newTitle += '_';
                }
                else
                {
                    newTitle += title[i];
                }
            }

            return (path + @"\" + newTitle + "." + ext);
        }

        private string GetFullPath(string path, string title)
        {
            string newTitle = "";
            for (int i = 0; i < title.Length; i++)
            {
                if (title[i].Equals("|") || title[i].Equals("[") || title[i].Equals("]") || title[i].Equals("~") ||
                    title[i].Equals("#") || title[i].Equals("&") || title[i].Equals("*") || title[i].Equals("{") ||
                    title[i].Equals("}") || title[i].Equals(":") || title[i].Equals("<") || title[i].Equals(">") ||
                    title[i].Equals("?") || title[i].Equals("%") || title[i].Equals("*") || title[i].Equals("\""))
                {
                    newTitle += '_';
                }
                else
                {
                    newTitle += title[i];
                }
            }

            return (path + @"\" + newTitle + ".");
        }

        private async Task DownloadVideo()
        {
            //define the progress bar variable
            var progress = new Progress<double>(p =>
            {
                pbDownloadProgress.Value = (int)(p * 100);
                tbInfoProgress.Text = "% " + pbDownloadProgress.Value;
            });

            //client
            var client = new YoutubeClient();
            //parse youtube id
            var id = YoutubeClient.ParseVideoId(tbURL.Text);
            // Get video info
            var video = await client.GetVideoMediaStreamInfosAsync(id.ToString());

            var videoTitle = await client.GetVideoAsync(id.ToString());
            string ext = ExtensionList.SelectedValue.ToString();
            string myPath = GetFullPath(FBD.SelectedPath, videoTitle.Title, ext);
            var streamInfo = video.Muxed.WithHighestVideoQuality();
            string vidPath = GetFullPath(FBD.SelectedPath, RemoveIllegalPathCharacters(videoTitle.Title + ".mp4"));
            await client.DownloadMediaStreamAsync(streamInfo, vidPath, progress);
        }

        private string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) +
                                 new string(System.IO.Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }

        private async Task downloadmp3()
        {
            var progress = new Progress<double>(p =>
            {
                pbDownloadProgress.Value = (int)(p * 100);
                tbInfoProgress.Text = "% " + pbDownloadProgress.Value;
            });

            //client
            var client = new YoutubeClient();
            //parse youtube id
            var id = YoutubeClient.ParseVideoId(tbURL.Text);
            var videoTitle = await client.GetVideoAsync(id.ToString());

            var audio = await client.GetVideoMediaStreamInfosAsync(id.ToString());
            var streamInfo2 = audio.Audio.OrderByDescending(s => s.Bitrate).First();

            string audioPath = GetFullPath(FBD.SelectedPath, RemoveIllegalPathCharacters(videoTitle.Title + ".mp3"));
            await client.DownloadMediaStreamAsync(streamInfo2, audioPath, progress);
        }
    }
}
