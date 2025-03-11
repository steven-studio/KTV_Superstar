using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

namespace DualScreenDemo
{
    public class CommandHandler
    {
        public static bool readyForSongListInput = false;

        private readonly SongListManager songListManager;

        public CommandHandler(SongListManager songListManager)
        {
            this.songListManager = songListManager;
        }

        public async Task ProcessData(string indata)
        {
            string filePath = Path.Combine(Application.StartupPath, "dataLog.txt");
            if (CheckLogForShutdown(filePath))
            {
                Console.WriteLine("Shutdown condition met. Application will now close.");
                ShutdownComputer();
            }

            switch (indata)
            {
                case "A261A4":
                    HandleInputA();
                    break;
                case "A262A4":
                    HandleInputB();
                    break;
                case "A263A4":
                    ClearDisplay();
                    break;
                case "A268A4":
                    OverlayForm.MainForm.currentPage = 1;
                    DisplaySongHistory();
                    break;
                case "A26AA4":
                    PreviousPage();
                    break;
                case "A26BA4":
                    NextPage();
                    break;
                case "A271A4":
                    HandleNewSongAnnouncements();
                    break;
                case "A273A4":
                    HandleHotSongAnnouncements();
                    break;
                case "A267A4":
                    SkipToNextSong();
                    ClearDisplay();
                    break;
                case "A269A4":
                    ReplayCurrentSong();
                    break;
                // 原唱
                case "A26CA4":
                    Console.WriteLine("ToggleVocalRemoval Invoked");
                    InvokeAction(() => VideoPlayerForm.Instance.ToggleVocalRemoval());
                    InvokeAction(() => OverlayForm.MainForm.ShowOriginalSongLabel());
                    break;
                // 導唱
                case "A26EA4":
                    InvokeAction(() => VideoPlayerForm.Instance.ToggleVocalRemoval());
                    break;
                case "A26DA4":
                    PauseOrResumeSong();
                    break;
                case "A276A4":
                    ToggleMute();
                    break;
                case "A274A4":
                    HandleArtistAnnouncements();
                    break;
                case "A2B3A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowVolumeUpLabel());
                    break;
                case "A2B4A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowVolumeDownLabel());
                    break;
                case "A2B5A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowMicUpLabel());
                    break;
                case "A2B6A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowMicDownLabel());
                    break;
                case "A2C2A4":
                    InvokeAction(() => OverlayForm.MainForm.HidemicLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowStandardLabel());
                    break;
                case "A2C3A4":
                    InvokeAction(() => OverlayForm.MainForm.HidemicLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowProfessionalLabel());
                    break;
                case "A2C4A4":
                    InvokeAction(() => OverlayForm.MainForm.HidemicLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowSquareLabel());
                    break;
                case "A2C1A4":
                    InvokeAction(() => OverlayForm.MainForm.HidemicLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowSingDownLabel());
                    break;
                case "A2D5A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowBrightLabel());
                    break;
                case "A2D7A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowRomanticLabel());
                    break;
               /* case "A27CA4":
                    InvokeAction(() => OverlayForm.MainForm.ShowMaleKeyLabel());
                    break;
                case "A282A4":
                    InvokeAction(() => OverlayForm.MainForm.ShowFemaleKeyLabel());
                    break;*/
                case "A2D6A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowSoftLabel());
                    break;
                case "A2D8A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowDynamicLabel());
                    break;
                case "A275A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowTintLabel());
                    break;
                case "A283A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyUpLabel("↑升4調"));
                break;
                case "A282A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyUpLabel("↑升3調"));
                    break;
                case "A281A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyUpLabel("↑升2調"));
                    break;
                case "A280A4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyUpLabel("↑升1調"));
                    break;
                case "A27FA4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowStandardKeyLabel());
                    break;
                case "A27EA4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyDownLabel("↓降1調"));
                    break;
                case "A27DA4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyDownLabel("↓降2調"));
                    break;
                case "A27CA4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyDownLabel("↓降3調"));
                    break;
                case "A27BA4":
                    InvokeAction(() => OverlayForm.MainForm.HideAllLabels());
                    InvokeAction(() => OverlayForm.MainForm.ShowKeyDownLabel("↓降4調"));
                    break;
                default:
                    if (Regex.IsMatch(indata, @"^A23\d+A4$")) 
                    {
                        HandleNumberInput(indata);
                    }
                    break;
            }
        }

