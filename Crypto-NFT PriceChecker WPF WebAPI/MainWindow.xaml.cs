using Crypto_NFT_PriceChecker_WPF_WebAPI.CryptoDescription5m;
using Crypto_NFT_PriceChecker_WPF_WebAPI.JsonClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Crypto_NFT_PriceChecker_WPF_WebAPI
{
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient();
        DispatcherTimer dt = new DispatcherTimer();
        private string _coin;
        private string _coinName;
        private int _days;
        private bool _startUpdating = false;

        public MainWindow()
        {
            client.BaseAddress = new Uri("https://api.coinpaprika.com/v1/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            InitializeComponent();

            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 1);
            dt.Start();
            getListOfAllCoins();
        }

        // TIMER_WITH_GRID
        private void dt_Tick(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_coin) && _startUpdating)
            {
                listOfPrices.Items.Clear();
                fillDataGrid();
                dt.Interval = new TimeSpan(0, 5, 0);
            }
        }
        private async void fillDataGrid()
        {
            string response = await client.GetStringAsync("tickers/" + _coin + "/historical?start=" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"));
            List<History> history = JsonConvert.DeserializeObject<List<History>>(response);
            history.Reverse();


            for (int i = 0; i < history.Count; i++)
            {
                var data = new CryptoDescription
                {
                    name = _coinName,
                    symbol = _coin,
                    price = String.Format("${0:#,##0.##}", history[i].price),
                    marketCap = history[i].market_cap,
                    hVolume = history[i].volume_24h,
                    timestamp = history[i].timestamp,
                    hpercent = i != history.Count - 1 ? String.Format("{0:#,##0.##} %", (100 - 100 * history[i + 1].price / history[i].price)) : ""
                };
                if(i == 0) tbCryptoPrice.Text = data.price;
                listOfPrices.Items.Add(data);
            }
        }

        // GRAPH
        private async void getCoinChanges(DateTime start, DateTime end, string coin)
        {
            spGraph.Children.Clear();
            string response = await client.GetStringAsync("coins/" + coin + "/ohlcv/historical?start=" + start.ToString("yyyy-MM-dd") + "&end=" + end.ToString("yyyy-MM-dd"));
            List<Prices> prices = JsonConvert.DeserializeObject<List<Prices>>(response);

            if (response != "[]")
            {
                double maxValue = Math.Max(prices.Max(p => p.open), prices.Max(p => p.close));
                double minValue = Math.Min(prices.Min(p => p.open), prices.Min(p => p.close));

                tbGraph100.Text = String.Format("${0:#,##0.##}", maxValue);
                tbGraph50.Text = String.Format("${0:#,##0.##}", (maxValue - (maxValue - minValue) * 0.5));
                tbGraph0.Text = String.Format("${0:#,##0.##}", minValue);

                for (int i = 0; i < prices.Count; i++)
                {
                    StackPanel spDay = new StackPanel()
                    {
                        Tag = prices[i],
                        Width = spGraph.Width / (_days + 1),
                        Height = spGraph.Height,
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new SolidColorBrush(Colors.Black),
                    };
                    spDay.MouseEnter += new MouseEventHandler(spEnter);
                    spDay.MouseLeave += new MouseEventHandler(spLeave);
                    spGraph.Children.Add(spDay);

                    StackPanel sp = new StackPanel();
                    spDay.Children.Add(sp);

                    if(prices[i].close > prices[i].open)
                    {
                        double fromTop = spGraph.Height * (1 - 1/((maxValue - minValue)/(prices[i].close - minValue)));
                        double endHeight = spGraph.Height * (1 - 1 / ((maxValue - minValue) / (prices[i].open - minValue))) - fromTop;
                        sp.Height = endHeight;
                        sp.Margin = new Thickness(0, fromTop, 0, 0);
                        sp.Background = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        double fromTop = spGraph.Height * (1 - 1 / ((maxValue - minValue) / (prices[i].open - minValue)));
                        double endHeight = spGraph.Height * (1 - 1 / ((maxValue - minValue) / (prices[i].close - minValue))) - fromTop;
                        sp.Height = endHeight;
                        sp.Margin = new Thickness(0, fromTop, 0, 0);
                        sp.Background = new SolidColorBrush(Colors.Red);
                    }
                }
            }
        }

        // HOVER_OPTION
        private void spLeave(object sender, MouseEventArgs e)
        {
            (sender as StackPanel).Background = new SolidColorBrush(Colors.Black);
            tbCryptoInformationDay.Text = "Day: ";
            tbCryptoInformationOpen.Text = "Start: ";
            tbCryptoInformationHigh.Text = "Max: ";
            tbCryptoInformationLow.Text = "Min: ";
            tbCryptoInformationClose.Text = "End: ";
        }
        private void spEnter(object sender, MouseEventArgs e)
        {
            (sender as StackPanel).Background = new SolidColorBrush(Colors.LightGray);
            tbCryptoInformationDay.Text = "Day: " + ((sender as StackPanel).Tag as Prices).time_open.ToString("dd MMMM yyyy");
            tbCryptoInformationOpen.Text = "Start: " + String.Format("${0:#,##0.##}", ((sender as StackPanel).Tag as Prices).open);
            tbCryptoInformationHigh.Text = "Max: " + String.Format("${0:#,##0.##}", ((sender as StackPanel).Tag as Prices).high);
            tbCryptoInformationLow.Text = "Min: " + String.Format("${0:#,##0.##}", ((sender as StackPanel).Tag as Prices).low);
            tbCryptoInformationClose.Text = "End: " + String.Format("${0:#,##0.##}", ((sender as StackPanel).Tag as Prices).close);
        }

        // LIST_OF_ALL_COINS
        private async void getListOfAllCoins()
        {
            string response = await client.GetStringAsync("coins");
            List<Coins> coins = JsonConvert.DeserializeObject<List<Coins>>(response);

            for(int i = 0; i < coins.Count; i++)
            {
                if(coins[i].type != "token" && coins[i].is_active == true)
                {
                    StackPanel sp = new StackPanel()
                    {
                        Width = 600,
                        Height = 50,
                        Tag = coins[i],
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Background = new SolidColorBrush(Color.FromArgb(0xFF, 23, 23, 23)),
                        Margin = new Thickness(-40, 0, -20, 0),
                        Cursor = Cursors.Hand
                    };
                    spListOfCoins.Children.Add(sp);
                    sp.MouseLeftButtonDown += new MouseButtonEventHandler(coinsFromListClick);

                    TextBlock tb = new TextBlock()
                    {
                        Text = coins[i].name,
                        FontSize = 20,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(20, 10, 0, 0)
                    };
                    sp.Children.Add(tb);
                }
            }
        }
        private void coinsFromListClick(object sender, MouseButtonEventArgs e)
        {
            foreach(StackPanel sp in spListOfCoins.Children)
            {
                sp.Background = new SolidColorBrush(Color.FromArgb(0xFF, 23, 23, 23));
            }
            (sender as StackPanel).Background = new SolidColorBrush(Color.FromArgb(0xFF, 43, 43, 43));
            foreach (TextBlock tb in (sender as StackPanel).Children)
            {
                _coinName = tb.Text;
            }
            _coin = ((sender as StackPanel).Tag as Coins).id.ToString();
        }

        // EXIT BUTTON FUNCTIONS
        private void btnExit_MouseEnter(object sender, MouseEventArgs e)
        {
            btnExit.Background = Brushes.Red;
        }
        private void btnExit_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExit.Background = new SolidColorBrush(Color.FromArgb(0xFF, 29, 29, 29));
        }
        private void btnExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void dragFrameFunction(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // DAYS_COMBO_BOX_CHANGE
        private void cbDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)cbDays.SelectedItem;
            _days = Int32.Parse(typeItem.Content.ToString());
        }

        // SEARCH_BUTTON
        private void btnCryptoSearch_Click(object sender, RoutedEventArgs e)
        {
            DateTime end = DateTime.Now;
            DateTime start = end.AddDays(-_days);
            _startUpdating = true;
            dt.Interval = new TimeSpan(0, 0, 1);

            tbCryptoName.Text = _coinName;
            tbCryptoTag.Text = _coin;
            getCoinChanges(start, end, _coin);
        }

        // SEARCH_TEXT_CHANGE
        private void tbSearchCrypto_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach(StackPanel sp in spListOfCoins.Children)
            {
                
                if ((sp.Tag as Coins).name.ToString().ToLower().Contains(tbSearchCrypto.Text.ToLower())) sp.Visibility = Visibility.Visible;
                else sp.Visibility = Visibility.Collapsed;
            }
        }
    }
}
