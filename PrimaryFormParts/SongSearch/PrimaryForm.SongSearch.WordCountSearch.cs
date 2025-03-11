using System;
using System.Drawing; 
using System.Linq; 
using System.Windows.Forms;
using System.IO;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        
        private void WordCountSearchSong_Click(object sender, EventArgs e)
        {
            zhuyinSearchSongButton.BackgroundImage = zhuyinSearchSongNormalBackground;
            englishSearchSongButton.BackgroundImage = englishSearchSongNormalBackground;
            pinyinSearchSongButton.BackgroundImage = pinyinSearchSongNormalBackground;
            wordCountSearchSongButton.BackgroundImage = wordCountSearchSongActiveBackground;
            handWritingSearchSongButton.BackgroundImage = handWritingSearchNormalBackground;
            numberSearchSongButton.BackgroundImage = numberSearchSongNormalBackground;

            ShowImageOnPictureBoxWordCount(Path.Combine(Application.StartupPath, @"themes\superstar\6-1.png"));

            SetPictureBoxWordCountAndButtonsVisibility(true);
            pictureBoxWordCount.Visible = true;
        }

        private void ShowImageOnPictureBoxWordCount(string imagePath)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle cropArea = new Rectangle(593, 135, 507, 508);

            
            Bitmap croppedImage = CropImage(originalImage, cropArea);

            
            pictureBoxWordCount.Image = croppedImage;
    
            
            ResizeAndPositionPictureBox(pictureBoxWordCount, cropArea.X + offsetXWordCount, cropArea.Y + offsetXWordCount, cropArea.Width, cropArea.Height);
            
            pictureBoxWordCount.Visible = true;
        }

        private void InitializeButtonsForPictureBoxWordCount()
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

            numberButtonsWordCount = new Button[10];

            for (int i = 0; i < numberButtonsWordCount.Length; i++)
            {
                numberButtonsWordCount[i] = new Button();

                
                ConfigureButton(
                    numberButtonsWordCount[i],
                    coords[i, 0], 
                    coords[i, 1], 
                    coords[i, 2] - coords[i, 0], 
                    coords[i, 3] - coords[i, 1], 
                    
                    resizedNormalStateImageFor6_1,
                    resizedMouseOverImageFor6_1,
                    resizedMouseDownImageFor6_1,
                    null
                );

                int newXForWordCount = (int)(((numberButtonsWordCount[i].Location.X / widthRatio) + offsetXWordCount) * widthRatio);
                int newYForWordCount = (int)(((numberButtonsWordCount[i].Location.Y / heightRatio) + offsetYWordCount) * heightRatio);
                numberButtonsWordCount[i].Location = new Point(newXForWordCount, newYForWordCount);

                
                numberButtonsWordCount[i].Name = "NumberButtonWordCount" + i;
                
                numberButtonsWordCount[i].Tag = (i + 1).ToString();
                if (i == 9) 
                {
                    numberButtonsWordCount[i].Name = "NumberButtonWordCount0";
                    numberButtonsWordCount[i].Tag = "0";
                }

                
                numberButtonsWordCount[i].Click += WordCountButton_Click;

                
                this.Controls.Add(numberButtonsWordCount[i]);
            }

            
            modifyButtonWordCount = new Button {
                Name = "ModifyButtonWordCount",
                Tag = "Modify",
                Visible = false
            };
            
            ConfigureButton(modifyButtonWordCount, 978, 292, 1081 - 978, 397 - 292, resizedNormalStateImageFor6_1, resizedMouseOverImageFor6_1, resizedMouseDownImageFor6_1, ModifyButtonWordCount_Click);
            int newX = (int)(((modifyButtonWordCount.Location.X / widthRatio) + offsetXWordCount) * widthRatio);
            int newY = (int)(((modifyButtonWordCount.Location.Y / widthRatio) + offsetYWordCount) * heightRatio);
            modifyButtonWordCount.Location = new Point(newX, newY);
            this.Controls.Add(modifyButtonWordCount);
            
            
            closeButtonWordCount = new Button {
                Name = "CloseButtonWordCount",
                Tag = "Close",
                Visible = false
            };
            
            ConfigureButton(closeButtonWordCount, 982, 147, 1082 - 982, 250 - 147, resizedNormalStateImageFor6_1, resizedMouseOverImageFor6_1, resizedMouseDownImageFor6_1, CloseButtonWordCount_Click);
            newX = (int)(((closeButtonWordCount.Location.X / widthRatio) + offsetXWordCount) * widthRatio);
            newY = (int)(((closeButtonWordCount.Location.Y / widthRatio) + offsetYWordCount) * heightRatio);
            closeButtonWordCount.Location = new Point(newX, newY);
            this.Controls.Add(closeButtonWordCount);

            inputBoxWordCount = new RichTextBox();
            inputBoxWordCount.Name = "inputBoxWordCount";
            ResizeAndPositionControl(inputBoxWordCount, 645 + offsetXWordCount, 197 + offsetYWordCount, 986 - 645, 281 - 197);
            inputBoxWordCount.ForeColor = Color.Black;
            inputBoxWordCount.Font = new Font("細明體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular);

            inputBoxWordCount.TextChanged += (sender, e) =>
            {
                string searchText = inputBoxWordCount.Text;
                int targetLength = 0;

                
                if (int.TryParse(searchText, out targetLength))
                {
                    
                    var searchResults = allSongs.Where(song => song.Song.Replace(" ", "").Length == targetLength).ToList();
                    currentPage = 0;
                    currentSongList = searchResults;
                    totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                    multiPagePanel.currentPageIndex = 0;
                    multiPagePanel.LoadSongs(currentSongList);
                }
                else
                {
                    
                    currentSongList.Clear();
                }
            };

            this.Controls.Add(inputBoxWordCount);
        }

        
        private void WordCountButton_Click(object sender, EventArgs e)
        {
            
            
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                inputBoxWordCount.Text += button.Tag.ToString();
            }
        }

        private void CloseButtonWordCount_Click(object sender, EventArgs e)
        {
            
            SetPictureBoxWordCountAndButtonsVisibility(false);
        }
    }
}