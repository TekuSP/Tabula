using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
using Tabula.Audio;
using Tabula.Enums;

namespace Tabula.Windows.Screen
{
    public partial class MainWindow : Window
    {
        Vlak zjisteni;
        NadrazniTabule tabule;
        public MainWindow()
        {
            InitializeComponent();
            #region slozkoBordel
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/");
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/test.vlk"))
            {
                WebClient wc = new WebClient();
                try
                {
                    wc.DownloadFile(new Uri("https://www.dropbox.com/s/xbfv6vt9mnk1wuq/test.vlk?dl=1"), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/test.vlk");
                }
                catch (Exception)
                {
                    MessageBox.Show("Tato aplikace nemůže fungovat bez připojení k síti!", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(Environment.ExitCode);
                }
            }
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/databanka/"))
            {
                new WebClient().DownloadFile("https://www.dropbox.com/s/0fb1je34zfra7bb/databanka.zip?dl=1", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + " /.tabula/databanka.zip");
                ZipFile.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + " /.tabula/databanka.zip", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + " /.tabula/", Encoding.GetEncoding(852)); //852 DOS
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + " /.tabula/databanka.zip");
            }
            #endregion
            //zjisteni = SortAndTellClosestToTime(nacist());
            new Configurator.Menu().Show();
            //tabule = new NadrazniTabule(typ, pres, cil, hodiny, odjezd, spozdeni, cislo, svitidlo, zjisteni);
            //new System.Threading.Thread(asyncZjistovani).Start();
            this.Close();
        }
        public MainWindow(Vlak input, string aktualniZastavka, string nastupiste, string kolej, string zpozden)
        {
            InitializeComponent();
            for (int i = 0; i < input.jizda.trasa.Length-1; i++)
            {
                if (input.jizda.trasa[i] == aktualniZastavka)
                    input.jizda.aktualniZastavka = i;
                input.jizda.aktualniStanice = aktualniZastavka;
            }
            input.cas = input.jizda.casy[input.AktualniPoloha()].casOdjezdu;
            input.kolejVlaku = kolej;
            input.nastupisteVlaku = nastupiste;
            input.zpozdeni = int.Parse(zpozden);
            tabule = new NadrazniTabule(typ, pres, cil, hodiny, odjezd, spozdeni, cislo, svitidlo, input);
            
            this.Show();
            //new System.Threading.Thread(asyncZjistovani).Start();
        }
        Vlak SortAndTellClosestToTime(Vlak[] vstup, int timeasIndex = 0)
        {
            // temos was here
            Vlak[] neco = ((from train in vstup where (Convert.ToInt32(train.cas.Substring(0, 2)) * 60 + Convert.ToInt32(train.cas.Substring(2, 2)) + train.zpozdeni) > DateTime.Now.Hour * 60 + DateTime.Now.Minute orderby Convert.ToInt32(train.cas.Substring(0, 2)) * 60 + Convert.ToInt32(train.cas.Substring(2, 2)) + train.zpozdeni select train)).ToArray();
            if (neco.Length == 0)
                return (from train in vstup orderby Convert.ToInt32(train.cas.Substring(0, 2)) * 60 + Convert.ToInt32(train.cas.Substring(2, 2)) + train.zpozdeni select train).ToArray()[0];
            return neco[timeasIndex];
        }
        private void dopredi_Click(object sender, RoutedEventArgs e)
        {

        }
        public void Exit()
        {
            tabule.Exit();
            this.Close();
        }
        private void zpet_Click(object sender, RoutedEventArgs e)
        {

        }
        void asyncZjistovani()
        {
            while (true)
            {
                if (Convert.ToInt32(zjisteni.cas) < Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()))
                {
                    zjisteni = SortAndTellClosestToTime(nacist());
                    tabule.ChangeData(zjisteni);
                }
                System.Threading.Thread.Sleep(5000);
            }
        }
        public static Vlak[] nacist()
        {
            StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/test.vlk");
            int pocetRadku = 0;
            while (!sr.EndOfStream)
            {
                pocetRadku++;
                sr.ReadLine();
            }
            sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/test.vlk");
            Vlak[] docasny = new Vlak[pocetRadku];
            Queue<string> system = new Queue<string>();
            TrainType mark = TrainType.Os;
            Company firma = Company.CD;
            pocetRadku = 0;
            while (!sr.EndOfStream)
            {
                string[] temp2 = sr.ReadLine().Split(',');
                foreach (var item in temp2)
                {
                    system.Enqueue(item);
                }
                if (pocetRadku != 0)
                {
                    system.Dequeue();
                }
                string temp = system.Dequeue();
                #region Chujo Switch
                switch (temp)
                {
                    case "CD":
                        firma = Company.CD;
                        break;
                    case "LE":
                        firma = Company.LE;
                        break;
                    case "RJ":
                        firma = Company.RJ;
                        break;
                }
                #endregion
                string nazevVlaku = system.Dequeue();
                temp = system.Dequeue();
                #region Chujo Switch
                switch (temp)
                {
                    case "Os":
                        mark = TrainType.Os;
                        break;
                    case "Sp":
                        mark = TrainType.Sp;
                        break;
                    case "R":
                        mark = TrainType.R;
                        break;
                    case "Ex":
                        mark = TrainType.Ex;
                        break;
                    case "IC":
                        mark = TrainType.IC;
                        break;
                    case "EC":
                        mark = TrainType.EC;
                        break;
                    case "SC":
                        mark = TrainType.SC;
                        break;
                    case "LE":
                        mark = TrainType.LE;
                        break;
                    case "EN":
                        mark = TrainType.EN;
                        break;
                    case "RJ":
                        mark = TrainType.RJ;
                        break;
                    case "Mv":
                        mark = TrainType.Mv;
                        break;
                    case "Rx":
                        mark = TrainType.Rx;
                        break;
                }
                #endregion
                string cisloVlaku = system.Dequeue();
                Dictionary<string, UltimatniInformace> databaze = new Dictionary<string, UltimatniInformace>();
                databaze.Add(system.Dequeue(), new UltimatniInformace("", system.Dequeue()));
                for (int i = 0; i < system.Count+i-1; i++)
                {
                    string prijezd = system.Dequeue();
                    string nazevstanicky = system.Dequeue();
                    string odjezd = system.Dequeue();
                    databaze.Add(nazevstanicky, new UltimatniInformace(prijezd, odjezd));
                }
                docasny[pocetRadku] = new Vlak(cisloVlaku, mark, new JizdniRad(databaze), "1", "1", firma, databaze.Keys.ToArray<string>()[0],nazevVlaku);
                pocetRadku++;
            }
            return docasny;
        }

