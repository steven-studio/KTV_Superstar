using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic; 
using IniParser;
using IniParser.Model;
using System.Text; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private PictureBox pictureBoxZhuYinSongs;

        private Button[] phoneticButtonsForSongs;
        private Button modifyButtonZhuYinSongs;
        private Button clearButtonZhuYinSongs;
        private Button closeButtonZhuYinSongs;
        private RichTextBox inputBoxZhuYinSongs;

        private void ZhuyinSearchSongsButton_Click(object sender, EventArgs e)
        {
            zhuyinSearchSongButton.BackgroundImage = zhuyinSearchSongActiveBackground;
            englishSearchSongButton.BackgroundImage = englishSearchSongNormalBackground;
            pinyinSearchSongButton.BackgroundImage = pinyinSearchSongNormalBackground;
            wordCountSearchSongButton.BackgroundImage = wordCountSearchSongNormalBackground;
            handWritingSearchSongButton.BackgroundImage = handWritingSearchSongNormalBackground;
            numberSearchSongButton.BackgroundImage = numberSearchSongNormalBackground;

            
            var configData = LoadConfigData();
            string imagePath = Path.Combine(Application.StartupPath, configData["ImagePaths"]["ZhuYinSongs"]);

            ShowImageOnPictureBoxZhuYinSongs(Path.Combine(Application.StartupPath, imagePath));
            
            
            
            SetZhuYinSingersAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetHandWritingForSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(true);
            pictureBoxZhuYinSongs.Visible = true;
        }

        private void InitializePhoneticButtonsForSongs()
        {
            var data = LoadConfigData();

            phoneticSymbols = LoadPhoneticSymbols(data);
            phoneticButtonCoords = LoadButtonCoordinates(data, "PhoneticButtonCoordinates", 35);
            phoneticButtonImages = LoadButtonImages(data, "PhoneticButtonImages", 35);

            phoneticButtonsForSongs = new Button[35];
            for (int i = 0; i < 35; i++)
            {
                var buttonImages = phoneticButtonImages[$"button{i}"];
                CreatePhoneticButtonForSongs(i, buttonImages.normal, buttonImages.mouseDown, buttonImages.mouseOver);
            }
        }

        private void CreatePhoneticButtonForSongs(int index, string normalImagePath, string mouseDownImagePath, string mouseOverImagePath)
        {
            try
            {
                

                phoneticButtonsForSongs[index] = new Button
                {
                    Name = $"phoneticButton_{phoneticSymbols[index]}",
                    BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, normalImagePath)),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 }
                };

                ResizeAndPositionButton(phoneticButtonsForSongs[index], phoneticButtonCoords[index].X, phoneticButtonCoords[index].Y, 
                                        phoneticButtonCoords[index].Width, phoneticButtonCoords[index].Height);

                Image normalImage = Image.FromFile(Path.Combine(Application.StartupPath, normalImagePath));
                Image mouseDownImage = Image.FromFile(Path.Combine(Application.StartupPath, mouseDownImagePath));
                Image mouseOverImage = Image.FromFile(Path.Combine(Application.StartupPath, mouseOverImagePath));

                phoneticButtonsForSongs[index].MouseDown += (s, e) => phoneticButtonsForSongs[index].BackgroundImage = mouseDownImage;
                phoneticButtonsForSongs[index].MouseUp += (s, e) => phoneticButtonsForSongs[index].BackgroundImage = normalImage;
                phoneticButtonsForSongs[index].MouseEnter += (s, e) => phoneticButtonsForSongs[index].BackgroundImage = mouseOverImage;
                phoneticButtonsForSongs[index].MouseLeave += (s, e) => phoneticButtonsForSongs[index].BackgroundImage = normalImage;
                phoneticButtonsForSongs[index].Click += PhoneticButton_Click;
                phoneticButtonsForSongs[index].Tag = phoneticSymbols[index];

                this.Controls.Add(phoneticButtonsForSongs[index]);

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating button at index {index}: {ex.Message}");
            }
        }

        private void InitializeButtonsForZhuYinSongs()
        {
            LoadPhoneticSymbolsFromConfig();
            InitializePhoneticButtonsForSongs();
            InitializeSpecialButtonsForZhuYinSongs();
            InitializeInputBoxZhuYinSongs();
        }

        private void InitializeSpecialButtonsForZhuYinSongs()
        {
            
            InitializeModifyButtonZhuYinSongs();

            
            InitializeClearButtonZhuYinSongs();

            
            InitializeCloseButtonZhuYinSongs();
        }

        private void InitializeModifyButtonZhuYinSongs()
        {
            var data = LoadConfigData();
            modifyButtonZhuYinCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "modifyButtonZhuYinSongs");
            var buttonImages = LoadButtonImages(data, "ModifyButtonImagesZhuYin");

            modifyButtonZhuYinSongs = CreateSpecialButton(
                "btnModifyZhuYinSongs",
                modifyButtonZhuYinCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ModifyButtonZhuYinSongs_Click
            );
        }

        private void ModifyButtonZhuYinSongs_Click(object sender, EventArgs e)
        {
            
            if (this.Controls.Contains(inputBoxZhuYinSongs) && inputBoxZhuYinSongs.Text.Length > 0)
            {
                inputBoxZhuYinSongs.Text = inputBoxZhuYinSongs.Text.Substring(0, inputBoxZhuYinSongs.Text.Length - 1);
            }
        }

        private void InitializeClearButtonZhuYinSongs()
        {
            var data = LoadConfigData();
            clearButtonZhuYinCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "clearButtonZhuYinSongs");
            var buttonImages = LoadButtonImages(data, "ClearButtonImagesZhuYin");

            clearButtonZhuYinSongs = CreateSpecialButton(
                "btnClearZhuYinSongs",
                clearButtonZhuYinCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ClearButtonZhuYinSongs_Click
            );
        }

        private void ClearButtonZhuYinSongs_Click(object sender, EventArgs e)
        {            
            if (this.Controls.Contains(inputBoxZhuYinSongs) && inputBoxZhuYinSongs.Text.Length > 0)
            {
                inputBoxZhuYinSongs.Text = "";
            }
        }

        private void InitializeCloseButtonZhuYinSongs()
        {
            var data = LoadConfigData();
            closeButtonZhuYinCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "closeButtonZhuYinSongs");
            var buttonImages = LoadButtonImages(data, "CloseButtonImagesZhuYin");

            closeButtonZhuYinSongs = CreateSpecialButton(
                "btnCloseZhuYinSongs",
                closeButtonZhuYinCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                CloseButtonZhuYinSongs_Click
            );
        }

        private void CloseButtonZhuYinSongs_Click(object sender, EventArgs e)
        {
            pictureBoxZhuYinSongs.Visible = false;
            SetZhuYinSongsAndButtonsVisibility(false);
        }

        private void InitializeInputBoxZhuYinSongs()
        {
            try
            {
                LoadInputBoxConfig();

                inputBoxZhuYinSongs = new RichTextBox
                {
                    Name = "inputBoxZhuYinSongs",
                    ForeColor = inputBoxForeColor,
                    Font = new Font(inputBoxFontName, inputBoxFontSize, inputBoxFontStyle),
                    ScrollBars = RichTextBoxScrollBars.None 
                };

                ResizeAndPositionControl(inputBoxZhuYinSongs, inputBoxZhuYinCoords.X, inputBoxZhuYinCoords.Y, inputBoxZhuYinCoords.Width, inputBoxZhuYinCoords.Height);

                inputBoxZhuYinSongs.TextChanged += (sender, e) =>
                {
                    string searchText = inputBoxZhuYinSongs.Text;
                    var searchResults = allSongs.Where(song => song.PhoneticNotation.StartsWith(searchText)).ToList();
                    currentPage = 0;
                    currentSongList = searchResults;
                    totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                    multiPagePanel.currentPageIndex = 0;
                    multiPagePanel.LoadSongs(currentSongList);
                };

                this.Controls.Add(inputBoxZhuYinSongs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing inputBoxZhuYinSongs: " + ex.Message);
            }
        }

        private (int X, int Y, int Width, int Height) pictureBoxZhuYinSongCoords;

        private void LoadPictureBoxZhuYinSongCoordsFromConfig()
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("config.ini");

            var coords = data["PictureBoxZhuYinSongs"];
            pictureBoxZhuYinSongCoords = (
                int.Parse(coords["X"]),
                int.Parse(coords["Y"]),
                int.Parse(coords["Width"]),
                int.Parse(coords["Height"])
            );
        }

        private void ShowImageOnPictureBoxZhuYinSongs(string imagePath)
        {
            
            LoadPictureBoxZhuYinSongCoordsFromConfig();

            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle displayArea = new Rectangle(pictureBoxZhuYinSongCoords.X, pictureBoxZhuYinSongCoords.Y, pictureBoxZhuYinSongCoords.Width, pictureBoxZhuYinSongCoords.Height);

            
            pictureBoxZhuYinSongs.Image = originalImage;

            
            ResizeAndPositionPictureBox(pictureBoxZhuYinSongs, displayArea.X, displayArea.Y, displayArea.Width, displayArea.Height);

            pictureBoxZhuYinSongs.Visible = true;
        }

        private void SetZhuYinSongsAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                try
                {
                    SuspendLayout();

                    if (pictureBoxZhuYinSongs == null)
                    {
                        Console.WriteLine("pictureBoxZhuYinSongs is null");
                    }
                    else
                    {
                        pictureBoxZhuYinSongs.Visible = isVisible;
                        if (isVisible) pictureBoxZhuYinSongs.BringToFront();
                    }

                    if (phoneticButtonsForSongs == null)
                    {
                        Console.WriteLine("phoneticButtonsForSongs is null");
                    }
                    else
                    {
                        foreach (var button in phoneticButtonsForSongs)
                        {
                            if (button == null)
                            {
                                Console.WriteLine("One of the phoneticButtonsForSongs is null");
                            }
                            else
                            {
                                button.Visible = isVisible;
                                if (isVisible) button.BringToFront();
                            }
                        }
                    }

                    if (modifyButtonZhuYinSongs == null)
                    {
                        Console.WriteLine("modifyButtonZhuYinSongs is null");
                    }
                    else
                    {
                        modifyButtonZhuYinSongs.Visible = isVisible;
                        if (isVisible) modifyButtonZhuYinSongs.BringToFront();
                    }

                    if (clearButtonZhuYinSongs == null)
                    {
                        Console.WriteLine("clearButtonZhuYinSongs is null");
                    }
                    else
                    {
                        clearButtonZhuYinSongs.Visible = isVisible;
                        if (isVisible) clearButtonZhuYinSongs.BringToFront();
                    }

                    if (closeButtonZhuYinSongs == null)
                    {
                        Console.WriteLine("closeButtonZhuYinSongs is null");
                    }
                    else
                    {
                        closeButtonZhuYinSongs.Visible = isVisible;
                        if (isVisible) closeButtonZhuYinSongs.BringToFront();
                    }

                    if (inputBoxZhuYinSongs == null)
                    {
                        Console.WriteLine("inputBoxZhuYinSongs is null");
                    }
                    else
                    {
                        inputBoxZhuYinSongs.Visible = isVisible;
                        if (isVisible) inputBoxZhuYinSongs.BringToFront();
                    }

                    ResumeLayout();
                    PerformLayout(); 

                    
                    pictureBoxZhuYinSongs?.Refresh();
                    if (phoneticButtonsForSongs != null)
                    {
                        foreach (var button in phoneticButtonsForSongs)
                        {
                            button?.Refresh();
                        }
                    }
                    modifyButtonZhuYinSongs?.Refresh();
                    clearButtonZhuYinSongs?.Refresh();
                    closeButtonZhuYinSongs?.Refresh();
                    inputBoxZhuYinSongs?.Refresh();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in SetZhuYinSongsAndButtonsVisibility: " + ex.Message);
                }
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