        void InvokeAction(Action action)
        {
            if (OverlayForm.MainForm.InvokeRequired)
            {
                OverlayForm.MainForm.Invoke(action);
            }
            else
            {
                action();
            }
        }

        
        
        private static void SkipToNextSong()
        {
            if (PrimaryForm.Instance.InvokeRequired)
            {
                PrimaryForm.Instance.Invoke(new System.Action(() => PrimaryForm.Instance.videoPlayerForm.SkipToNextSong()));
            }
            else
            {
                PrimaryForm.Instance.videoPlayerForm.SkipToNextSong();
            }

            OverlayForm.MainForm.Invoke(new System.Action(() => 
            {
                OverlayForm.MainForm.ShowStandardLabel();
            }));
        }


        private static void ReplayCurrentSong()
        {
            if (PrimaryForm.Instance.InvokeRequired)
            {
                PrimaryForm.Instance.Invoke(new System.Action(() => PrimaryForm.Instance.videoPlayerForm.ReplayCurrentSong()));
            }
            else
            {
                PrimaryForm.Instance.videoPlayerForm.ReplayCurrentSong();
            }
        }

        private static void PauseOrResumeSong()
        {
            if (PrimaryForm.Instance.InvokeRequired)
            {
                PrimaryForm.Instance.Invoke(new System.Action(() => PrimaryForm.Instance.videoPlayerForm.PauseOrResumeSong()));
            }
            else
            {
                PrimaryForm.Instance.videoPlayerForm.PauseOrResumeSong();
            }
        }

