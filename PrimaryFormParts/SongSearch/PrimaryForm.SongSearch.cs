using System;
using System.Drawing;
using System.Windows.Forms;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button zhuyinSearchSongButton;
        private Bitmap zhuyinSearchSongNormalBackground;
        private Bitmap zhuyinSearchSongActiveBackground;
        private Button englishSearchSongButton;
        private Bitmap englishSearchSongNormalBackground;
        private Bitmap englishSearchSongActiveBackground;
        private Button wordCountSearchSongButton;
        private Bitmap wordCountSearchSongNormalBackground;
        private Bitmap wordCountSearchSongActiveBackground;
        private Button pinyinSearchSongButton;
        private Bitmap pinyinSearchSongNormalBackground;
        private Bitmap pinyinSearchSongActiveBackground;
        private Button handWritingSearchSongButton;
        private Bitmap handWritingSearchSongNormalBackground;
        private Bitmap handWritingSearchSongActiveBackground;
        private Button numberSearchSongButton;
        private Bitmap numberSearchSongNormalBackground;
        private Bitmap numberSearchSongActiveBackground;

        private void SongSearchButton_Click(object sender, EventArgs e)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchActiveBackground;
            languageSearchButton.BackgroundImage = languageSearchNormalBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
            myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;
            isOnOrderedSongsPage = false;

            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetGroupButtonsVisibility(false);
            SetPictureBoxCategoryAndButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetPictureBoxToggleLightAndButtonsVisibility(false);
            SetPictureBoxSceneSoundEffectsAndButtonsVisibility(false);
            SetSongSearchButtonsVisibility(true);

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }
        
        private void SetSongSearchButtonsVisibility(bool isVisible)
        {
            pictureBox4.Visible = isVisible;

            Button[] songSearchButtons = { zhuyinSearchSongButton, englishSearchSongButton, wordCountSearchSongButton, pinyinSearchSongButton, handWritingSearchSongButton, numberSearchSongButton };

            foreach (var button in songSearchButtons)
            {
                button.Visible = isVisible;
                if (isVisible)
                {
                    button.BringToFront();
                }
            }
        }

        private void InitializeButtonsForSongSearch()
        {
            
            InitializeSearchButton(ref zhuyinSearchSongButton, "zhuyinSearchSongButton", 1214, 230, 209, 59, ref zhuyinSearchSongNormalBackground, ref zhuyinSearchSongActiveBackground, normalStateImageSongQuery, mouseDownImageSongQuery, ZhuyinSearchSongsButton_Click);

            
            InitializeSearchButton(ref englishSearchSongButton, "englishSearchSongButton", 1214, 293, 209, 58, ref englishSearchSongNormalBackground, ref englishSearchSongActiveBackground, normalStateImageSongQuery, mouseDownImageSongQuery, EnglishSearchSongsButton_Click);

            
            InitializeSearchButton(ref pinyinSearchSongButton, "pinyinSearchSongButton", 1214, 356, 209, 58, ref pinyinSearchSongNormalBackground, ref pinyinSearchSongActiveBackground, normalStateImageSongQuery, mouseDownImageSongQuery, PinyinSearchSongsButton_Click);

            
            InitializeSearchButton(ref wordCountSearchSongButton, "wordCountSearchSongButton", 1214, 418, 209, 59, ref wordCountSearchSongNormalBackground, ref wordCountSearchSongActiveBackground, normalStateImageSongQuery, mouseDownImageSongQuery, WordCountSearchSong_Click);

            
            InitializeSearchButton(ref handWritingSearchSongButton, "handWritingSearchSongButton", 1214, 481, 209, 59, ref handWritingSearchSongNormalBackground, ref handWritingSearchSongActiveBackground, normalStateImageSongQuery, mouseDownImageSongQuery, HandWritingSearchButtonForSongs_Click);

            
            InitializeSearchButton(ref numberSearchSongButton, "numberSearchSongButton", 1214, 544, 209, 58, ref numberSearchSongNormalBackground, ref numberSearchSongActiveBackground, normalStateImageSongQuery, mouseDownImageSongQuery, NumberSearchButton2_Click);
        }
    }
}