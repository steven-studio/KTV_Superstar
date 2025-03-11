using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO; 
using System.Linq;
using System.Collections.Generic; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button hotPlayButton;
        private Bitmap hotPlayNormalBackground; 
        private Bitmap hotPlayActiveBackground; 

        private Button guoYuButtonHotSong;
        private Bitmap guoYuHotSongNormalBackground;
        private Bitmap guoYuHotSongActiveBackground;
        private Button taiYuButtonHotSong;
        private Bitmap taiYuHotSongNormalBackground;
        private Bitmap taiYuHotSongActiveBackground;
        private Button taiYuNewSongButtonHotSong;
        private Bitmap taiYuNewSongHotSongNormalBackground;
        private Bitmap taiYuNewSongHotSongActiveBackground;
        private Button guoYuNewSongButtonHotSong;
        private Bitmap guoYuNewSongHotSongNormalBackground;
        private Bitmap guoYuNewSongHotSongActiveBackground;
        private Button yueYuButtonHotSong;
        private Bitmap yueYuHotSongNormalBackground;
        private Bitmap yueYuHotSongActiveBackground;
        private Button yingWenButtonHotSong;
        private Bitmap yingWenHotSongNormalBackground;
        private Bitmap yingWenHotSongActiveBackground;
        private Button riYuButtonHotSong;
        private Bitmap riYuHotSongNormalBackground;
        private Bitmap riYuHotSongActiveBackground;
        private Button hanYuButtonHotSong;
        private Bitmap hanYuHotSongNormalBackground;
        private Bitmap hanYuHotSongActiveBackground;


        private void SetHotSongButtonsVisibility(bool isVisible)
        {
            
            

            
            Button[] hotSongButtons = { guoYuButtonHotSong, taiYuButtonHotSong, taiYuNewSongButtonHotSong, guoYuNewSongButtonHotSong, yingWenButtonHotSong, riYuButtonHotSong, hanYuButtonHotSong };

            
            foreach (var button in hotSongButtons)
            {
                button.Visible = isVisible;

                
                if (isVisible)
                {
                    button.BringToFront();
                }
            }
        }

        private void HotPlayButton_Click(object sender, EventArgs e)
        {
            UpdateButtonBackgrounds(hotPlayButton, hotPlayActiveBackground);
            UpdateHotSongButtons(guoYuButtonHotSong, guoYuHotSongActiveBackground);
            isOnOrderedSongsPage = false;

            int songLimit = ReadHotSongLimit(); 

            guoYuSongs = GetSongsByCategory("國語", songLimit);
            UpdateSongList(guoYuSongs);

            SetButtonsVisibility();
            HideQRCode();
        }

        private void UpdateButtonBackgrounds(Button activeButton, Image activeBackground)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchNormalBackground;
            languageSearchButton.BackgroundImage = languageSearchNormalBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
            myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;

            activeButton.BackgroundImage = activeBackground;
        }

        private void OnHotSongButtonClick(Button activeButton, Bitmap activeBackground, string category)
        {
            UpdateHotSongButtons(activeButton, activeBackground);

            int songLimit = ReadHotSongLimit();

            var selectedSongs = allSongs.Where(song => song.Category == category)
                                        .OrderByDescending(song => song.Plays)
                                        .Take(songLimit)
                                        .ToList();

            UpdateSongList(selectedSongs);
        }

        private void UpdateHotSongButtons(Button activeButton, Image activeBackground)
        {
            guoYuButtonHotSong.BackgroundImage = guoYuHotSongNormalBackground;
            taiYuButtonHotSong.BackgroundImage = taiYuHotSongNormalBackground;
            taiYuNewSongButtonHotSong.BackgroundImage = taiYuNewSongHotSongNormalBackground;
            guoYuNewSongButtonHotSong.BackgroundImage = guoYuNewSongHotSongNormalBackground;
            yingWenButtonHotSong.BackgroundImage = yingWenHotSongNormalBackground;
            riYuButtonHotSong.BackgroundImage = riYuHotSongNormalBackground;
            hanYuButtonHotSong.BackgroundImage = hanYuHotSongNormalBackground;

            activeButton.BackgroundImage = activeBackground;
        }

        private List<SongData> GetSongsByCategory(string category, int limit)
        {
            return allSongs.Where(song => song.Category == category)
                        .OrderByDescending(song => song.Plays) 
                        .Take(limit) 
                        .ToList();
        }

        private void UpdateSongList(List<SongData> songs)
        {
            currentPage = 0; 
            currentSongList = songs; 
            totalPages = (int)Math.Ceiling((double)songs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }

        private void SetButtonsVisibility()
        {
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetGroupButtonsVisibility(false);
            SetPictureBoxCategoryAndButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetPictureBoxToggleLightAndButtonsVisibility(false);
            SetPictureBoxSceneSoundEffectsAndButtonsVisibility(false);
            SetHotSongButtonsVisibility(true);
        }

        private void HideQRCode()
        {
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }

        private void InitializeButtonsForHotSong()
        {   

            InitializeHotSongButton(ref guoYuNewSongButtonHotSong, "國語新歌", 1214, 230, 209, 58, 
                normalStateImageHotSong, 
                out guoYuNewSongHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out guoYuNewSongHotSongActiveBackground, 
                GuoYuNewSongButtonHotSong_Click);

            InitializeHotSongButton(ref taiYuNewSongButtonHotSong, "台語新歌", 1214, 293, 209, 58, 
                normalStateImageHotSong, 
                out taiYuNewSongHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out taiYuNewSongHotSongActiveBackground, 
                TaiYuNewSongButtonHotSong_Click);


            InitializeHotSongButton(ref taiYuButtonHotSong, "台語", 1214, 418, 209, 58, 
                normalStateImageHotSong, 
                out taiYuHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out taiYuHotSongActiveBackground, 
                TaiYuButtonHotSong_Click);


            InitializeHotSongButton(ref yueYuButtonHotSong, "粵語", 1214, 356, 209, 58, 
                normalStateImageHotSong, 
                out yueYuHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out yueYuHotSongActiveBackground, 
                YueYuButtonHotSong_Click);

            InitializeHotSongButton(ref guoYuButtonHotSong, "國語", 1214, 356, 209, 59, 
                normalStateImageHotSong, 
                out guoYuHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out guoYuHotSongActiveBackground, 
                GuoYuButtonHotSong_Click);

            InitializeHotSongButton(ref yingWenButtonHotSong, "英文", 1214, 481, 209, 59, 
                normalStateImageHotSong, 
                out yingWenHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out yingWenHotSongActiveBackground, 
                YingWenButtonHotSong_Click);

            InitializeHotSongButton(ref riYuButtonHotSong, "日語", 1214, 544, 209, 59, 
                normalStateImageHotSong, 
                out riYuHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out riYuHotSongActiveBackground, 
                RiYuButtonHotSong_Click);

            InitializeHotSongButton(ref hanYuButtonHotSong, "韓語", 1214, 607, 209, 58, 
                normalStateImageHotSong, 
                out hanYuHotSongNormalBackground, 
                mouseDownImageHotSong, 
                out hanYuHotSongActiveBackground, 
                HanYuButtonHotSong_Click);

        }

        private void InitializeHotSongButton(ref Button button, string buttonText, int x, int y, int width, int height, 
                                            Image normalBackground, out Bitmap normalBackgroundOut, 
                                            Image activeBackground, out Bitmap activeBackgroundOut, 
                                            EventHandler clickEventHandler)
        {
            button = new Button { 
                Text = "",  // 移除文字
                Visible = false 
            };
            ResizeAndPositionButton(button, x, y, width, height);

            // 修改裁剪區域，避開文字部分
            Rectangle cropArea = new Rectangle(1214, y, 209, 58);  // 使用固定的裁剪區域
            
            normalBackgroundOut = new Bitmap(normalBackground).Clone(cropArea, normalBackground.PixelFormat);
            activeBackgroundOut = new Bitmap(activeBackground).Clone(cropArea, activeBackground.PixelFormat);
            
            button.BackgroundImage = normalBackgroundOut;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickEventHandler;
            this.Controls.Add(button);
        }

        public static int ReadHotSongLimit()
        {
            string filePath = Path.Combine(Application.StartupPath, "SongLimitsSettings.txt"); 
            try
            {
                
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    
                    if (line.StartsWith("HotSongLimit:"))
                    {
                        string valuePart = line.Split(':')[1].Trim(); 
                        int limit; 
                        if (int.TryParse(valuePart, out limit))
                        {
                            return limit; 
                        }
                        break; 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read song limits from file: " + ex.Message);
                return 100; 
            }

            return 100; 
        }
    }
}