using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO; 
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button newSongAlertButton;
        
        private Bitmap newSongAlertNormalBackground; 
        private Bitmap newSongAlertActiveBackground; 

        private Button guoYuButtonNewSong;
        private Bitmap guoYuNewSongNormalBackground; 
        private Bitmap guoYuNewSongActiveBackground;
        private Button taiYuButtonNewSong;
        private Bitmap taiYuNewSongNormalBackground; 
        private Bitmap taiYuNewSongActiveBackground;
        private Button yueYuButtonNewSong;
        private Bitmap yueYuNewSongNormalBackground; 
        private Bitmap yueYuNewSongActiveBackground;
        private Button yingWenButtonNewSong;
        private Bitmap yingWenNewSongNormalBackground; 
        private Bitmap yingWenNewSongActiveBackground;
        private Button riYuButtonNewSong;
        private Bitmap riYuNewSongNormalBackground; 
        private Bitmap riYuNewSongActiveBackground;
        private Button hanYuButtonNewSong;
        private Bitmap hanYuNewSongNormalBackground; 
        private Bitmap hanYuNewSongActiveBackground;

        private void ToggleNewSongButtonsVisibility()
        {
            
            bool areButtonsVisible = guoYuButtonNewSong.Visible; 

            
            SetNewSongButtonsVisibility(!areButtonsVisible);
        }

        private void SetNewSongButtonsVisibility(bool isVisible)
        {
            
            

            
            Button[] pictureBox2Buttons = { guoYuButtonNewSong, taiYuButtonNewSong, yueYuButtonNewSong, yingWenButtonNewSong, riYuButtonNewSong, hanYuButtonNewSong };

            
            foreach (var button in pictureBox2Buttons)
            {
                button.Visible = isVisible;

                
                if (isVisible)
                {
                    button.BringToFront();
                }
            }
        }

        private void NewSongAlertButton_Click(object sender, EventArgs e)
        {
            
            newSongAlertButton.BackgroundImage = newSongAlertActiveBackground;
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
            isOnOrderedSongsPage = false;

            guoYuButtonNewSong.BackgroundImage = guoYuNewSongActiveBackground;
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongNormalBackground;
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongNormalBackground;
            riYuButtonNewSong.BackgroundImage = riYuNewSongNormalBackground;
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongNormalBackground;

            int songLimit = ReadNewSongLimit(); 

            guoYuSongs2 = allSongs.Where(song => song.Category == "國語")
                                .OrderByDescending(song => song.AddedTime) 
                                .Take(songLimit) 
                                .ToList();
            currentPage = 0; 
            currentSongList = guoYuSongs2; 
            totalPages = (int)Math.Ceiling((double)guoYuSongs2.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);

            
            SetHotSongButtonsVisibility(false);
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
            ToggleNewSongButtonsVisibility();

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }

        private void InitializeButtonsForNewSong()
        {
            
            
            guoYuButtonNewSong = new Button{ Text = "", Visible = false };
            
            ResizeAndPositionButton(guoYuButtonNewSong, 1214, 230, 209, 59);
            Rectangle guoYuNewSongButtonCropArea = new Rectangle(1214, 230, 209, 59); 
            guoYuNewSongNormalBackground = normalStateImageNewSongAlert.Clone(guoYuNewSongButtonCropArea, normalStateImageNewSongAlert.PixelFormat);
            guoYuNewSongActiveBackground = mouseDownImageNewSongAlert.Clone(guoYuNewSongButtonCropArea, mouseDownImageNewSongAlert.PixelFormat);
            guoYuButtonNewSong.BackgroundImage = guoYuNewSongNormalBackground;
            guoYuButtonNewSong.BackgroundImageLayout = ImageLayout.Stretch;
            guoYuButtonNewSong.FlatStyle = FlatStyle.Flat;
            guoYuButtonNewSong.FlatAppearance.BorderSize = 0; 
            
            guoYuButtonNewSong.Click += GuoYuButtonNewSong_Click;
            
            this.Controls.Add(guoYuButtonNewSong);

            
            taiYuButtonNewSong = new Button { Text = "", Visible = false };
            
            ResizeAndPositionButton(taiYuButtonNewSong, 1214, 293, 209, 58);
            Rectangle taiYuNewSongButtonCropArea = new Rectangle(1214, 293, 209, 58); 
            taiYuNewSongNormalBackground = normalStateImageNewSongAlert.Clone(taiYuNewSongButtonCropArea, normalStateImageNewSongAlert.PixelFormat);
            taiYuNewSongActiveBackground = mouseDownImageNewSongAlert.Clone(taiYuNewSongButtonCropArea, mouseDownImageNewSongAlert.PixelFormat);
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            taiYuButtonNewSong.BackgroundImageLayout = ImageLayout.Stretch;
            taiYuButtonNewSong.FlatStyle = FlatStyle.Flat;
            taiYuButtonNewSong.FlatAppearance.BorderSize = 0; 
            
            taiYuButtonNewSong.Click += TaiYuButtonNewSong_Click;
            
            this.Controls.Add(taiYuButtonNewSong);

            
            yueYuButtonNewSong = new Button { Text = "", Visible = false };
            
            ResizeAndPositionButton(yueYuButtonNewSong, 1214, 356, 209, 58);
            Rectangle yueYuNewSongButtonCropArea = new Rectangle(1214, 356, 209, 58); 
            yueYuNewSongNormalBackground = normalStateImageNewSongAlert.Clone(yueYuNewSongButtonCropArea, normalStateImageNewSongAlert.PixelFormat);
            yueYuNewSongActiveBackground = mouseDownImageNewSongAlert.Clone(yueYuNewSongButtonCropArea, mouseDownImageNewSongAlert.PixelFormat);
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImageLayout = ImageLayout.Stretch;
            yueYuButtonNewSong.FlatStyle = FlatStyle.Flat;
            yueYuButtonNewSong.FlatAppearance.BorderSize = 0; 
            
            yueYuButtonNewSong.Click += YueYuButtonNewSong_Click;
            
            this.Controls.Add(yueYuButtonNewSong);

            
            yingWenButtonNewSong = new Button { Text = "英文2", Visible = false };
            
            ResizeAndPositionButton(yingWenButtonNewSong, 1214, 418, 209, 59);
            Rectangle yingWenNewSongButtonCropArea = new Rectangle(1214, 418, 209, 59); 
            yingWenNewSongNormalBackground = normalStateImageNewSongAlert.Clone(yingWenNewSongButtonCropArea, normalStateImageNewSongAlert.PixelFormat);
            yingWenNewSongActiveBackground = mouseDownImageNewSongAlert.Clone(yingWenNewSongButtonCropArea, mouseDownImageNewSongAlert.PixelFormat);
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongNormalBackground;
            yingWenButtonNewSong.BackgroundImageLayout = ImageLayout.Stretch;
            yingWenButtonNewSong.FlatStyle = FlatStyle.Flat;
            yingWenButtonNewSong.FlatAppearance.BorderSize = 0; 
            
            yingWenButtonNewSong.Click += YingWenButtonNewSong_Click;
            
            this.Controls.Add(yingWenButtonNewSong);

            
            riYuButtonNewSong = new Button { Text = "日語2", Visible = false };
            
            ResizeAndPositionButton(riYuButtonNewSong, 1214, 481, 209, 59);
            Rectangle riYuNewSongButtonCropArea = new Rectangle(1214, 481, 209, 59); 
            riYuNewSongNormalBackground = normalStateImageNewSongAlert.Clone(riYuNewSongButtonCropArea, normalStateImageNewSongAlert.PixelFormat);
            riYuNewSongActiveBackground = mouseDownImageNewSongAlert.Clone(riYuNewSongButtonCropArea, mouseDownImageNewSongAlert.PixelFormat);
            riYuButtonNewSong.BackgroundImage = riYuNewSongNormalBackground;
            riYuButtonNewSong.BackgroundImageLayout = ImageLayout.Stretch;
            riYuButtonNewSong.FlatStyle = FlatStyle.Flat;
            riYuButtonNewSong.FlatAppearance.BorderSize = 0; 
            
            riYuButtonNewSong.Click += RiYuButtonNewSong_Click;
            
            this.Controls.Add(riYuButtonNewSong);

            
            hanYuButtonNewSong = new Button { Text = "韓語2", Visible = false };
            
            ResizeAndPositionButton(hanYuButtonNewSong, 1214, 544, 209, 58);
            Rectangle hanYuNewSongButtonCropArea = new Rectangle(1214, 544, 209, 58); 
            hanYuNewSongNormalBackground = normalStateImageNewSongAlert.Clone(hanYuNewSongButtonCropArea, normalStateImageNewSongAlert.PixelFormat);
            hanYuNewSongActiveBackground = mouseDownImageNewSongAlert.Clone(hanYuNewSongButtonCropArea, mouseDownImageNewSongAlert.PixelFormat);
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongNormalBackground;
            hanYuButtonNewSong.BackgroundImageLayout = ImageLayout.Stretch;
            hanYuButtonNewSong.FlatStyle = FlatStyle.Flat;
            hanYuButtonNewSong.FlatAppearance.BorderSize = 0; 
            
            hanYuButtonNewSong.Click += HanYuButtonNewSong_Click;
            
            this.Controls.Add(hanYuButtonNewSong);
        }

        public static int ReadNewSongLimit()
        {
            string filePath = Path.Combine(Application.StartupPath, "SongLimitsSettings.txt"); 
            try
            {
                
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    
                    if (line.StartsWith("NewSongLimit:"))
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