        public static void ToggleMute()
        {
            if (VideoPlayerForm.Instance.InvokeRequired)
            {
                VideoPlayerForm.Instance.Invoke(new System.Action(ToggleMute));
            }
            else
            {
                if (VideoPlayerForm.Instance.isMuted)
                {
                    
                    VideoPlayerForm.Instance.SetVolume(VideoPlayerForm.Instance.previousVolume);
                    
                    VideoPlayerForm.Instance.isMuted = false;
                    OverlayForm.MainForm.Invoke(new System.Action(() => OverlayForm.MainForm.HideMuteLabel())); 
                }
                else
                {
                    
                    VideoPlayerForm.Instance.previousVolume = VideoPlayerForm.Instance.GetVolume();
                    VideoPlayerForm.Instance.SetVolume(-10000);
                    
                    VideoPlayerForm.Instance.isMuted = true;
                    OverlayForm.MainForm.Invoke(new System.Action(() => OverlayForm.MainForm.ShowMuteLabel())); 
                }
            }
        }

        
        private void HandleInputA()
        {
            
            
            OverlayForm.displayTimer.Stop();
            string input = "a"; 

            
            string songNumber = OverlayForm.ReadSongNumber(); 
            var song = songListManager.SearchSongByNumber(songNumber); 
            
            
            
            if (readyForSongListInput)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        OverlayForm.MainForm.OnUserInput(input);  
                    }));
                }
                else
                {
                    OverlayForm.MainForm.OnUserInput(input);
                }
            }
            else
            {
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        if (song != null)
                        {
                            ClearDisplay();
                            OverlayForm.MainForm.displayLabel.Text = String.Format("已點歌曲:{0}", song);
                            OverlayForm.MainForm.AddSongToPlaylist(song);
                            OverlayForm.MainForm.nextSongLabel.Visible = false;
                            OverlayForm.displayTimer.Start();
                        }
                        else
                        {
                            ClearDisplay();
                            OverlayForm.MainForm.displayLabel.Text = "輸入錯誤!!!";
                            OverlayForm.MainForm.nextSongLabel.Visible = false;
                            OverlayForm.displayTimer.Start();
                        }
                    }));
                }
                else
                {
                    if (song != null)
                    {
                        ClearDisplay();
                        OverlayForm.MainForm.displayLabel.Text = String.Format("{0}", song);
                        OverlayForm.MainForm.AddSongToPlaylist(song);
                        OverlayForm.MainForm.nextSongLabel.Visible = false;
                        OverlayForm.displayTimer.Start();
                    }
                    else
                    {
                        ClearDisplay();
                        OverlayForm.MainForm.displayLabel.Text = "輸入錯誤!!!";
                        OverlayForm.MainForm.nextSongLabel.Visible = false;
                        OverlayForm.displayTimer.Start();
                    }
                }
            }
        }

        
        private void HandleInputB()
        {
            
            OverlayForm.displayTimer.Stop();
            string input = "b"; 

            
            string songNumber = OverlayForm.ReadSongNumber(); 
            var song = songListManager.SearchSongByNumber(songNumber); 
            
            
            
            if (readyForSongListInput)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        OverlayForm.MainForm.OnUserInput(input);  
                    }));
                }
                else
                {
                    OverlayForm.MainForm.OnUserInput(input);
                }
            }
            else
            {
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        if (song != null)
                        {
                            ClearDisplay();
                            OverlayForm.MainForm.displayLabel.Text = String.Format("插播歌曲{0}", song);
                            OverlayForm.MainForm.InsertSongToPlaylist(song);
                            OverlayForm.MainForm.nextSongLabel.Visible = false;
                            OverlayForm.displayTimer.Start();
                        }
                        else
                        {
                            ClearDisplay();
                            OverlayForm.MainForm.displayLabel.Text = "輸入錯誤!!!";
                            OverlayForm.MainForm.nextSongLabel.Visible = false;
                            OverlayForm.displayTimer.Start();
                        }
                    }));
                }
                else
                {
                    if (song != null)
                    {
                        ClearDisplay();
                        OverlayForm.MainForm.displayLabel.Text = String.Format("已點歌曲:{0}", song);
                        OverlayForm.MainForm.nextSongLabel.Visible = false;
                    }
                    else
                    {
                        ClearDisplay();
                        OverlayForm.MainForm.displayLabel.Text = "輸入錯誤!!!";
                        OverlayForm.MainForm.nextSongLabel.Visible = false;
                        OverlayForm.displayTimer.Start();
                    }
                }
            }
        }

        private static void ClearDisplay()
        {
            
            OverlayForm.displayTimer.Stop();
            
            
            if (OverlayForm.MainForm.InvokeRequired)
            {
                OverlayForm.MainForm.Invoke(new System.Action(() =>
                {
                    
                    foreach (var control in OverlayForm.MainForm.Controls.OfType<Control>().ToArray())
                    {
                        if (control != OverlayForm.MainForm.displayLabel &&
                            control != OverlayForm.MainForm.pauseLabel &&
                            control != OverlayForm.MainForm.muteLabel &&
                            control != OverlayForm.MainForm.volumeUpLabel &&
                            control != OverlayForm.MainForm.volumeDownLabel &&
                            control != OverlayForm.MainForm.micUpLabel &&
                            control != OverlayForm.MainForm.micDownLabel &&
                            control != OverlayForm.MainForm.standardKeyLabel &&
                            control != OverlayForm.MainForm.keyUpLabel &&
                            control != OverlayForm.MainForm.keyDownLabel &&
                            control != OverlayForm.MainForm.maleKeyLabel &&
                            control != OverlayForm.MainForm.femaleKeyLabel &&
                            control != OverlayForm.MainForm.squareLabel &&
                            control != OverlayForm.MainForm.professionalLabel &&
                            control != OverlayForm.MainForm.standardLabel &&
                            control != OverlayForm.MainForm.singDownLabel &&
                            control != OverlayForm.MainForm.brightLabel &&
                            control != OverlayForm.MainForm.softLabel &&
                            control != OverlayForm.MainForm.autoLabel &&
                            control != OverlayForm.MainForm.romanticLabel &&
                            control != OverlayForm.MainForm.dynamicLabel &&
                            control != OverlayForm.MainForm.tintLabel &&
                            control != OverlayForm.MainForm.blackBackgroundPanel &&
                            control != OverlayForm.MainForm.nextSongLabel) 
                        {
                            OverlayForm.MainForm.Controls.Remove(control);
                            control.Dispose();
                        }
                    }

                    OverlayForm.MainForm.displayLabel.Text = ""; 
                    readyForSongListInput = false;
                    OverlayForm.SetUIState(OverlayForm.UIState.Initial);
                    Console.WriteLine(OverlayForm.MainForm.displayLabel.Text);
                }));
            }
            else
            {
                
                foreach (var control in OverlayForm.MainForm.Controls.OfType<Control>().ToArray())
                {
                    if (control != OverlayForm.MainForm.displayLabel &&
                        control != OverlayForm.MainForm.pauseLabel &&
                        control != OverlayForm.MainForm.muteLabel &&
                        control != OverlayForm.MainForm.volumeUpLabel &&
                        control != OverlayForm.MainForm.volumeDownLabel &&
                        control != OverlayForm.MainForm.micUpLabel &&
                        control != OverlayForm.MainForm.micDownLabel &&
                        control != OverlayForm.MainForm.standardKeyLabel &&
                        control != OverlayForm.MainForm.keyUpLabel &&
                        control != OverlayForm.MainForm.keyDownLabel &&
                        control != OverlayForm.MainForm.maleKeyLabel &&
                        control != OverlayForm.MainForm.femaleKeyLabel &&
                        control != OverlayForm.MainForm.squareLabel &&
                        control != OverlayForm.MainForm.professionalLabel &&
                        control != OverlayForm.MainForm.standardLabel &&
                        control != OverlayForm.MainForm.singDownLabel &&
                        control != OverlayForm.MainForm.brightLabel &&
                        control != OverlayForm.MainForm.softLabel &&
                        control != OverlayForm.MainForm.autoLabel &&
                        control != OverlayForm.MainForm.romanticLabel &&
                        control != OverlayForm.MainForm.dynamicLabel &&
                        control != OverlayForm.MainForm.tintLabel &&
                        control != OverlayForm.MainForm.blackBackgroundPanel &&
                        control != OverlayForm.MainForm.nextSongLabel)
                    {
                        OverlayForm.MainForm.Controls.Remove(control);
                        control.Dispose();
                    }
                }

                OverlayForm.MainForm.displayLabel.Text = ""; 
                Console.WriteLine("ClearDisplay called.");
                readyForSongListInput = false;
                OverlayForm.SetUIState(OverlayForm.UIState.Initial);
                Console.WriteLine(OverlayForm.MainForm.displayLabel.Text);
            }
        }
