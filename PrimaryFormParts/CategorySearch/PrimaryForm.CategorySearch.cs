using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button categorySearchButton;
        private Bitmap categorySearchNormalBackground; 
        private Bitmap categorySearchActiveBackground; 

        private Button loveDuetButton;
        private Bitmap loveDuetNormalBackground;
        private Bitmap loveDuetActiveBackground;
        private Button talentShowButton;
        private Bitmap talentShowNormalBackground;
        private Bitmap talentShowActiveBackground;
        private Button medleyDanceButton;
        private Bitmap medleyDanceNormalBackground;
        private Bitmap medleyDanceActiveBackground;
        private Button ninetiesButton;
        private Bitmap ninetiesNormalBackground;
        private Bitmap ninetiesActiveBackground;
        private Button nostalgicSongsButton;
        private Bitmap nostalgicSongsNormalBackground;
        private Bitmap nostalgicSongsActiveBackground;
        private Button chinaSongsButton;
        private Bitmap chinaNormalBackground;
        private Bitmap chinaActiveBackground;
        private Button vietnameseSongsButton;
        private Bitmap vietnameseNormalBackground;
        private Bitmap vietnameseActiveBackground;
        
        private void InitializeButtonsForCategorySearch()
        {
            categorySearchButton = new Button { Text = "" };
            categorySearchButton.Name = "categorySearchButton";
            ResizeAndPositionButton(categorySearchButton, 731, 97, 99, 99);
            Rectangle categorySearchCropArea = new Rectangle(731, 97, 99, 99);
            categorySearchNormalBackground = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\ICON上方\\上方ICON_類別查詢-07.png"));
            categorySearchActiveBackground = mouseDownImage.Clone(categorySearchCropArea, mouseDownImage.PixelFormat);
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            categorySearchButton.BackgroundImageLayout = ImageLayout.Stretch;
            categorySearchButton.FlatStyle = FlatStyle.Flat;
            categorySearchButton.FlatAppearance.BorderSize = 0; 
            categorySearchButton.Click += CategorySearchButton_Click;
            this.Controls.Add(categorySearchButton);
        }

        private void InitializeCategorySearchButtons()
        {
            
            loveDuetButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(loveDuetButton, 1214, 230, 209, 59);
            Rectangle loveDuetButtonCropArea = new Rectangle(1214, 230, 209, 59);
            loveDuetNormalBackground = normalStateImageCategoryQuery.Clone(loveDuetButtonCropArea, normalStateImageCategoryQuery.PixelFormat);
            loveDuetActiveBackground = mouseDownImageCategoryQuery.Clone(loveDuetButtonCropArea, mouseDownImageCategoryQuery.PixelFormat);
            loveDuetButton.BackgroundImage = loveDuetNormalBackground;
            loveDuetButton.BackgroundImageLayout = ImageLayout.Stretch;
            loveDuetButton.FlatStyle = FlatStyle.Flat;
            loveDuetButton.FlatAppearance.BorderSize = 0;
            loveDuetButton.Click += LoveDuetButton_Click;  
            this.Controls.Add(loveDuetButton);

            
            talentShowButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(talentShowButton, 1214, 293, 209, 58);
            Rectangle talentShowButtonCropArea = new Rectangle(1214, 293, 209, 58);
            talentShowNormalBackground = normalStateImageCategoryQuery.Clone(talentShowButtonCropArea, normalStateImageCategoryQuery.PixelFormat);
            talentShowActiveBackground = mouseDownImageCategoryQuery.Clone(talentShowButtonCropArea, mouseDownImageCategoryQuery.PixelFormat);
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            talentShowButton.BackgroundImageLayout = ImageLayout.Stretch;
            talentShowButton.FlatStyle = FlatStyle.Flat;
            talentShowButton.FlatAppearance.BorderSize = 0;
            talentShowButton.Click += TalentShowButton_Click;  
            this.Controls.Add(talentShowButton);

            
            medleyDanceButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(medleyDanceButton, 1214, 356, 209, 58);
            Rectangle medleyDanceButtonCropArea = new Rectangle(1214, 356, 209, 58);
            medleyDanceNormalBackground = normalStateImageCategoryQuery.Clone(medleyDanceButtonCropArea, normalStateImageCategoryQuery.PixelFormat);
            medleyDanceActiveBackground = mouseDownImageCategoryQuery.Clone(medleyDanceButtonCropArea, mouseDownImageCategoryQuery.PixelFormat);
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            medleyDanceButton.BackgroundImageLayout = ImageLayout.Stretch;
            medleyDanceButton.FlatStyle = FlatStyle.Flat;
            medleyDanceButton.FlatAppearance.BorderSize = 0;
            medleyDanceButton.Click += MedleyDanceButton_Click;  
            this.Controls.Add(medleyDanceButton);

            
            ninetiesButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(ninetiesButton, 1214, 418, 209, 59);
            Rectangle ninetiesButtonCropArea = new Rectangle(1214, 418, 209, 59);
            ninetiesNormalBackground = normalStateImageCategoryQuery.Clone(ninetiesButtonCropArea, normalStateImageCategoryQuery.PixelFormat);
            ninetiesActiveBackground = mouseDownImageCategoryQuery.Clone(ninetiesButtonCropArea, mouseDownImageCategoryQuery.PixelFormat);
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            ninetiesButton.BackgroundImageLayout = ImageLayout.Stretch;
            ninetiesButton.FlatStyle = FlatStyle.Flat;
            ninetiesButton.FlatAppearance.BorderSize = 0;
            ninetiesButton.Click += NinetiesButton_Click;  
            this.Controls.Add(ninetiesButton);

            
            nostalgicSongsButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(nostalgicSongsButton, 1214, 481, 209, 59);
            Rectangle nostalgicSongsButtonCropArea = new Rectangle(1214, 481, 209, 59);
            nostalgicSongsNormalBackground = normalStateImageCategoryQuery.Clone(nostalgicSongsButtonCropArea, normalStateImageCategoryQuery.PixelFormat);
            nostalgicSongsActiveBackground = mouseDownImageCategoryQuery.Clone(nostalgicSongsButtonCropArea, mouseDownImageCategoryQuery.PixelFormat);
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            nostalgicSongsButton.BackgroundImageLayout = ImageLayout.Stretch;
            nostalgicSongsButton.FlatStyle = FlatStyle.Flat;
            nostalgicSongsButton.FlatAppearance.BorderSize = 0;
            nostalgicSongsButton.Click += NostalgicSongsButton_Click;  
            this.Controls.Add(nostalgicSongsButton);

            
            chinaSongsButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(chinaSongsButton, 1214, 544, 209, 58);
            Rectangle chinaCropArea = new Rectangle(1214, 544, 209, 58); 
            
            chinaNormalBackground = normalStateImageCategoryQuery.Clone(chinaCropArea, normalStateImageCategoryQuery.PixelFormat);
            chinaActiveBackground = mouseDownImageCategoryQuery.Clone(chinaCropArea, mouseDownImageCategoryQuery.PixelFormat);
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            chinaSongsButton.BackgroundImageLayout = ImageLayout.Stretch;
            chinaSongsButton.FlatStyle = FlatStyle.Flat;
            chinaSongsButton.FlatAppearance.BorderSize = 0; 
            chinaSongsButton.Click += ChinaSongsButton_Click; 
            this.Controls.Add(chinaSongsButton);

            vietnameseSongsButton = new Button { Text = "", Visible = false };
            ResizeAndPositionButton(vietnameseSongsButton, 1214, 607, 209, 58);
            Rectangle vietnameseCropArea = new Rectangle(1214, 607, 209, 58);
            
            vietnameseNormalBackground = normalStateImageCategoryQuery.Clone(vietnameseCropArea, normalStateImageCategoryQuery.PixelFormat);
            vietnameseActiveBackground = mouseDownImageCategoryQuery.Clone(vietnameseCropArea, mouseDownImageCategoryQuery.PixelFormat);
            vietnameseSongsButton.BackgroundImage = vietnameseNormalBackground;
            vietnameseSongsButton.BackgroundImageLayout = ImageLayout.Stretch;
            vietnameseSongsButton.FlatStyle = FlatStyle.Flat;
            vietnameseSongsButton.FlatAppearance.BorderSize = 0;
            vietnameseSongsButton.Click += VietnameseSongsButton_Click;
            this.Controls.Add(vietnameseSongsButton);

            
            
            
            
            
            
            
            
            
            
            
            
            
        }

        private void CategorySearchButton_Click(object sender, EventArgs e)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchNormalBackground;
            languageSearchButton.BackgroundImage = languageSearchNormalBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchActiveBackground;
            orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
            myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;
            isOnOrderedSongsPage = false;

            loveDuetButton.BackgroundImage = loveDuetActiveBackground;
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            loveDuetSongs = allSongs.Where(song => song.SongGenre.Contains("A1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = loveDuetSongs; 
            totalPages = (int)Math.Ceiling((double)loveDuetSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);

            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetGroupButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetPictureBoxToggleLightAndButtonsVisibility(false);
            SetPictureBoxSceneSoundEffectsAndButtonsVisibility(false);
            SetPictureBoxCategoryAndButtonsVisibility(true);

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }
        
        private void TogglePictureBoxCategoryButtonsVisibility()
        {
            
            bool areButtonsVisible = loveDuetButton.Visible; 

            
            SetPictureBoxCategoryAndButtonsVisibility(!areButtonsVisible);
        }
        
        private void SetPictureBoxCategoryAndButtonsVisibility(bool isVisible)
        {   
            loveDuetButton.Visible = isVisible;
            loveDuetButton.BringToFront();
            
            talentShowButton.Visible = isVisible;
            talentShowButton.BringToFront();
            
            medleyDanceButton.Visible = isVisible;
            medleyDanceButton.BringToFront();
            
            ninetiesButton.Visible = isVisible;
            ninetiesButton.BringToFront();
            
            nostalgicSongsButton.Visible = isVisible;
            nostalgicSongsButton.BringToFront();
            
            chinaSongsButton.Visible = isVisible;
            chinaSongsButton.BringToFront();

            vietnameseSongsButton.Visible = isVisible;
            vietnameseSongsButton.BringToFront();

            
            
        }
    }
}