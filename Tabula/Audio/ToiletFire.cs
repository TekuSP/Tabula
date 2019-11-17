using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Tabula.Enums;

namespace Tabula.Audio
{
    [Obsolete("ToiletFire is 2016 project, very poorly written and very poorly optimized, please use newer version", false)]
    public class ToiletFire
    {
        string appPath;
        SoundEngine_ToiletFire_NumbersEngine numb;
        SoundEngine_ToiletFire_Category ct;
        SoundEngine_ToiletFire_Company co;
        SoundEngine_ToiletFire_AdditionalsSounds ass;
        SoundEngine_ToiletFire_StationName st;
        SoundEngine_ToiletFire_StationSounds ss;
        SoundEngine_ToiletFire_TrainName tn;
        public ToiletFire(string applicationPath)
        {
            appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.tabula/databanka";
            numb = new SoundEngine_ToiletFire_NumbersEngine(appPath);
            ct = new SoundEngine_ToiletFire_Category(appPath);
            co = new SoundEngine_ToiletFire_Company(appPath);
            ss = new SoundEngine_ToiletFire_StationSounds(appPath);
            tn = new SoundEngine_ToiletFire_TrainName(appPath);
            ass = new SoundEngine_ToiletFire_AdditionalsSounds(appPath);
            st = new SoundEngine_ToiletFire_StationName(appPath);
        }
        public void ReadNumber(int number)
        {
            foreach (var item in numb.GetSoundOfNumber(number))
            {
                item.PlaySync();
            }
        }
        public void ReadCategory(TrainType typVlaku)
        {
            ct.GetSoundsOfCategory(typVlaku);
        }
        public void ReadCompany(Company Company)
        {
            co.GetSoundsOfCompany(Company);
        }
        public void PlayStationSong(string aktualniStanice)
        {
            ss.GetSoundOfStationSounds(aktualniStanice);
        }
        public void ReadTrainName(string nazevVlaku)
        {
            tn.GetSoundOfTrainName(nazevVlaku);
        }
        public void PlayAddS(KnownMessages hlaska)
        {
            ass.GetSoundOfAdditionalSounds(hlaska);
        }
        public void PlayStationName(string nazevStanice, bool playDef = false)
        {
            if (playDef)
            {
                st.GetSoundOfStationName("DEF");
            }
            st.GetSoundOfStationName(nazevStanice);
        }
        public void ReadTime(string time)
        {
            ReadNumber(Convert.ToInt32(time.Substring(0, 2)));
            PlayAddS(KnownMessages.hodin);
            ReadNumber(Convert.ToInt32(time.Substring(2, 2)));
            PlayAddS(KnownMessages.minut);
        }
        public class SoundEngine_ToiletFire_NumbersEngine
        {
            Dictionary<Int32, SoundPlayer> cisla;
            public SoundEngine_ToiletFire_NumbersEngine(string appPath)
            {
                cisla = new Dictionary<int, SoundPlayer>();
                foreach (string item in Directory.GetFiles(Path.Combine(appPath, "cisla")))
                {
                    cisla.Add(Convert.ToInt32(item.Substring(item.IndexOf("cisla") + 6, item.Length - 4 - (item.IndexOf("cisla") + 6))), new SoundPlayer(item));
                    cisla[Convert.ToInt32(item.Substring((item.IndexOf("cisla") + 6), item.Length - (item.IndexOf("cisla") + 6) - 4))].LoadAsync();
                }
                var keys = cisla.Keys.ToArray<int>();
                keys = (from item in keys orderby item select item).ToArray<int>();
                var dict = new Dictionary<int, SoundPlayer>();
                foreach (int item in keys)
                    dict.Add(item, cisla[item]);
                cisla = dict;
            }
            public Queue<SoundPlayer> GetSoundOfNumber(Int32 number)
            {
                Queue<SoundPlayer> returnQueue = new Queue<SoundPlayer>();
                var reversedKeys = cisla.Keys.ToArray();
                Array.Reverse(reversedKeys);
                if (number.ToString().Length < 4)
                {
                    number++;
                    foreach (var item in reversedKeys)
                    {
                        if (number - item > 0)
                        {
                            returnQueue.Enqueue(cisla[item]);
                            number -= item;
                        }
                    }
                    var arr = new SoundPlayer[returnQueue.Count - 1];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        arr[i] = returnQueue.Dequeue();
                    }
                    returnQueue.Clear();
                    foreach (SoundPlayer player in arr)
                        returnQueue.Enqueue(player);
                }
                else if (number.ToString().Length == 4)
                {
                    foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(0, 2))))
                        returnQueue.Enqueue(player);
                    foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(2, 2))))
                        returnQueue.Enqueue(player);
                }
                else if (number.ToString().Length == 5)
                {
                    foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(0, 2))))
                        returnQueue.Enqueue(player);
                    foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(2, 3))))
                        returnQueue.Enqueue(player);
                }
                else if (number.ToString().Length == 6)
                {
                    foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(0, 3))))
                        returnQueue.Enqueue(player);
                    foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(3, 3))))
                        returnQueue.Enqueue(player);
                }
                else if (number.ToString().Length > 6)
                {
                    for (int i = 0; i < number.ToString().Length; i++)
                    {
                        foreach (SoundPlayer player in GetSoundOfNumber(Convert.ToInt32(number.ToString().Substring(i, 1))))
                            returnQueue.Enqueue(player);
                    }
                }
                return returnQueue;
            }
        }
        public class SoundEngine_ToiletFire_Category
        {
            Dictionary<TrainType, SoundPlayer> databaze;
            public SoundEngine_ToiletFire_Category(string appPath)
            {
                appPath = appPath + "\\typ\\";
                databaze = new Dictionary<TrainType, SoundPlayer>();
                foreach (string item in Directory.GetFiles(appPath))
                {
                    TrainType mark = TrainType.Os;
                    #region Chuj Switch
                    switch (item.Substring(item.IndexOf("typ") + 4, item.Length - (item.IndexOf("typ") + 4)))
                    {
                        case "Os.wav":
                            mark = TrainType.Os;
                            break;
                        case "Sp.wav":
                            mark = TrainType.Sp;
                            break;
                        case "R.wav":
                            mark = TrainType.R;
                            break;
                        case "Ex.wav":
                            mark = TrainType.Ex;
                            break;
                        case "IC.wav":
                            mark = TrainType.IC;
                            break;
                        case "EC.wav":
                            mark = TrainType.EC;
                            break;
                        case "SC.wav":
                            mark = TrainType.SC;
                            break;
                        case "LE.wav":
                            mark = TrainType.LE;
                            break;
                        case "EN.wav":
                            mark = TrainType.EN;
                            break;
                        case "RJ.wav":
                            mark = TrainType.RJ;
                            break;
                        case "Mv.wav":
                            mark = TrainType.Mv;
                            break;
                        case "Rx.wav":
                            mark = TrainType.Rx;
                            break;
                    }
                    #endregion/
                    databaze.Add(mark, new SoundPlayer(item));
                    foreach (var item2 in databaze)
                    {
                        item2.Value.LoadAsync();
                    }
                }
            }
            public void GetSoundsOfCategory(TrainType druhVlaku)
            {
                databaze[druhVlaku].PlaySync();
            }
        }
        public class SoundEngine_ToiletFire_Company
        {
            Dictionary<Company, SoundPlayer> databaze;
            public SoundEngine_ToiletFire_Company(string appPath)
            {
                databaze = new Dictionary<Company, SoundPlayer>();
                appPath = appPath + "\\spolecnost\\";
                foreach (string item in Directory.GetFiles(appPath))
                {
                    Company mark = Company.CD;
                    #region Chuj Switch
                    switch (item.Substring(item.IndexOf("spolecnost") + 11, item.Length - (item.IndexOf("spolecnost") + 11)))
                    {
                        case "CD.wav":
                            mark = Company.CD;
                            break;
                        case "LE.wav":
                            mark = Company.LE;
                            break;
                        case "RJ.wav":
                            mark = Company.RJ;
                            break;
                    }
                    #endregion
                    databaze.Add(mark, new SoundPlayer(item));
                    foreach (var item2 in databaze)
                    {
                        item2.Value.LoadAsync();
                    }
                }
            }
            public void GetSoundsOfCompany(Company Company)
            {
                databaze[Company].PlaySync();
            }
        }
        public class SoundEngine_ToiletFire_StationSounds
        {
            Dictionary<string, SoundPlayer> databaze;
            public SoundEngine_ToiletFire_StationSounds(string appPath)
            {
                databaze = new Dictionary<string, SoundPlayer>();
                foreach (string item in Directory.GetFiles(appPath + "\\znelky\\"))
                {
                    databaze.Add(item.Substring(item.IndexOf("znelky") + 7, item.Length - 4 - (item.IndexOf("znelky") + 7)), new SoundPlayer(item));
                    databaze[item.Substring((item.IndexOf("znelky") + 7), item.Length - (item.IndexOf("znelky") + 7) - 4)].LoadAsync();
                }
                var keys = databaze.Keys.ToArray<string>();
                keys = (from item in keys orderby item select item).ToArray<string>();
                var dict = new Dictionary<string, SoundPlayer>();
                foreach (string item in keys)
                    dict.Add(item, databaze[item]);
                databaze = dict;
            }
            public void GetSoundOfStationSounds(string aktualniStanice)
            {
                SoundPlayer prehravac = new SoundPlayer();
                databaze.TryGetValue(aktualniStanice, out prehravac);
                if (prehravac == default(SoundPlayer))
                    databaze["default"].PlaySync();
                else
                    prehravac.PlaySync();
            }
        }
        public class SoundEngine_ToiletFire_StationName
        {
            Dictionary<string, SoundPlayer> databaze;
            public SoundEngine_ToiletFire_StationName(string appPath)
            {
                databaze = new Dictionary<string, SoundPlayer>();
                foreach (string item in Directory.GetFiles(appPath + "\\stanice\\"))
                {
                    databaze.Add(item.Substring(item.IndexOf("stanice") + 8, item.Length - 4 - (item.IndexOf("stanice") + 8)), new SoundPlayer(item));
                    databaze[item.Substring((item.IndexOf("stanice") + 8), item.Length - (item.IndexOf("stanice") + 8) - 4)].LoadAsync();
                }
                var keys = databaze.Keys.ToArray<string>();
                keys = (from item in keys orderby item select item).ToArray<string>();
                var dict = new Dictionary<string, SoundPlayer>();
                foreach (string item in keys)
                    dict.Add(item, databaze[item]);
                databaze = dict;
            }
            public void GetSoundOfStationName(string stanice)
            {
                SoundPlayer prehravac = new SoundPlayer();
                databaze.TryGetValue(stanice, out prehravac);
                if (prehravac == default(SoundPlayer))
                    databaze["default"].PlaySync();
                else
                    prehravac.PlaySync();
            }
        }
        public class SoundEngine_ToiletFire_TrainName
        {
            Dictionary<string, SoundPlayer> databaze;
            public SoundEngine_ToiletFire_TrainName(string appPath)
            {
                databaze = new Dictionary<string, SoundPlayer>();
                foreach (string item in Directory.GetFiles(appPath + "\\nazev\\"))
                {
                    databaze.Add(item.Substring(item.IndexOf("nazev") + 6, item.Length - 4 - (item.IndexOf("nazev") + 6)), new SoundPlayer(item));
                    databaze[item.Substring((item.IndexOf("nazev") + 6), item.Length - (item.IndexOf("nazev") + 6) - 4)].LoadAsync();
                }
                var keys = databaze.Keys.ToArray<string>();
                keys = (from item in keys orderby item select item).ToArray<string>();
                var dict = new Dictionary<string, SoundPlayer>();
                foreach (string item in keys)
                    dict.Add(item, databaze[item]);
                databaze = dict;
            }
            public void GetSoundOfTrainName(string nazev)
            {
                if (string.IsNullOrEmpty(nazev) || string.IsNullOrWhiteSpace(nazev))
                    return;
                SoundPlayer prehravac = new SoundPlayer();
                databaze.TryGetValue(nazev, out prehravac);
                if (prehravac == default(SoundPlayer))
                    databaze["default"].PlaySync();
                else
                    prehravac.PlaySync();
            }
        }
        public class SoundEngine_ToiletFire_AdditionalsSounds
        {
            Dictionary<KnownMessages, SoundPlayer> databaze;
            public SoundEngine_ToiletFire_AdditionalsSounds(string appPath)
            {
                databaze = new Dictionary<KnownMessages, SoundPlayer>();
                appPath = appPath + "\\mezi\\";
                foreach (string item in Directory.GetFiles(appPath))
                {
                    KnownMessages mark = KnownMessages.vazeniCestujici;
                    #region Chuj Switch
                    switch (item.Substring(item.IndexOf("mezi") + 5, item.Length - (item.IndexOf("mezi") + 5)))
                    {
                        case "a.wav":
                            mark = KnownMessages.a;
                            break;
                        case "budepokracovat.wav":
                            mark = KnownMessages.budePokracovatVeSmeru;
                            break;
                        case "hodin.wav":
                            mark = KnownMessages.hodin;
                            break;
                        case "kolej.wav":
                            mark = KnownMessages.kolej;
                            break;
                        case "minut.wav":
                            mark = KnownMessages.minut;
                            break;
                        case "omluva.wav":
                            mark = KnownMessages.omluvaZaZpozdeni;
                            break;
                        case "pravidelny.wav":
                            mark = KnownMessages.pravidelnyOdjezd;
                            break;
                        case "prijede.wav":
                            mark = KnownMessages.prijedeKNastupisti;
                            break;
                        case "prijel.wav":
                            mark = KnownMessages.prijelKNastupisti;
                            break;
                        case "ukoncete.wav":
                            mark = KnownMessages.ukonceteNastupOdjezdVlaku;
                            break;
                        case "vazenicestujici.wav":
                            mark = KnownMessages.vazeniCestujici;
                            break;
                        case "zesmeru.wav":
                            mark = KnownMessages.zeSmeru;
                            break;
                        case "zpozdeny.wav":
                            mark = KnownMessages.zpozdeny;
                            break;
                    }
                    #endregion
                    databaze.Add(mark, new SoundPlayer(item));
                    foreach (var item2 in databaze)
                    {
                        item2.Value.LoadAsync();
                    }
                }
            }
            public void GetSoundOfAdditionalSounds(KnownMessages hlaska)
            {
                databaze[hlaska].PlaySync();
            }
        }
    }
}
