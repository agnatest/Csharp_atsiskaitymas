using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Atsiskaitymas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
//-----!!!!!  
//    1. isvalyti search - po enter/ar search button -- isvalyti filmo parodyma
//    2. try catch poster img
    public partial class MainWindow : Window
    {
        public string _json { get; set;}

        public MainWindow()
        {
            InitializeComponent();
        }
     
        private string KviestiPirmaAPI(string ivestis)
        {
            string apiRaktas = "8eea6ef1";
            string bazinisUrl = $"http://www.omdbapi.com/?apikey={apiRaktas}";

            var sb = new StringBuilder(bazinisUrl);
            sb.Append($"&type=movie&s={ivestis}");

            var request = WebRequest.Create(sb.ToString());
            request.Timeout = 1000;
            request.Method = "GET";
            request.ContentType = "application/json";

            string result = string.Empty;

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException e)
            {
                title.Content = e.ToString();
            }
            catch (Exception e)
            {
                title.Content = e.ToString();
            }

            _json = result;
            return result;
        }

        private string KviestiAntraAPI(string id)
        {
            string apiRaktas = "8eea6ef1";
            string bazinisUrl = $"http://www.omdbapi.com/?apikey={apiRaktas}";

            var sb = new StringBuilder(bazinisUrl);
            sb.Append($"&i={id}");

            var request = WebRequest.Create(sb.ToString());
            request.Timeout = 1000;
            request.Method = "GET";
            request.ContentType = "application/json";

            string result = string.Empty;

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException e)
            {
                title.Content = e.ToString();
            }
            catch (Exception e)
            {
                title.Content = e.ToString();
            }

            return result;
        }

        private void ieskoti_btn_Click(object sender, RoutedEventArgs e)
        {
            string ivestis;
            ivestis = paieska.Text;
            parseJSON(KviestiPirmaAPI(ivestis));
            paieska.Text = String.Empty;
        }

        private void paieska_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string ivestis;
                ivestis = paieska.Text;
                parseJSON(KviestiPirmaAPI(ivestis));
                paieska.Text = String.Empty;
            }
        }

        private void parseJSON(string json)
        {
            patikrintiPaieskosRezultata(json);  
        }

        private void patikrintiPaieskosRezultata(string json)
        {

            var jsonObjektas = JsonConvert.DeserializeObject<PaieskosRezultatas>(json);
            var totalResult = jsonObjektas.totalResults;

            // tikrina kiek rezultatu grazina paieskos uzklausa
            if (Convert.ToInt32(totalResult) >= 2)
            {
                atvaizduotiPaieskosRezultatusListBoxe(json);
            }
            else if (Convert.ToInt32(totalResult) == 1)
            {
                var ids = jsonObjektas.Search.Select(x =>x.imdbID).ToList();
                foreach (string id in ids)
                {
                    atvaizduotiFilma(KviestiAntraAPI(id));
                }
            }
            else
            {
                title.Content = "KLAIDA! Nerasta jokių filmų.";
            }
        }

        private void atvaizduotiPaieskosRezultatusListBoxe(string json)
        {
            paieskosListBox.Items.Clear();

            var jsonObjektas = JsonConvert.DeserializeObject<PaieskosRezultatas>(json);
            var titles = jsonObjektas.Search.Select(pavadinimas => pavadinimas.Title).ToList();
         
            foreach (string title in titles)
            {
                paieskosListBox.Items.Add(title);
            }
        }

        private void paieskosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
            var jsonObjektas = JsonConvert.DeserializeObject<PaieskosRezultatas>(_json);
            var titles = jsonObjektas.Search.Select(pavadinimas => pavadinimas.Title).ToList();
            var ids = jsonObjektas.Search.Select(id => id.imdbID).ToList();
            var types = jsonObjektas.Search.Select(type => type.Type).ToList();

            Dictionary<string, string> dict = new Dictionary<string, string>();
           
            for (int i = 0; i < titles.Count; i++)
            {
                dict.Add(titles[i], ids[i]);
            }


            var pasirinkimas = paieskosListBox.SelectedItem.ToString();
            var gautiID  = dict[pasirinkimas].ToString();

            atvaizduotiFilma(KviestiAntraAPI(gautiID));

         //   paieskosListBox.UnselectAll();
        }

        private void atvaizduotiFilma(string json)
        {
            labels_block1.Visibility = Visibility.Visible;
            labels_block2.Visibility = Visibility.Visible;
            labels_block3.Visibility = Visibility.Visible;
            var jsonObjektas = JsonConvert.DeserializeObject<PasirinktasRezultatas>(json);
            title.Content = jsonObjektas.Title;
            year.Content = jsonObjektas.Year;
            runtime.Content = jsonObjektas.Runtime;
            genre.Content = jsonObjektas.Genre;
            director.Content = jsonObjektas.Director;
            writer.Text = jsonObjektas.Writer;
            actors.Text = jsonObjektas.Actors;
            plot.Text = jsonObjektas.Plot;
            released.Content = jsonObjektas.Released;
            language.Content = jsonObjektas.Language;
            country.Content = jsonObjektas.Country;
            imdbRating.Content = jsonObjektas.imdbRating;
            suformuotiIMDB_Url(jsonObjektas.imdbID);
            sukurtiPosterUrlIrAtvaizduoti(jsonObjektas.Poster);
        }

        private void suformuotiIMDB_Url(string imdbID)
        {
            string bazinisUrl = $"https://www.imdb.com/title/{imdbID}/";
            imdbUrl.NavigateUri = new Uri(bazinisUrl, UriKind.RelativeOrAbsolute);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void sukurtiPosterUrlIrAtvaizduoti(string poster_path)
        {
            var url = "";
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            url = $"{poster_path}";
            img.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();

            poster.Source = img;
        }
    }
}