        private void pres_Loaded(object sender, RoutedEventArgs e)
        {
            pres.Content = "";
        }
    }
    public static class OutputData
    {
        public static Dictionary<string, string> dostupneStanice = new Dictionary<string, string>();
        public static List<Vlak> poleVlaku = new List<Vlak>(MainWindow.nacist());
    }
    public class NadrazniTabule
    {
        Label druhVlacku;
        Label cestaVlacku;
        Label konecVlacku;
        Label casVlacku;
        Label odjezdVlacku;
        Label zpozdeniVlacku;
        Label cisloVlacku;
        Image svetlo;
        Vlak vlacek;
        public object grabber = new Object();
        System.Threading.Thread asyncP;
        System.Threading.Thread asyncH;
        System.Threading.Thread asyncZ;
        System.Threading.Thread asyncC;
        public NadrazniTabule(Vlak vstupnivlacek)
        {
            vlacek = vstupnivlacek;
            new System.Threading.Thread(asyncPres) { IsBackground = true }.Start();
            new System.Threading.Thread(asyncHodiny) { IsBackground = true }.Start();
            new System.Threading.Thread(asyncZpozdeni) { IsBackground = true }.Start();
            Inicializace();
        }
        public NadrazniTabule(Label druhVlaku, Label cestaVlaku, Label konecVlaku, Label cas, Label odjezdVlaku, Label zpozdeniVlaku, Label cisloVlaku, Image obrazecek, Vlak vstupnivlacek)
        {
            druhVlacku = druhVlaku;
            cestaVlacku = cestaVlaku;
            konecVlacku = konecVlaku;
            casVlacku = cas;
            odjezdVlacku = odjezdVlaku;
            zpozdeniVlacku = zpozdeniVlaku;
            cisloVlacku = cisloVlaku;
            vlacek = vstupnivlacek;
            svetlo = obrazecek;
            Inicializace();
        }
        private void Inicializace()
        {
            asyncP = new System.Threading.Thread(asyncPres) { IsBackground = true };
            asyncH = new System.Threading.Thread(asyncHodiny) { IsBackground = true };
            asyncZ = new System.Threading.Thread(asyncZpozdeni) { IsBackground = true };
            asyncP.Start();
            asyncH.Start();
            asyncZ.Start();
            druhVlacku.Content = vlacek.druh;
            cisloVlacku.Content = vlacek.cislo;
            string temp2 = vlacek.cas.ToString().Substring(0, 2);
            string temp1 = vlacek.cas.ToString().Substring(2, 2);
            odjezdVlacku.Content = temp2 + ":" + temp1;
            zpozdeniVlacku.Content = vlacek.zpozdeni;
            if (vlacek.druh == TrainType.Os)
            {
                svetlo.Visibility = Visibility.Visible;
            }
            if (vlacek.druh == TrainType.IC || vlacek.druh == TrainType.SC || vlacek.druh == TrainType.EC || vlacek.druh == TrainType.EN)
            {
                druhVlacku.FontStyle = FontStyles.Italic;
            }
            if (vlacek.zpozdeni == 0)
            {
                zpozdeniVlacku.Content = "  ";
            }
            if (vlacek.cilVlaku.Length > 10)
            {
                konecVlacku.Content = vlacek.cilVlaku.ToUpper();
                asyncC = new System.Threading.Thread(asyncCil) { IsBackground = true };
                asyncC.Start();
            }
            else
            {
                konecVlacku.Content = vlacek.cilVlaku.ToUpper();
            }
            new System.Threading.Thread(asyncHlaseni) { IsBackground = true }.Start();
        }
        public void asyncHlaseni()
        {
            //new Hlasit(this).Prijizdi(this.vlacek.zpozdeni != 0);
            new Hlasit(this).Prijel(this.vlacek.zpozdeni != 0);
        }
        public void asyncCil()
        {
            string temp = vlacek.cilVlaku.ToUpper();
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(500);
                    if (temp.Length == 0)
                    {
                        temp = vlacek.cilVlaku;
                        konecVlacku.Dispatcher.Invoke(() => { konecVlacku.Content = temp.ToUpper(); });
                    }
                    else
                    {
                        temp = temp.Substring(1, temp.Length - 1);
                        konecVlacku.Dispatcher.Invoke(() => { konecVlacku.Content = temp.ToUpper(); });
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        public void asyncHodiny()
        {
            while (true)
            {
                string minuty = DateTime.Now.Minute.ToString();
                if (minuty.Substring(0, 1) != "0" && minuty.Length == 1)
                {
                    char temp = minuty[0];
                    minuty = "0" + temp;
                }
                if (minuty == "0")
                {
                    minuty = "00";
                }
                casVlacku.Dispatcher.Invoke(() => { casVlacku.Content = DateTime.Now.Hour + ":" + minuty; });
                System.Threading.Thread.Sleep(5000);
            }
        }
        public void asyncPres()
        {
            try
            {
                string[] arr;
                while (true)
                {
                    arr = new string[vlacek.jizda.trasa.Length - vlacek.jizda.aktualniZastavka - 1];
                    for (int i = vlacek.jizda.aktualniZastavka; i < vlacek.jizda.trasa.Length - 1; i++)
                    {
                        arr[i - vlacek.jizda.aktualniZastavka] = vlacek.jizda.trasa[i];
                    }
                    string[] temp = new string[arr.Length - 1];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (i != 0 && i != arr.Length)
                        {
                            temp[i - 1] = arr[i];
                        }
                    }
                    arr = temp;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        cestaVlacku.Dispatcher.Invoke(() => { cestaVlacku.Content = arr[i]; });
                        System.Threading.Thread.Sleep(2500);
                        if (arr[i].Length > 12)
                        {
                            while (true)
                            {
                                if (arr[i].Length == 0)
                                {
                                    break;
                                }
                                arr[i] = arr[i].Substring(1, arr[i].Length - 1);
                                cestaVlacku.Dispatcher.Invoke(() => { cestaVlacku.Content = arr[i]; });
                                System.Threading.Thread.Sleep(285);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        public void asyncZpozdeni()
        {
            int temp = vlacek.zpozdeni;
            while (true)
            {
                if (temp != vlacek.zpozdeni)
                {
                    zpozdeniVlacku.Dispatcher.Invoke(() => { zpozdeniVlacku.Content = vlacek.zpozdeni; });
                }
                System.Threading.Thread.Sleep(5000);
            }
        }
        public void Exit()
        {
            asyncC.Abort();
            asyncH.Abort();
            asyncP.Abort();
            asyncZ.Abort();
        }
        public void ChangeData(Vlak vlak)
        {
            konecVlacku.Dispatcher.Invoke(() => Exit());
            konecVlacku.Dispatcher.Invoke(() => vlacek = vlak);
            konecVlacku.Dispatcher.Invoke(() => Inicializace());
        }
        public class Hlasit
        {
            NadrazniTabule aktual;
            ToiletFire sf = new ToiletFire(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\databanka\\");
            public Hlasit(NadrazniTabule aktualniTabule)
            {
                aktual = aktualniTabule;
            }
            /*sf.ReadNumber(Convert.ToInt32(aktual.cislo));
            sf.ReadCategory(aktual.druh);
            sf.ReadCompany(aktual.firma);
            sf.PlayStationSong(aktual.jizda.AktualniPoloha());
            sf.PlayStationName(aktual.jizda.AktualniPoloha());
            sf.ReadTrainName(aktual.nazevVlaku);*/
            public void Prijizdi()
            {
                if (aktual.vlacek.jizda.aktualniZastavka == 0)
                {
                    return;
                }
                sf.PlayStationSong(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayStationName(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayAddS(KnownMessages.vazeniCestujici);
                sf.ReadCategory(aktual.vlacek.druh);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.cislo));
                sf.ReadTrainName(aktual.vlacek.nazevVlaku);
                sf.ReadCompany(aktual.vlacek.firma);
                if (aktual.vlacek.jizda.aktualniZastavka != 0)
                {
                    sf.PlayAddS(KnownMessages.zeSmeru);
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            break;
                        }
                        sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                    }
                }
                sf.PlayAddS(KnownMessages.prijedeKNastupisti);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.nastupisteVlaku));
                sf.PlayAddS(KnownMessages.kolej);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.kolejVlaku));
                if (aktual.vlacek.jizda.AktualniPoloha() != aktual.vlacek.cilVlaku)
                {
                    sf.PlayAddS(KnownMessages.budePokracovatVeSmeru);
                    bool temp = false;
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        int index = 0;
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            temp = true;
                        }
                        if (temp)
                        {
                            if (aktual.vlacek.jizda.trasa[i] != aktual.vlacek.jizda.AktualniPoloha())
                            {
                                if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.trasa[aktual.vlacek.jizda.trasa.Length - 1])
                                {
                                    if (index != 0)
                                    {
                                        sf.PlayAddS(KnownMessages.a);
                                    }
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                else
                                {
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                index++;
                            }
                        }
                    }
                    sf.PlayAddS(KnownMessages.pravidelnyOdjezd);
                    sf.ReadTime(aktual.vlacek.jizda.casy[aktual.vlacek.AktualniPoloha()].casOdjezdu); 
                }
                sf.PlayStationSong("instant");
            }
            public void Prijizdi(bool spozdeny)
            {
                if (aktual.vlacek.jizda.aktualniZastavka == 0)
                {
                    return;
                }
                sf.PlayStationSong(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayStationName(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayAddS(KnownMessages.vazeniCestujici);
                if (spozdeny)
                    sf.PlayAddS(KnownMessages.zpozdeny);
                sf.ReadCategory(aktual.vlacek.druh);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.cislo));
                sf.ReadTrainName(aktual.vlacek.nazevVlaku);
                sf.ReadCompany(aktual.vlacek.firma);
                if (aktual.vlacek.jizda.aktualniZastavka != 0)
                {
                    sf.PlayAddS(KnownMessages.zeSmeru);
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            break;
                        }
                        sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                    }
                }
                sf.PlayAddS(KnownMessages.prijedeKNastupisti);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.nastupisteVlaku));
                sf.PlayAddS(KnownMessages.kolej);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.kolejVlaku));
                if (aktual.vlacek.jizda.AktualniPoloha() != aktual.vlacek.cilVlaku)
                {
                    sf.PlayAddS(KnownMessages.budePokracovatVeSmeru);
                    bool temp = false;
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        int index = 0;
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            temp = true;
                        }
                        if (temp)
                        {
                            if (aktual.vlacek.jizda.trasa[i] != aktual.vlacek.jizda.AktualniPoloha())
                            {
                                if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.trasa[aktual.vlacek.jizda.trasa.Length - 1])
                                {
                                    if (index != 0)
                                    {
                                        sf.PlayAddS(KnownMessages.a);
                                    }
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                else
                                {
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                index++;
                            }
                        }
                    }
                    sf.PlayAddS(KnownMessages.pravidelnyOdjezd);
                    sf.ReadTime(aktual.vlacek.jizda.casy[aktual.vlacek.AktualniPoloha()].casOdjezdu); 
                }
                sf.PlayStationSong("instant");
            }
            public void Prijel()
            {
                sf.PlayStationSong(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayStationName(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayAddS(KnownMessages.vazeniCestujici);
                sf.ReadCategory(aktual.vlacek.druh);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.cislo));
                sf.ReadTrainName(aktual.vlacek.nazevVlaku);
                sf.ReadCompany(aktual.vlacek.firma);
                if (aktual.vlacek.jizda.aktualniZastavka != 0)
                {
                    sf.PlayAddS(KnownMessages.zeSmeru);
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            break;
                        }
                        sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                    } 
                }
                sf.PlayAddS(KnownMessages.prijelKNastupisti);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.nastupisteVlaku));
                sf.PlayAddS(KnownMessages.kolej);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.kolejVlaku));
                if (aktual.vlacek.jizda.AktualniPoloha() != aktual.vlacek.cilVlaku)
                {
                    sf.PlayAddS(KnownMessages.budePokracovatVeSmeru);
                    bool temp = false;
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        int index = 0;
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            temp = true;
                        }
                        if (temp)
                        {
                            if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                            {
                                if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.trasa[aktual.vlacek.jizda.trasa.Length - 1])
                                {
                                    if (index != 0)
                                    {
                                        sf.PlayAddS(KnownMessages.a);
                                    }
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                else
                                {
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                index++;
                            }
                        }
                    }
                    sf.PlayAddS(KnownMessages.pravidelnyOdjezd);
                    sf.ReadTime(aktual.vlacek.jizda.casy[aktual.vlacek.AktualniPoloha()].casOdjezdu); 
                }
                sf.PlayStationSong("instant");
            }
            public void Prijel(bool spozdeny)
            {
                sf.PlayStationSong(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayStationName(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayAddS(KnownMessages.vazeniCestujici);
                if (spozdeny)
                    sf.PlayAddS(KnownMessages.zpozdeny);
                sf.ReadCategory(aktual.vlacek.druh);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.cislo));
                sf.ReadTrainName(aktual.vlacek.nazevVlaku);
                sf.ReadCompany(aktual.vlacek.firma);
                if (aktual.vlacek.jizda.aktualniZastavka != 0)
                {
                    sf.PlayAddS(KnownMessages.zeSmeru);
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            break;
                        }
                        sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                    }
                }
                sf.PlayAddS(KnownMessages.prijelKNastupisti);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.nastupisteVlaku));
                sf.PlayAddS(KnownMessages.kolej);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.kolejVlaku));
                if (aktual.vlacek.jizda.AktualniPoloha() != aktual.vlacek.cilVlaku)
                {
                    sf.PlayAddS(KnownMessages.budePokracovatVeSmeru);
                    bool temp = false;
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        int index = 0;
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            temp = true;
                        }
                        if (temp)
                        {
                            if (aktual.vlacek.jizda.trasa[i] != aktual.vlacek.jizda.AktualniPoloha())
                            {
                                if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.trasa[aktual.vlacek.jizda.trasa.Length - 1])
                                {
                                    if (index != 0)
                                    {
                                        sf.PlayAddS(KnownMessages.a);
                                    }
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                else
                                {
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                index++;
                            }
                        }
                    }
                    sf.PlayAddS(KnownMessages.pravidelnyOdjezd);
                    sf.ReadTime(aktual.vlacek.jizda.casy[aktual.vlacek.AktualniPoloha()].casOdjezdu); 
                }
                sf.PlayStationSong("instant");
            }
            public void Prijel(bool hlasitUkoncitNastup, bool spozdeny)
            {
                sf.PlayStationSong(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayStationName(aktual.vlacek.jizda.AktualniPoloha());
                sf.PlayAddS(KnownMessages.vazeniCestujici);
                if (spozdeny)
                    sf.PlayAddS(KnownMessages.zpozdeny);
                sf.ReadCategory(aktual.vlacek.druh);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.cislo));
                sf.ReadTrainName(aktual.vlacek.nazevVlaku);
                sf.ReadCompany(aktual.vlacek.firma);
                sf.PlayAddS(KnownMessages.zeSmeru);
                if (aktual.vlacek.jizda.aktualniZastavka != 0)
                {
                    sf.PlayAddS(KnownMessages.zeSmeru);
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            break;
                        }
                        sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                    }
                }
                sf.PlayAddS(KnownMessages.prijelKNastupisti);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.nastupisteVlaku));
                sf.PlayAddS(KnownMessages.kolej);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.kolejVlaku));
                if (aktual.vlacek.jizda.AktualniPoloha() != aktual.vlacek.cilVlaku)
                {
                    sf.PlayAddS(KnownMessages.budePokracovatVeSmeru);
                    bool temp = false;
                    for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                    {
                        int index = 0;
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.AktualniPoloha())
                        {
                            temp = true;
                        }
                        if (temp)
                        {
                            if (aktual.vlacek.jizda.trasa[i] != aktual.vlacek.jizda.AktualniPoloha())
                            {
                                if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.trasa[aktual.vlacek.jizda.trasa.Length - 1])
                                {
                                    if (index != 0)
                                    {
                                        sf.PlayAddS(KnownMessages.a);
                                    }
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                else
                                {
                                    sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                                }
                                index++;
                            }
                        }
                    }
                    sf.PlayAddS(KnownMessages.pravidelnyOdjezd);
                    sf.ReadTime(aktual.vlacek.jizda.casy[aktual.vlacek.AktualniPoloha()].casOdjezdu);
                    if (hlasitUkoncitNastup)
                        sf.PlayAddS(KnownMessages.ukonceteNastupOdjezdVlaku); 
                }
                sf.PlayStationSong("instant");
            }
            public void UkonceteNastup()
            {
                throw new NotImplementedException();
                sf.PlayAddS(KnownMessages.vazeniCestujici);
                //sf.PlayAddS(KnownMessages) NA NASTUPISTI!!! - DOKOPAT ZADARMA
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.nastupisteVlaku));
                sf.PlayAddS(KnownMessages.kolej);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.kolejVlaku));
                //sf.PlayAddS(KnownMessages) UKONCETE NASTUP DO !!! - DOKOPAT ZADARMA
                sf.ReadCategory(aktual.vlacek.druh);
                sf.ReadNumber(Convert.ToInt32(aktual.vlacek.cislo));
                sf.ReadTrainName(aktual.vlacek.nazevVlaku);
                sf.ReadCompany(aktual.vlacek.firma);
                //sf.PlayAddS(KnownMessages) VE SMERU !!! - DOKOPAT ZADARMA
                bool temp = false;
                for (int i = 0; i < aktual.vlacek.jizda.trasa.Length; i++)
                {
                    if (aktual.vlacek.jizda.trasa[i] != aktual.vlacek.jizda.AktualniPoloha())
                    {
                        temp = true;
                    }
                    if (temp)
                    {
                        if (aktual.vlacek.jizda.trasa[i] == aktual.vlacek.jizda.trasa[aktual.vlacek.jizda.trasa.Length - 1])
                        {
                            sf.PlayAddS(KnownMessages.a);
                            sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                        }
                        else
                        {
                            sf.PlayStationName(aktual.vlacek.jizda.trasa[i]);
                        }
                    }
                }
                sf.PlayAddS(KnownMessages.pravidelnyOdjezd);
                sf.ReadTime(aktual.vlacek.jizda.casy[aktual.vlacek.AktualniPoloha()].casOdjezdu);
                sf.PlayAddS(KnownMessages.ukonceteNastupOdjezdVlaku);
                sf.PlayStationSong("instant");
            }
            public void Spozdeni()
            {
                throw new NotImplementedException();
                sf.PlayStationSong("instant");
            }
        }
    }
    public class Vlak
    {
        public string cislo { get; private set; }
        public TrainType druh { get; private set; }
        public JizdniRad jizda { get; private set; }
        public int zpozdeni = 0;
        public string kolejVlaku { get; set; }
        public string nastupisteVlaku { get; set; }
        public string cas;
        public string cilVlaku;
        private bool jede = false;
        public Company firma { get; private set; }
        public string nazevVlaku { get; private set; }
        public Vlak(string cisloVlaku, TrainType typ, JizdniRad cesta, string kolej, string nastupiste, Company drazniFirma, string aktualniStanice, string nazev)
        {
            cislo = cisloVlaku;
            druh = typ;
            jizda = cesta;
            kolejVlaku = kolej;
            nastupisteVlaku = nastupiste;
            firma = drazniFirma;
            if (cilVlaku == null)
                cilVlaku = jizda.trasa[jizda.trasa.Length - 1];
            cas = cesta.casy[aktualniStanice].casOdjezdu;
            nazevVlaku = nazev;
        }
        public void Prijezd(string kolej, string nastupiste)
        {
            jizda.DalsiZastavka();
            kolejVlaku = kolej;
            nastupisteVlaku = nastupiste;
            jede = false;
        }
        public void Odjezd()
        {
            kolejVlaku = null;
            nastupisteVlaku = null;
            jede = true;
        }
        public string AktualniPoloha()
        {
            if (!jede)
            {
                return jizda.AktualniPoloha();
            }
            return "Je na cestě.";
        }
    }
    public class JizdniRad
    {
        /* Polí stringů s trasou
         * 
         * 
         */
        public string[] trasa { private set; get; }
        public int aktualniZastavka;
        public string aktualniStanice;
        public Dictionary<string, UltimatniInformace> casy { get; private set; }
        public JizdniRad(Dictionary<string, UltimatniInformace> cas)
        {
            casy = cas;
            trasa = cas.Keys.ToArray<string>();
            aktualniZastavka = 0;
            aktualniStanice = cas.Keys.ToArray<string>()[aktualniZastavka];
            foreach (var item in trasa)
            {
                try
                {
                    OutputData.dostupneStanice.Add(item, null);
                }
                catch
                {
                }
            }
        }
        public JizdniRad(Dictionary<string, UltimatniInformace> cas, int aktualniStaniceIndex)
        {
            casy = cas;
            trasa = cas.Keys.ToArray<string>();
            aktualniZastavka = aktualniStaniceIndex;
            aktualniStanice = cas.Keys.ToArray<string>()[aktualniZastavka];
        }
        public string AktualniPoloha()
        {
            return aktualniStanice;
        }
        public void DalsiZastavka()
        {
            aktualniZastavka++;
            aktualniStanice = casy.Keys.ToArray<string>()[aktualniZastavka];
        }
    }
    public class UltimatniInformace
    {
        public string casPrijezdu { get; set; }
        public string casOdjezdu { get; set; }
        public UltimatniInformace(string casP, string casO)
        {
            casPrijezdu = casP;
            casOdjezdu = casO;
        }
    }

}