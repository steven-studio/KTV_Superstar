using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void InitializeButtonsForPictureBoxArtistSearch()
        {
            
            int[,] coords = new int[,]
            {
                {651, 292, 752, 400}, 
                {760, 292, 861, 400}, 
                {869, 292, 972, 399}, 
                {652, 401, 752, 502}, 
                {760, 401, 861, 504}, 
                {869, 398, 972, 502}, 
                {651, 502, 753, 607}, 
                {759, 504, 863, 607}, 
                {869, 503, 973, 608}, 
                {981, 501, 1083, 609} 
            };

            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            
            float widthRatio = screenW / (float)1440;
            float heightRatio = screenH / (float)900;

            numberButtonsArtistSearch = new Button[10];

            for (int i = 0; i < numberButtonsArtistSearch.Length; i++)
            {
                numberButtonsArtistSearch[i] = new Button();

                
                ConfigureButton(
                    numberButtonsArtistSearch[i],
                    coords[i, 0], 
                    coords[i, 1], 
                    coords[i, 2] - coords[i, 0], 
                    coords[i, 3] - coords[i, 1], 
                    
                    resizedNormalStateImageFor6_1,
                    resizedMouseOverImageFor6_1,
                    resizedMouseDownImageFor6_1,
                    null
                );

                int newXForArtistSearch = (int)(((numberButtonsArtistSearch[i].Location.X / widthRatio) + offsetXArtistSearch) * widthRatio);
                int newYForArtistSearch = (int)(((numberButtonsArtistSearch[i].Location.Y / heightRatio) + offsetXArtistSearch) * heightRatio);
                numberButtonsArtistSearch[i].Location = new Point(newXForArtistSearch, newYForArtistSearch);

                
                numberButtonsArtistSearch[i].Name = "NumberButtonArtistSearch" + i;
                
                numberButtonsArtistSearch[i].Tag = (i + 1).ToString();
                if (i == 9) 
                {
                    numberButtonsArtistSearch[i].Name = "NumberButtonArtistSearch0";
                    numberButtonsArtistSearch[i].Tag = "0";
                }

                
                numberButtonsArtistSearch[i].Click += ArtistButton_Click;

                
                this.Controls.Add(numberButtonsArtistSearch[i]);
            }

            
            modifyButtonArtistSearch = new Button {
                Name = "ModifyButtonArtistSearch",
                Tag = "Modify",
                Visible = false
            };
            
            ConfigureButton(modifyButtonArtistSearch, 978, 292, 1081 - 978, 397 - 292, resizedNormalStateImageFor6_1, resizedMouseOverImageFor6_1, resizedMouseDownImageFor6_1, ModifyButtonArtist_Click);
            int newX = (int)(((modifyButtonArtistSearch.Location.X / widthRatio) + offsetXArtistSearch) * widthRatio);
            int newY = (int)(((modifyButtonArtistSearch.Location.Y / widthRatio) + offsetYArtistSearch) * heightRatio);
            modifyButtonArtistSearch.Location = new Point(newX, newY);
            this.Controls.Add(modifyButtonArtistSearch);
            
            
            closeButtonArtistSearch = new Button {
                Name = "CloseButtonArtistSearch",
                Tag = "Close",
                Visible = false
            };
            
            ConfigureButton(closeButtonArtistSearch, 982, 147, 1082 - 982, 250 - 147, resizedNormalStateImageFor6_1, resizedMouseOverImageFor6_1, resizedMouseDownImageFor6_1, CloseButtonArtistSearch_Click);
            newX = (int)(((closeButtonArtistSearch.Location.X / widthRatio) + offsetXArtistSearch) * widthRatio);
            newY = (int)(((closeButtonArtistSearch.Location.Y / widthRatio) + offsetYArtistSearch) * heightRatio);
            closeButtonArtistSearch.Location = new Point(newX, newY);
            this.Controls.Add(closeButtonArtistSearch);

            inputBoxArtistSearch = new RichTextBox();
            inputBoxArtistSearch.Name = "inputBoxArtistSearch";
            ResizeAndPositionControl(inputBoxArtistSearch, 645 + offsetXArtistSearch, 197 + offsetXArtistSearch, 986 - 645, 281 - 197);
            inputBoxArtistSearch.ForeColor = Color.Black;
            inputBoxArtistSearch.Font = new Font("細明體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular);

            inputBoxArtistSearch.TextChanged += (sender, e) =>
            {
                string searchText = inputBoxArtistSearch.Text;
                int targetLength = 0;

                
                if (int.TryParse(searchText, out targetLength))
                {
                    
                    var searchResults = allArtists.Where(artist => artist.Name.Replace(" ", "").Length == targetLength).ToList();

                    currentPage = 0;
                    currentArtistList = searchResults;
                    totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                    multiPagePanel.currentPageIndex = 0;
                    multiPagePanel.LoadSingers(currentArtistList);
                }
                else
                {
                    
                    currentArtistList.Clear();
                }
            };

            this.Controls.Add(inputBoxArtistSearch);
        }

        
        private void ArtistButton_Click(object sender, EventArgs e)
        {
            
            
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                inputBoxArtistSearch.Text += button.Tag.ToString();
            }
        }
        
        private void WordCountSearchButton_Click(object sender, EventArgs e)
        {
            zhuyinSearchButton.BackgroundImage = zhuyinSearchNormalBackground;
            englishSearchButton.BackgroundImage = englishSearchNormalBackground;
            pinyinSearchButton.BackgroundImage = pinyinSearchNormalBackground;
            wordCountSearchButton.BackgroundImage = wordCountSearchActiveBackground;
            handWritingSearchButton.BackgroundImage = handWritingSearchNormalBackground;

            
            bool shouldBeVisible = !pictureBoxArtistSearch.Visible;

            
            if (shouldBeVisible)
            {
                ShowImageOnPictureBoxArtistSearch(Path.Combine(Application.StartupPath, @"themes\superstar\6-1.png"));
            }

            SetEnglishSingersAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetHandWritingForSingersAndButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetPictureBoxArtistSearchAndButtonsVisibility(shouldBeVisible);
            pictureBoxArtistSearch.Visible = shouldBeVisible;
        }

        private void CloseButtonArtistSearch_Click(object sender, EventArgs e)
        {
            
            SetPictureBoxArtistSearchAndButtonsVisibility(false);
        }
    }
}