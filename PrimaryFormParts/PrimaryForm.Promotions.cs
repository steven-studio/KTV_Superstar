using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button promotionsButton;
        private Bitmap promotionsNormalBackground;
        private Bitmap promotionsActiveBackground;
        private Button previousPromotionButton;
        private Button nextPromotionButton;
        private Button closePromotionsButton;
        
        private void InitializePromotionsButton()
        {
            try 
            {
                // 設定基礎位置和間距
                int baseX = Screen.PrimaryScreen.Bounds.Width - 300;
                int baseY = Screen.PrimaryScreen.Bounds.Height - 120;
                int buttonSpacing = 90;

                // 共用的按鈕設置
                void ConfigurePromotionButton(Button button, string imagePath, Point location)
                {
                    button.Size = new Size(80, 80);
                    button.BackColor = Color.Transparent;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    button.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    button.Location = location;
                    
                    using (var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(Application.StartupPath, imagePath))))
                    {
                        var image = Image.FromStream(stream);
                        if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                        {
                            var bitmap = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            using (var g = Graphics.FromImage(bitmap))
                            {
                                g.Clear(Color.Transparent);
                                g.DrawImage(image, 0, 0, image.Width, image.Height);
                            }
                            button.BackgroundImage = bitmap;
                        }
                        else
                        {
                            button.BackgroundImage = image;
                        }
                    }
                    button.BackgroundImageLayout = ImageLayout.Stretch;
                }

                // 配置各個按鈕
                previousPromotionButton = new Button { Name = "previousPromotionButton", Visible = true };
                ConfigurePromotionButton(previousPromotionButton, "themes\\superstar\\上一頁.png", new Point(baseX, baseY));
                previousPromotionButton.Click += PreviousPromotionButton_Click;

                closePromotionsButton = new Button { Name = "closePromotionsButton", Visible = false };
                ConfigurePromotionButton(closePromotionsButton, "themes\\superstar\\退出.png", new Point(baseX + buttonSpacing, baseY));
                closePromotionsButton.Click += ClosePromotionsButton_Click;

                nextPromotionButton = new Button { Name = "nextPromotionButton", Visible = true };
                ConfigurePromotionButton(nextPromotionButton, "themes\\superstar\\下一頁.png", new Point(baseX + (buttonSpacing * 2), baseY));
                nextPromotionButton.Click += NextPromotionButton_Click;

                this.Controls.Add(previousPromotionButton);
                this.Controls.Add(closePromotionsButton);
                this.Controls.Add(nextPromotionButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化按鈕時發生錯誤: {ex.Message}");
            }
        }

        private List<Image> LoadPromotionsImages()
        {
            List<Image> images = new List<Image>();
            string newsFolderPath = Path.Combine(Application.StartupPath, "news");

            string[] imageFiles = Directory.GetFiles(newsFolderPath, "*.jpg");

            foreach (string filePath in imageFiles)
            {
                try
                {
                    images.Add(Image.FromFile(filePath));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading image: " + filePath + ". Exception: " + ex.Message);
                }
            }

            return images;
        }

        private void promotionsButton_Click(object sender, EventArgs e)
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
            promotionsButton.BackgroundImage = promotionsActiveBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;
            isOnOrderedSongsPage = false;
            
            
            promotionsAndMenuPanel.LoadImages(promotions); 
            promotionsAndMenuPanel.Visible = true;
            promotionsAndMenuPanel.BringToFront();

            previousPromotionButton.Visible = true;
            previousPromotionButton.BringToFront();
            nextPromotionButton.Visible = true;
            nextPromotionButton.BringToFront();
            
            closePromotionsButton.Visible = true;
            closePromotionsButton.BringToFront();

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }

            SetPictureBoxToggleLightAndButtonsVisibility(false);
        }

        private void PreviousPromotionButton_Click(object sender, EventArgs e)
        {
            
            
            
            
            
            promotionsAndMenuPanel.LoadPreviousPage();
        }

        private void NextPromotionButton_Click(object sender, EventArgs e)
        {
            
            
            
            
            
            promotionsAndMenuPanel.LoadNextPage();
        }

        private void ClosePromotionsButton_Click(object sender, EventArgs e)
        {
            
            promotionsAndMenuPanel.Visible = false;
            previousPromotionButton.Visible = false;
            nextPromotionButton.Visible = false;
            closePromotionsButton.Visible = false;

            HotPlayButton_Click(sender, e);
        }
    }
}