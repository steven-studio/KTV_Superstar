using System;
using System.Drawing; 
using System.IO; 
using System.Linq; 
using System.Windows.Forms; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void NumberSearchButton2_Click(object sender, EventArgs e)
        {
            zhuyinSearchSongButton.BackgroundImage = zhuyinSearchSongNormalBackground;
            englishSearchSongButton.BackgroundImage = englishSearchSongNormalBackground;
            pinyinSearchSongButton.BackgroundImage = pinyinSearchSongNormalBackground;
            wordCountSearchSongButton.BackgroundImage = wordCountSearchSongNormalBackground;
            handWritingSearchSongButton.BackgroundImage = handWritingSearchNormalBackground;
            numberSearchSongButton.BackgroundImage = numberSearchSongActiveBackground;

            ShowImageOnPictureBoxSongIDSearch(Path.Combine(Application.StartupPath, @"themes\superstar\6-1.png"));

            SetPictureBoxSongIDSearchAndButtonsVisibility(true);
            pictureBoxSongIDSearch.Visible = true;
        }

        private void ShowImageOnPictureBoxSongIDSearch(string imagePath)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle cropArea = new Rectangle(593, 135, 507, 508);

            
            Bitmap croppedImage = CropImage(originalImage, cropArea);

            
            pictureBoxSongIDSearch.Image = croppedImage;
    
            
            ResizeAndPositionPictureBox(pictureBoxSongIDSearch, cropArea.X + offsetXSongID, cropArea.Y + offsetYSongID, cropArea.Width, cropArea.Height);
            
            pictureBoxSongIDSearch.Visible = true;
        }

        private void ModifyButtonSongIDSearch_Click(object sender, EventArgs e)
        {
            
            if (inputBoxSongIDSearch.Text.Length > 0)
            {
                
                inputBoxSongIDSearch.Text = inputBoxSongIDSearch.Text.Substring(0, inputBoxSongIDSearch.Text.Length - 1);
            }
        }

        private void CloseButtonSongIDSearch_Click(object sender, EventArgs e)
        {
            
            SetPictureBoxSongIDSearchAndButtonsVisibility(false);
        }

        private void SetPictureBoxSongIDSearchAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                SuspendLayout();

                
                pictureBoxSongIDSearch.Visible = isVisible;

                
                if (isVisible) pictureBoxSongIDSearch.BringToFront();

                
                modifyButtonSongIDSearch.Visible = isVisible;
                closeButtonSongIDSearch.Visible = isVisible;

                
                if (isVisible)
                {
                    modifyButtonSongIDSearch.BringToFront();
                    closeButtonSongIDSearch.BringToFront();
                }

                
                foreach (Button button in numberButtonsSongIDSearch)
                {
                    button.Visible = isVisible;
                    
                    if (isVisible)
                        button.BringToFront();
                }

                inputBoxSongIDSearch.Visible = isVisible;
                if (isVisible) inputBoxSongIDSearch.BringToFront();

                ResumeLayout();
            };

            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }
        
        private void InitializeButtonsForPictureBoxSongIDSearch()
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

            numberButtonsSongIDSearch = new Button[10];

            for (int i = 0; i < numberButtonsSongIDSearch.Length; i++)
            {
                numberButtonsSongIDSearch[i] = new Button();

                
                ConfigureButton(
                    numberButtonsSongIDSearch[i],
                    coords[i, 0], 
                    coords[i, 1], 
                    coords[i, 2] - coords[i, 0], 
                    coords[i, 3] - coords[i, 1], 
                    
                    resizedNormalStateImageFor6_1,
                    resizedMouseOverImageFor6_1,
                    resizedMouseDownImageFor6_1,
                    null
                );

                int newXForSongID = (int)(((numberButtonsSongIDSearch[i].Location.X / widthRatio) + offsetXSongID) * widthRatio);
                int newYForSongID = (int)(((numberButtonsSongIDSearch[i].Location.Y / heightRatio) + offsetYSongID) * heightRatio);
                numberButtonsSongIDSearch[i].Location = new Point(newXForSongID, newYForSongID);

                
                numberButtonsSongIDSearch[i].Name = "NumberButtonSongIDSearch" + i;
                
                numberButtonsSongIDSearch[i].Tag = (i + 1).ToString();
                if (i == 9) 
                {
                    numberButtonsSongIDSearch[i].Name = "NumberButtonSongIDSearch0";
                    numberButtonsSongIDSearch[i].Tag = "0";
                }

                
                numberButtonsSongIDSearch[i].Click += SongIDSearchButton_Click;

                
                this.Controls.Add(numberButtonsSongIDSearch[i]);
            }

            
            modifyButtonSongIDSearch = new Button {
                Name = "ModifyButtonSongIDSearch",
                Tag = "Modify",
                Visible = false
            };
            
            ConfigureButton(modifyButtonSongIDSearch, 978, 292, 1081 - 978, 397 - 292, resizedNormalStateImageFor6_1, resizedMouseOverImageFor6_1, resizedMouseDownImageFor6_1, ModifyButtonSongIDSearch_Click);
            int newX = (int)(((modifyButtonSongIDSearch.Location.X / widthRatio) + offsetXSongID) * widthRatio);
            int newY = (int)(((modifyButtonSongIDSearch.Location.Y / widthRatio) + offsetYSongID) * heightRatio);
            modifyButtonSongIDSearch.Location = new Point(newX, newY);
            this.Controls.Add(modifyButtonSongIDSearch);
            
            
            closeButtonSongIDSearch = new Button {
                Name = "CloseButtonSongIDSearch",
                Tag = "Close",
                Visible = false
            };
            
            ConfigureButton(closeButtonSongIDSearch, 982, 147, 1082 - 982, 250 - 147, resizedNormalStateImageFor6_1, resizedMouseOverImageFor6_1, resizedMouseDownImageFor6_1, CloseButtonSongIDSearch_Click);
            newX = (int)(((closeButtonSongIDSearch.Location.X / widthRatio) + offsetXSongID) * widthRatio);
            newY = (int)(((closeButtonSongIDSearch.Location.Y / widthRatio) + offsetYSongID) * heightRatio);
            closeButtonSongIDSearch.Location = new Point(newX, newY);
            this.Controls.Add(closeButtonSongIDSearch);

            inputBoxSongIDSearch = new RichTextBox();
            inputBoxSongIDSearch.Name = "inputBoxSongIDSearch";
            ResizeAndPositionControl(inputBoxSongIDSearch, 645 + offsetXSongID, 197 + offsetXSongID, 986 - 645, 281 - 197);
            inputBoxSongIDSearch.ForeColor = Color.Black;
            inputBoxSongIDSearch.Font = new Font("細明體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular);

            inputBoxSongIDSearch.TextChanged += (sender, e) =>
            {
                string searchText = inputBoxSongIDSearch.Text;
                
                var searchResults = allSongs.Where(song => song.SongNumber.StartsWith(searchText)).ToList();
                currentPage = 0;
                currentSongList = searchResults;
                totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);
                
                multiPagePanel.currentPageIndex = 0;
                multiPagePanel.LoadSongs(currentSongList);
            };

            this.Controls.Add(inputBoxSongIDSearch);
        }

        
        private void SongIDSearchButton_Click(object sender, EventArgs e)
        {
            
            
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                inputBoxSongIDSearch.Text += button.Tag.ToString();
            }
        }
    }
}