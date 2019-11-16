using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tabula
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Dictionary<int, Vlak> index = new Dictionary<int, Vlak>();
        public int indexInt = 1;
        public string olditem;
        public Menu()
        {
            InitializeComponent();
        }

        private void stanice_Loaded(object sender, RoutedEventArgs e)
        {
            var output = OutputData.dostupneStanice.OrderBy(x => x.Key).ThenBy(x => x.Key).ToList();
            foreach (var item in output)
            {
                stanice.Items.Add(new ListBoxItem() {Content = item.Key, IsEnabled = false });
            }
        }

        private void vlak_loaded(object sender, RoutedEventArgs e)
        {
            vlak.SelectedIndex = 0;
            ukaz.IsEnabled = false;
        }

        private void stanice_LostFocus(object sender, RoutedEventArgs e)
        {
            if (stanice.SelectedIndex == 0)
            {
                vlak.Items.Clear();
                vlak.Items.Add(new ListBoxItem() { Content = "None", IsEnabled = false });
                vlak.SelectedIndex = 0;
                ukaz.IsEnabled = false;
            }
            else
            {
                if (olditem == (stanice.Items[stanice.SelectedIndex] as ListBoxItem).Content.ToString())
                    return;
                vlak.Items.Clear();
                vlak.Items.Add(new ListBoxItem() { Content = "None", IsEnabled = false});
                vlak.SelectedIndex = 0;
                index.Clear();
                indexInt = 0;
                vlak.IsEnabled = true;
                foreach (var vlaky in OutputData.poleVlaku)
                {
                    foreach (var zastavka in vlaky.jizda.trasa)
                    {
                        if (zastavka == (stanice.Items[stanice.SelectedIndex] as ListBoxItem).Content.ToString())
                        {
                            string time = (((int.Parse(vlaky.cas.Substring(0, 2)) * 60) + int.Parse(vlaky.cas.Substring(2, 2))) - ((DateTime.Now.Hour * 60) + DateTime.Now.Minute)).ToString();
                            vlak.Items.Add(new ListBoxItem() { IsEnabled = false, Content = vlaky.cislo + " | " + vlaky.cilVlaku + " | " + time + " MIN" });
                            index.Add(indexInt, vlaky);
                            indexInt++;
                            olditem = (stanice.Items[stanice.SelectedIndex] as ListBoxItem).Content.ToString();
                        }
                    }
                }
            }
        }
        MainWindow old;
        private void ukaz_Click(object sender, RoutedEventArgs e)
        {
            if (old == null)
                old = new MainWindow(index[vlak.SelectedIndex-1], (stanice.Items[stanice.SelectedIndex] as ListBoxItem).Content.ToString(), nastupiste.Text, kolej.Text, spozdeni.Text);
            else
            {
                old.Close();
                old = new MainWindow(index[vlak.SelectedIndex-1], (stanice.Items[stanice.SelectedIndex] as ListBoxItem).Content.ToString(), nastupiste.Text, kolej.Text, spozdeni.Text);
            }
        }

        private void vlak_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((vlak.Items[vlak.SelectedIndex] as ListBoxItem).Content.ToString() == "None")
            {
                ukaz.IsEnabled = false;
                kolej.IsEnabled = false;
                nastupiste.IsEnabled = false;
                spozdeni.IsEnabled = false;
            }
            else
            {
                ukaz.IsEnabled = true;
                kolej.IsEnabled = true;
                nastupiste.IsEnabled = true;
                spozdeni.IsEnabled = true;
            }
        }

        private void kolej_Loaded(object sender, RoutedEventArgs e)
        {
            kolej.Text = "1";
            kolej.IsEnabled = false;
        }

        private void nastupiste_Loaded(object sender, RoutedEventArgs e)
        {
            nastupiste.Text = "1";
            nastupiste.IsEnabled = false;
        }

        private void kolej_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(kolej.Text))
                kolej.Text = "1";
        }

        private void nastupiste_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nastupiste.Text))
                nastupiste.Text = "1";
        }

        private void spozdeni_Loaded(object sender, RoutedEventArgs e)
        {
            spozdeni.Text = "0";
            spozdeni.IsEnabled = false;
        }

        private void spozdeni_LostFocus(object sender, RoutedEventArgs e)
        {
            int asdf;
            if (!int.TryParse(spozdeni.Text, out asdf))
                spozdeni.Text = "0";
        }
    }
}