private static void DisplaySongHistory()
{
    ClearDisplay();

    // 設定總歌曲數量
    OverlayForm.MainForm.totalSongs = PrimaryForm.playedSongsHistory.Count;

    // 如果播放歷史為空
    if (OverlayForm.MainForm.totalSongs == 0)
    {
        Console.WriteLine("No song history available.");
        return;
    }

    // 計算總頁數
    int totalPages = (int)Math.Ceiling(OverlayForm.MainForm.totalSongs / (double)OverlayForm.MainForm.songsPerPage);
    int startIndex = (OverlayForm.MainForm.currentPage - 1) * OverlayForm.MainForm.songsPerPage;
    int endIndex = Math.Min(startIndex + OverlayForm.MainForm.songsPerPage, OverlayForm.MainForm.totalSongs);

    // 準備傳遞給 UpdateHistoryLabel 的數據
    List<SongData> historySongs = new List<SongData>();
    List<PlayState> playStates = new List<PlayState>();

    for (int i = startIndex; i < endIndex; i++)
    {
        historySongs.Add(PrimaryForm.playedSongsHistory[i]);
        playStates.Add(PrimaryForm.playStates[i]);
    }

    // 調用 UpdateHistoryLabel 顯示數據
    if (OverlayForm.MainForm.InvokeRequired)
    {
        OverlayForm.MainForm.Invoke(new System.Action(() =>
        {
            OverlayForm.MainForm.UpdateHistoryLabel(historySongs, playStates, OverlayForm.MainForm.currentPage, totalPages);
            OverlayForm.MainForm.nextSongLabel.Visible = false;
        }));
    }
    else
    {
        OverlayForm.MainForm.UpdateHistoryLabel(historySongs, playStates, OverlayForm.MainForm.currentPage, totalPages);
        OverlayForm.MainForm.nextSongLabel.Visible = false;
    }

    // 設定 UI 狀態
    OverlayForm.SetUIState(OverlayForm.UIState.PlayHistory);
}



        private static void PreviousPage()
        {
            if (OverlayForm.CurrentUIState == OverlayForm.UIState.SelectingSong)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        
                        OverlayForm.MainForm.PreviousPage();
                    }));
                }
                else
                {
                    
                    OverlayForm.MainForm.PreviousPage();
                }
            }
            else if (OverlayForm.CurrentUIState == OverlayForm.UIState.SelectingArtist)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        
                        OverlayForm.MainForm.PreviousPage();
                    }));
                }
                else
                {
                    
                    OverlayForm.MainForm.PreviousPage();
                }
            }
            else if (OverlayForm.CurrentUIState == OverlayForm.UIState.PlayHistory)
            {
                if (OverlayForm.MainForm.currentPage > 1)
                {    
                    OverlayForm.MainForm.currentPage--;

                    if (OverlayForm.MainForm.InvokeRequired)
                    {
                        OverlayForm.MainForm.Invoke(new System.Action(() =>
                        {
                            DisplaySongHistory();
                        }));
                    }
                    else
                    {
                        DisplaySongHistory();
                    }
                }
            }
            else
            {
                Console.WriteLine("Page turning is not allowed in the current state.");
            }
        }

        private static void NextPage()
        {
            if (OverlayForm.CurrentUIState == OverlayForm.UIState.SelectingSong)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        
                        OverlayForm.MainForm.NextPage();
                    }));
                }
                else
                {
                    
                    OverlayForm.MainForm.NextPage();
                }
            }
            else if (OverlayForm.CurrentUIState == OverlayForm.UIState.SelectingArtist)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        
                        OverlayForm.MainForm.NextPage();
                    }));
                }
                else
                {
                    
                    OverlayForm.MainForm.NextPage();
                }
            }
            else if (OverlayForm.CurrentUIState == OverlayForm.UIState.PlayHistory)
            {
                if (OverlayForm.MainForm.currentPage * OverlayForm.MainForm.songsPerPage < OverlayForm.MainForm.totalSongs)
                {    
                    OverlayForm.MainForm.currentPage++;
                    if (OverlayForm.CurrentUIState == OverlayForm.UIState.PlayHistory)
                    {
                        if (OverlayForm.MainForm.InvokeRequired)
                        {
                            OverlayForm.MainForm.Invoke(new System.Action(() =>
                            {
                                DisplaySongHistory();
                            }));
                        }
                        else
                        {
                            DisplaySongHistory();
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Page turning is not allowed in the current state.");
            }
        }

        private static void HandleNewSongAnnouncements()
        {
            ClearDisplay();

            
            OverlayForm.CurrentCategory = OverlayForm.Category.NewSongs;

            
            string[] messages = new string[]
            {
                "新歌快訊",
                "1. 國語",
                "2. 台語",
                "3. 粵語",
                "4. 英語",
                "5. 日語",
                "6. 韓語",
            };

            
            readyForSongListInput = true;

            
            OverlayForm.SetUIState(OverlayForm.UIState.SelectingLanguage);

            
            UpdateDisplayLabels(messages);
        }

        private static void HandleHotSongAnnouncements()
        {
            ClearDisplay();

            
            OverlayForm.CurrentCategory = OverlayForm.Category.HotSongs;

            
            string[] messages = new string[]
            {
                "熱門排行",
                "1. 國語",
                "2. 台語",
                "3. 粵語",
                "4. 英語",
                "5. 日語",
                "6. 韓語",
            };

            
            readyForSongListInput = true;

            
            OverlayForm.SetUIState(OverlayForm.UIState.SelectingLanguage);

            
            UpdateDisplayLabels(messages);
        }

        private static void HandleArtistAnnouncements()
        {
            ClearDisplay();

            
            OverlayForm.CurrentCategory = OverlayForm.Category.Artists;

            
            string[] messages = new string[]
            {
                "歌星選歌",
                "1.男歌星",
                "2.女歌星",
                "3.團體",
                "4.外語",
                "5.全部",
            };

            
            readyForSongListInput = true;

            
            OverlayForm.SetUIState(OverlayForm.UIState.SelectingArtistCategory);

            
            UpdateDisplayLabels(messages);
        }

        private static void UpdateDisplayLabels(string[] messages)
        {
            if (OverlayForm.MainForm.InvokeRequired)
            {
                OverlayForm.MainForm.Invoke(new System.Action(() =>
                {
                    
                    foreach (var control in OverlayForm.MainForm.Controls.OfType<Control>().ToArray())
                    {
                        if (control != OverlayForm.MainForm.displayLabel &&
                            control != OverlayForm.MainForm.pauseLabel &&
                            control != OverlayForm.MainForm.muteLabel &&
                            control != OverlayForm.MainForm.volumeUpLabel &&
                            control != OverlayForm.MainForm.volumeDownLabel &&
                            control != OverlayForm.MainForm.micUpLabel &&
                            control != OverlayForm.MainForm.micDownLabel &&
                            control != OverlayForm.MainForm.standardKeyLabel &&
                            control != OverlayForm.MainForm.keyUpLabel &&
                            control != OverlayForm.MainForm.keyDownLabel &&
                            control != OverlayForm.MainForm.maleKeyLabel &&
                            control != OverlayForm.MainForm.femaleKeyLabel &&
                            control != OverlayForm.MainForm.squareLabel &&
                            control != OverlayForm.MainForm.professionalLabel &&
                            control != OverlayForm.MainForm.standardLabel &&
                            control != OverlayForm.MainForm.singDownLabel &&
                            control != OverlayForm.MainForm.brightLabel &&
                            control != OverlayForm.MainForm.softLabel &&
                            control != OverlayForm.MainForm.autoLabel &&
                            control != OverlayForm.MainForm.romanticLabel &&
                            control != OverlayForm.MainForm.dynamicLabel &&
                            control != OverlayForm.MainForm.tintLabel &&
                            control != OverlayForm.MainForm.blackBackgroundPanel &&
                            control != OverlayForm.MainForm.nextSongLabel) 
                        {
                            OverlayForm.MainForm.Controls.Remove(control);
                            control.Dispose();
                        }
                    }
                    OverlayForm.MainForm.UpdateDisplayLabels(messages);
                }));
            }
            else
            {
                
                foreach (var control in OverlayForm.MainForm.Controls.OfType<Control>().ToArray())
                {
                    if (control != OverlayForm.MainForm.displayLabel &&
                        control != OverlayForm.MainForm.pauseLabel &&
                        control != OverlayForm.MainForm.muteLabel &&
                        control != OverlayForm.MainForm.volumeUpLabel &&
                        control != OverlayForm.MainForm.volumeDownLabel &&
                        control != OverlayForm.MainForm.micUpLabel &&
                        control != OverlayForm.MainForm.micDownLabel &&
                        control != OverlayForm.MainForm.standardKeyLabel &&
                        control != OverlayForm.MainForm.keyUpLabel &&
                        control != OverlayForm.MainForm.keyDownLabel &&
                        control != OverlayForm.MainForm.maleKeyLabel &&
                        control != OverlayForm.MainForm.femaleKeyLabel &&
                        control != OverlayForm.MainForm.squareLabel &&
                        control != OverlayForm.MainForm.professionalLabel &&
                        control != OverlayForm.MainForm.standardLabel &&
                        control != OverlayForm.MainForm.singDownLabel &&
                        control != OverlayForm.MainForm.brightLabel &&
                        control != OverlayForm.MainForm.softLabel &&
                        control != OverlayForm.MainForm.autoLabel &&
                        control != OverlayForm.MainForm.romanticLabel &&
                        control != OverlayForm.MainForm.dynamicLabel &&
                        control != OverlayForm.MainForm.tintLabel &&
                        control != OverlayForm.MainForm.blackBackgroundPanel &&
                        control != OverlayForm.MainForm.nextSongLabel)
                    {
                        OverlayForm.MainForm.Controls.Remove(control);
                        control.Dispose();
                    }
                }
                OverlayForm.MainForm.UpdateDisplayLabels(messages);
            }
        }

        private static void HandleNumberInput(string trimmedData)
        {
            string number = trimmedData; 

            
            var match = Regex.Match(trimmedData, @"^A23(\d)A4$");
            if (match.Success)
            {
                number = match.Groups[1].Value;
                
                Console.WriteLine($"Handling number: {number}");
                
            }

            
            if (readyForSongListInput)
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        OverlayForm.MainForm.OnUserInput(number);  
                    }));
                }
                else
                {
                    OverlayForm.MainForm.OnUserInput(number);
                }
                
            }
            else
            {
                
                if (OverlayForm.MainForm.InvokeRequired)
                {
                    OverlayForm.MainForm.Invoke(new System.Action(() =>
                    {
                        OverlayForm.DisplayNumberAtTopLeft(number);  
                        OverlayForm.MainForm.HideAllLabels(); 
                    }));
                }
                else
                {
                    OverlayForm.DisplayNumberAtTopLeft(number);
                    OverlayForm.MainForm.HideAllLabels(); 
                }
            }
        }

        public static bool CheckLogForShutdown(string filePath)
        {
            if (File.Exists(filePath))
            {
                
                string content = File.ReadAllText(filePath).Replace(Environment.NewLine, "");
                if (content.Length >= 6 && content.Substring(content.Length - 6) == "bbbaaa")
                {
                    return true;
                }
            }
            return false;
        }

        public static void ShutdownComputer()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c shutdown /s /f /t 0") 
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                Console.WriteLine("Computer is shutting down...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error shutting down computer: " + ex.Message);
            }
        }
    }
}