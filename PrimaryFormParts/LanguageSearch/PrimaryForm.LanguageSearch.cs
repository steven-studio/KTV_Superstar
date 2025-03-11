using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        
        private void InitializeButton(ref Button button, string buttonText, int x, int y, int width, int height, Rectangle cropArea, Image normalBackground, out Bitmap normalBackgroundOut, Image activeBackground, out Bitmap activeBackgroundOut, EventHandler clickEventHandler)
        {
            button = new Button { Text = buttonText, Visible = false };
            ResizeAndPositionButton(button, x, y, width, height);
            normalBackgroundOut = new Bitmap(normalBackground).Clone(cropArea, normalBackground.PixelFormat);
            activeBackgroundOut = new Bitmap(activeBackground).Clone(cropArea, activeBackground.PixelFormat);
            button.BackgroundImage = normalBackgroundOut;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickEventHandler;
            this.Controls.Add(button);
        }

        private Button languageSearchButton;
        private Bitmap languageSearchNormalBackground; 
        private Bitmap languageSearchActiveBackground; 

        private Button guoYuButton;
        private Bitmap guoYuNormalBackground;
        private Bitmap guoYuActiveBackground;
        private Button taiYuButton;
        private Bitmap taiYuNormalBackground;
        private Bitmap taiYuActiveBackground;
        private Button yueYuButton;
        private Bitmap yueYuNormalBackground;
        private Bitmap yueYuActiveBackground;
        private Button yingWenButton;
        private Bitmap yingWenNormalBackground;
        private Bitmap yingWenActiveBackground;
        private Button riYuButton;
        private Bitmap riYuNormalBackground;
        private Bitmap riYuActiveBackground;
        private Button hanYuButton;
        private Bitmap hanYuNormalBackground;
        private Bitmap hanYuActiveBackground;
        private Button keYuButton;
        private Bitmap keYuNormalBackground;
        private Bitmap keYuActiveBackground;

        private void LanguageSongSelectionButton_Click(object sender, EventArgs e)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchNormalBackground;
            languageSearchButton.BackgroundImage = languageSearchActiveBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
            myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;
            isOnOrderedSongsPage = false;

            guoYuButton.BackgroundImage = guoYuActiveBackground;
            taiYuButton.BackgroundImage = taiYuNormalBackground;
            yueYuButton.BackgroundImage = yueYuNormalBackground;
            yingWenButton.BackgroundImage = yingWenNormalBackground;
            riYuButton.BackgroundImage = riYuNormalBackground;
            hanYuButton.BackgroundImage = hanYuNormalBackground;
            keYuButton.BackgroundImage = keYuNormalBackground;

            guoYuSongs = allSongs.Where(song => song.Category == "國語")
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = guoYuSongs; 
            totalPages = (int)Math.Ceiling((double)guoYuSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);

            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
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
            SetPictureBoxLanguageButtonsVisibility(true);

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }

        private void OnLanguageButtonClick(Button activeButton, Image activeBackground, string category)
        {
            
            guoYuButton.BackgroundImage = guoYuNormalBackground;
            taiYuButton.BackgroundImage = taiYuNormalBackground;
            yueYuButton.BackgroundImage = yueYuNormalBackground;
            yingWenButton.BackgroundImage = yingWenNormalBackground;
            riYuButton.BackgroundImage = riYuNormalBackground;
            hanYuButton.BackgroundImage = hanYuNormalBackground;
            keYuButton.BackgroundImage = keYuNormalBackground;

            
            activeButton.BackgroundImage = activeBackground;

            
            var selectedSongs = allSongs.Where(song => song.Category == category)
                                        .OrderByDescending(song => song.Plays) 
                                        .ToList();
            currentPage = 0; 
            currentSongList = selectedSongs; 
            totalPages = (int)Math.Ceiling((double)selectedSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }

        private void SetPictureBoxLanguageButtonsVisibility(bool isVisible)
        {   
            guoYuButton.Visible = isVisible;
            guoYuButton.BringToFront();
            
            taiYuButton.Visible = isVisible;
            taiYuButton.BringToFront();
            
            yueYuButton.Visible = isVisible;
            yueYuButton.BringToFront();
            
            yingWenButton.Visible = isVisible;
            yingWenButton.BringToFront();
            
            riYuButton.Visible = isVisible;
            riYuButton.BringToFront();
            
            hanYuButton.Visible = isVisible;
            hanYuButton.BringToFront();

            keYuButton.Visible = isVisible;
            keYuButton.BringToFront();
        }

        private void InitializeButtonsForPictureBoxLanguageQuery()
        {
            InitializeGuoYuButton();
            InitializeTaiYuButton();
            InitializeYueYuButton();
            InitializeYingWenButton();
            InitializeRiYuButton();
            InitializeHanYuButton();
            InitializeKeYuButton();
        }
    }
}