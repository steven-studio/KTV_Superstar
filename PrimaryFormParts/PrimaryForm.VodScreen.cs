using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button vodButton;
        private Button insertButton;
        private Button albumButton;
        private Button favoriteButton;
        private Panel disabledPanel;
        private Button vodScreenCloseButton;
        
        private void InitializeButtonsForVodScreenPictureBox()
        {
            int screenWidth = 1440;
            int screenHeight = 900;
            int pictureBoxWidth = 700;
            int pictureBoxHeight = 140;

            int xPosition = (screenWidth - pictureBoxWidth) / 2;
            int yPosition = (screenHeight - pictureBoxHeight) / 2;

            
            vodButton = new Button();
            vodButton.Text = "";
            ResizeAndPositionButton(vodButton, xPosition + 10, yPosition + 85, 110, 50);
            vodButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\點播介面\點播介面_點歌.png"));
            vodButton.BackgroundImageLayout = ImageLayout.Stretch;
            vodButton.FlatStyle = FlatStyle.Flat;
            vodButton.FlatAppearance.BorderSize = 0; 
            vodButton.BackColor = Color.Transparent;
            vodButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            vodButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            vodButton.Click += VodButton_Click;  
            vodButton.Visible = false;

            
            insertButton = new Button();
            insertButton.Text = "";
            ResizeAndPositionButton(insertButton, xPosition + 135, yPosition + 85, 110, 50);
            insertButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\點播介面\點播介面_插播.png"));
            insertButton.BackgroundImageLayout = ImageLayout.Stretch;
            insertButton.FlatStyle = FlatStyle.Flat;
            insertButton.FlatAppearance.BorderSize = 0; 
            insertButton.BackColor = Color.Transparent;
            insertButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            insertButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            insertButton.Click += InsertButton_Click;
            insertButton.Visible = false;

            
            albumButton = new Button();
            albumButton.Text = "";
            ResizeAndPositionButton(albumButton, xPosition + 265, yPosition + 85, 140, 50);
            albumButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\點播介面\點播介面_歷年專輯.png"));
            albumButton.BackgroundImageLayout = ImageLayout.Stretch;
            albumButton.FlatStyle = FlatStyle.Flat;
            albumButton.FlatAppearance.BorderSize = 0; 
            albumButton.BackColor = Color.Transparent;
            albumButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            albumButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            albumButton.Click += AlbumButton_Click;
            albumButton.Visible = false;

            
            favoriteButton = new Button();
            favoriteButton.Text = "";
            ResizeAndPositionButton(favoriteButton, xPosition + 425, yPosition + 85, 140, 50);
            favoriteButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\點播介面\點播介面_我的最愛.png"));
            favoriteButton.BackgroundImageLayout = ImageLayout.Stretch;
            favoriteButton.FlatStyle = FlatStyle.Flat;
            favoriteButton.FlatAppearance.BorderSize = 0; 
            favoriteButton.BackColor = Color.Transparent;
            favoriteButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            favoriteButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            
            disabledPanel = new Panel();
            disabledPanel.BackColor = Color.FromArgb(128, Color.Black); 
            disabledPanel.Dock = DockStyle.Fill; 
            disabledPanel.Visible = !IsUserLoggedIn(); 

            
            favoriteButton.Controls.Add(disabledPanel);
            favoriteButton.Click += FavoriteButton_Click;
            
            
            if (!IsUserLoggedIn()) {
                favoriteButton.Enabled = false;
                favoriteButton.BackColor = SystemColors.Control; 
            }
            favoriteButton.Visible = IsUserLoggedIn();

            
            vodScreenCloseButton = new Button();
            vodScreenCloseButton.Text = "";
            ResizeAndPositionButton(vodScreenCloseButton, xPosition + 580, yPosition + 85, 110, 50);
            vodScreenCloseButton.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\點播介面\點播介面_關閉.png"));
            vodScreenCloseButton.BackgroundImageLayout = ImageLayout.Stretch;
            vodScreenCloseButton.FlatStyle = FlatStyle.Flat;
            vodScreenCloseButton.FlatAppearance.BorderSize = 0; 
            vodScreenCloseButton.BackColor = Color.Transparent;
            vodScreenCloseButton.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            vodScreenCloseButton.FlatAppearance.MouseOverBackColor = Color.Transparent; 
            vodScreenCloseButton.Click += VodScreenCloseButton_Click;
            vodScreenCloseButton.Visible = false;

            
            this.Controls.Add(vodButton);
            this.Controls.Add(insertButton);
            this.Controls.Add(albumButton);
            this.Controls.Add(favoriteButton);
            this.Controls.Add(vodScreenCloseButton);
        }
        
        
        private void VodButton_Click(object sender, EventArgs e)
        {
            
            OverlayForm.MainForm.AddSongToPlaylist(currentSelectedSong);
            SetVodScreenPictureBoxAndButtonsVisibility(false);
        }

        private void InsertButton_Click(object sender, EventArgs e)
        {
            
            OverlayForm.MainForm.InsertSongToPlaylist(currentSelectedSong);
            SetVodScreenPictureBoxAndButtonsVisibility(false);
        }

        private void AlbumButton_Click(object sender, EventArgs e)
        {
            
            var selectedSongs = allSongs.Where(song => song.ArtistA == currentSelectedSong.ArtistA)
                            .OrderByDescending(song => song.AddedTime)
                            .ToList();

            UpdateSongList(selectedSongs);
            SetVodScreenPictureBoxAndButtonsVisibility(false);
        }

        private void FavoriteButton_Click(object sender, EventArgs e)
        {
            
            Console.WriteLine("Favorite Button Clicked");

            
            SongListManager.Instance.AddToFavorite(currentSelectedSong.SongNumber);
            SetVodScreenPictureBoxAndButtonsVisibility(false);
        }

        private void VodScreenCloseButton_Click(object sender, EventArgs e)
        {
            
            SetVodScreenPictureBoxAndButtonsVisibility(false);
        }

        
        private bool IsUserLoggedIn()
        {
            
            return SongListManager.Instance.IsUserLoggedIn; 
        }

        private void SetVodScreenPictureBoxAndButtonsVisibility(bool isVisible)
        {
            
            overlayPanel.Visible = isVisible;
            VodScreenPictureBox.Visible = isVisible;

            
            vodButton.Visible = isVisible;
            insertButton.Visible = isVisible;
            albumButton.Visible = isVisible;
            favoriteButton.Visible = isVisible;
            vodScreenCloseButton.Visible = isVisible;

            

            if (isVisible)
            {
                
                if (IsUserLoggedIn())
                {
                    favoriteButton.Enabled = true;
                    favoriteButton.Controls.Remove(disabledPanel); 
                }
                else
                {
                    favoriteButton.Enabled = false;
                    
                }

                
                overlayPanel.BringToFront();
                VodScreenPictureBox.BringToFront();

                
                vodButton.BringToFront();
                insertButton.BringToFront();
                albumButton.BringToFront();
                favoriteButton.BringToFront();
                vodScreenCloseButton.BringToFront();
            }
        }
    }
}