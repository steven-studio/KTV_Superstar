using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button myFavoritesButton;
        private Bitmap myFavoritesNormalBackground;
        private Bitmap myFavoritesActiveBackground;

        private string mobileNumber = string.Empty;
        public static bool isPhoneNumberValid;
        private bool showError = false;
        private PictureBox FavoritePictureBox;
        private Button[] favoriteNumberButton;
        private Button enterFavoriteButton;
        private Button newFavoriteButton;
        private Button refillFavoriteButton;
        private Button closeFavoriteButton;
        private Label errorMessageLabel;

        private void InitializeButtonsForFavoritePictureBox()
        {
            
            int[,] coords = new int[,]
            {
                {799, 508, 70, 65}, 
                {878, 508, 70, 65}, 
                {957, 508, 70, 65}, 
                {1036, 508, 70, 65}, 
                {1115, 508, 70, 65}, 
                {799, 580, 70, 65}, 
                {878, 580, 70, 65}, 
                {957, 580, 70, 65}, 
                {1036, 580, 70, 65}, 
                {1115, 580, 70, 65} 
            };

            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            
            float widthRatio = screenW / (float)1440;
            float heightRatio = screenH / (float)900;

            favoriteNumberButton = new Button[10];

            for (int i = 0; i < favoriteNumberButton.Length; i++)
            {
                favoriteNumberButton[i] = new Button();

                ResizeAndPositionButton(favoriteNumberButton[i], coords[i, 0], coords[i, 1], coords[i, 2], coords[i, 3]);

                
                string fileName = (i + 2).ToString("00");  
                string filePath = Path.Combine(Application.StartupPath, @"themes\superstar\我的最愛\我的最愛-" + fileName + ".jpg");
                favoriteNumberButton[i].BackgroundImage = Image.FromFile(filePath);
                favoriteNumberButton[i].BackgroundImageLayout = ImageLayout.Stretch;
                favoriteNumberButton[i].FlatStyle = FlatStyle.Flat;
                favoriteNumberButton[i].FlatAppearance.BorderSize = 0; 
                favoriteNumberButton[i].BackColor = Color.Transparent;
                favoriteNumberButton[i].FlatAppearance.MouseDownBackColor = Color.Transparent; 
                favoriteNumberButton[i].FlatAppearance.MouseOverBackColor = Color.Transparent; 

                
                favoriteNumberButton[i].Name = "favoriteNumberButton" + i;
                
                favoriteNumberButton[i].Tag = (i + 1).ToString();
                if (i == 9) 
                {
                    favoriteNumberButton[i].Name = "favoriteNumberButton0";
                    favoriteNumberButton[i].Tag = "0";
                }

                
                favoriteNumberButton[i].Click += FavoriteNumberButton_Click;

                
                this.Controls.Add(favoriteNumberButton[i]);
            }

            
            enterFavoriteButton = new Button()
            {
                Name = "enterFavoriteButton"
            };
            ResizeAndPositionButton(enterFavoriteButton, 842, 652, 70, 65);
            enterFavoriteButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\我的最愛\我的最愛-12.jpg"));
            enterFavoriteButton.BackgroundImageLayout = ImageLayout.Stretch;
            enterFavoriteButton.FlatStyle = FlatStyle.Flat;
            enterFavoriteButton.FlatAppearance.BorderSize = 0; 
            enterFavoriteButton.BackColor = Color.Transparent;
            enterFavoriteButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            enterFavoriteButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            enterFavoriteButton.Click += EnterFavoriteButton_Click; 

            
            newFavoriteButton = new Button()
            {
                Name = "newFavoriteButton"
            };
            ResizeAndPositionButton(newFavoriteButton, 921, 652, 70, 65);
            newFavoriteButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\我的最愛\我的最愛-13.jpg"));
            newFavoriteButton.BackgroundImageLayout = ImageLayout.Stretch;
            newFavoriteButton.FlatStyle = FlatStyle.Flat;
            newFavoriteButton.FlatAppearance.BorderSize = 0; 
            newFavoriteButton.BackColor = Color.Transparent;
            newFavoriteButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            newFavoriteButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            newFavoriteButton.Click += NewFavoriteButton_Click;

            
            refillFavoriteButton = new Button()
            {
                Name = "refillFavoriteButton"
            };
            ResizeAndPositionButton(refillFavoriteButton, 999, 652, 70, 65);
            refillFavoriteButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\我的最愛\我的最愛-14.jpg"));
            refillFavoriteButton.BackgroundImageLayout = ImageLayout.Stretch;
            refillFavoriteButton.FlatStyle = FlatStyle.Flat;
            refillFavoriteButton.FlatAppearance.BorderSize = 0; 
            refillFavoriteButton.BackColor = Color.Transparent;
            refillFavoriteButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            refillFavoriteButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            refillFavoriteButton.Click += RefillFavoriteButton_Click;

            
            closeFavoriteButton = new Button()
            {
                Name = "closeFavoriteButton"
            };
            ResizeAndPositionButton(closeFavoriteButton, 1078, 652, 70, 65);
            closeFavoriteButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\我的最愛\我的最愛-15.jpg"));
            closeFavoriteButton.BackgroundImageLayout = ImageLayout.Stretch;
            closeFavoriteButton.FlatStyle = FlatStyle.Flat;
            closeFavoriteButton.FlatAppearance.BorderSize = 0; 
            closeFavoriteButton.BackColor = Color.Transparent;
            closeFavoriteButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            closeFavoriteButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            closeFavoriteButton.Click += CloseFavoriteButton_Click;

            
            errorMessageLabel = new Label
            {
                Text = "",
                ForeColor = Color.Black,
                Location = new Point(10, 250),  
                AutoSize = true
            };

            
            this.Controls.Add(enterFavoriteButton);
            this.Controls.Add(newFavoriteButton);
            this.Controls.Add(refillFavoriteButton);
            this.Controls.Add(closeFavoriteButton);
            this.Controls.Add(errorMessageLabel);
        }
        
        private void FavoriteNumberButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                
                mobileNumber += clickedButton.Tag.ToString();
                Console.WriteLine("Number button clicked: " + clickedButton.Tag.ToString());
                phonenumber = mobileNumber;
                
                FavoritePictureBox.Invalidate();
            }
        }
        public static string phonenumber;
        private void FavoritePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (!string.IsNullOrEmpty(mobileNumber))
            {
                using (Font font = new Font("Arial", 24))
                using (Brush brush = new SolidBrush(Color.Black))
                {
                    int x = 16;
                    int y = 68;

                    if (showError)
                    {
                        string errorMessage;
                        if (!isPhoneNumberValid)
                        {
                            errorMessage = "查無此手機號碼!!!";
                        }
                        else
                        {
                            errorMessage = "手機號碼輸入錯誤!!!";
                        }
                        e.Graphics.DrawString(errorMessage, font, brush, x, y);
                    }
                    else
                    {
                        e.Graphics.DrawString(mobileNumber, font, brush, x, y);
                    }
                }
            }
        }
        
        
        private void EnterFavoriteButton_Click(object sender, EventArgs e)
        {
            if (mobileNumber.StartsWith("09") && mobileNumber.Length == 10)
            {
                if (SongListManager.Instance.CheckIfPhoneNumberExists(mobileNumber))
                {
                    isPhoneNumberValid = true;
                    SongListManager.Instance.UserLogin(mobileNumber);
                    
                    ToggleFavoritePictureBoxButtonsVisibility();
                }
                else
                {
                    isPhoneNumberValid = false;
                    showError = true; 
                    FavoritePictureBox.Invalidate();
                    FavoritePictureBox.Refresh(); 
                }
            }
            else
            {
                showError = true; 
                isPhoneNumberValid = true;
                FavoritePictureBox.Invalidate(); 
                FavoritePictureBox.Refresh(); 
            }
        }
        
        
        private void NewFavoriteButton_Click(object sender, EventArgs e)
        {
            if (mobileNumber.StartsWith("09") && mobileNumber.Length == 10)
            {
                if (SongListManager.Instance.CheckIfPhoneNumberExists(mobileNumber))
                {
                    isPhoneNumberValid = true;
                    SongListManager.Instance.UserLogin(mobileNumber);
                    
                    ToggleFavoritePictureBoxButtonsVisibility();
                }
                else
                {
                    isPhoneNumberValid = true;
                    SongListManager.Instance.AddNewUser(mobileNumber);
                    SongListManager.Instance.UserLogin(mobileNumber);
                    
                    
                    List<SongData> emptySongList = new List<SongData> { new SongData("", "", "歡迎光臨 " + "(" + mobileNumber + ")", 0, "", "", "", "", DateTime.Now, "", "", "", "", "", "", "", "", "", "", "", "", 1) };
                    multiPagePanel.currentPageIndex = 0;
                    multiPagePanel.LoadSongs(emptySongList);
                    ToggleFavoritePictureBoxButtonsVisibility();
                }
            }
            else
            {
                showError = true; 
                isPhoneNumberValid = true;
                FavoritePictureBox.Invalidate(); 
                FavoritePictureBox.Refresh();
            }
        }
        
        
        private void RefillFavoriteButton_Click(object sender, EventArgs e)
        {
            
            mobileNumber = string.Empty;

            
            showError = false;

            
            FavoritePictureBox.Invalidate();
            FavoritePictureBox.Refresh();

            SongListManager.Instance.IsUserLoggedIn = false;
            SongListManager.Instance.UserPhoneNumber = string.Empty;
        }
        
        
        private void CloseFavoriteButton_Click(object sender, EventArgs e)
        {
            
            ToggleFavoritePictureBoxButtonsVisibility();
        }
        
        private void MyFavoritesButton_Click(object sender, EventArgs e)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchNormalBackground;
            languageSearchButton.BackgroundImage = languageSearchNormalBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
            myFavoritesButton.BackgroundImage = myFavoritesActiveBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;
            isOnOrderedSongsPage = false;

            
            if (!FavoritePictureBox.Visible)
            {
                
                ShowImageOnFavoritePictureBox(Path.Combine(Application.StartupPath, @"themes\superstar\其他介面\其他_我的最愛.jpg"));
                SetFavoritePictureBoxAndButtonsVisibility(true);
            }
            else
            {
                
                ToggleFavoritePictureBoxButtonsVisibility();
            }

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }

            SetPictureBoxToggleLightAndButtonsVisibility(false);
            SetPictureBoxSceneSoundEffectsAndButtonsVisibility(false);
        }
        
        private void ShowImageOnFavoritePictureBox(string imagePath)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Console.WriteLine(String.Format("Original Image Size: {0}x{1}", originalImage.Width, originalImage.Height));

            
            Rectangle cropArea = new Rectangle(784, 393, 555, 442);

            
            Bitmap croppedImage = CropImage(originalImage, cropArea);

            
            FavoritePictureBox.Image = croppedImage;
    
            
            ResizeAndPositionPictureBox(FavoritePictureBox, cropArea.X, cropArea.Y, 416, 323);
            
            FavoritePictureBox.Visible = true;
        }

        private void ToggleFavoritePictureBoxButtonsVisibility()
        {
            
            bool areButtonsVisible = FavoritePictureBox.Visible; 

            
            SetFavoritePictureBoxAndButtonsVisibility(!areButtonsVisible);
        }

        private void SetFavoritePictureBoxAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                SuspendLayout();

                
                FavoritePictureBox.Visible = isVisible;

                
                if (isVisible) FavoritePictureBox.BringToFront();

                
                enterFavoriteButton.Visible = isVisible;
                newFavoriteButton.Visible = isVisible;
                refillFavoriteButton.Visible = isVisible;
                closeFavoriteButton.Visible = isVisible;

                
                if (isVisible)
                {
                    enterFavoriteButton.BringToFront();
                    newFavoriteButton.BringToFront();
                    refillFavoriteButton.BringToFront();
                    closeFavoriteButton.BringToFront();
                }

                
                foreach (Button button in favoriteNumberButton)
                {
                    button.Visible = isVisible;
                    
                    if (isVisible)
                        button.BringToFront();
                }

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
    }
}