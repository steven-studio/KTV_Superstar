using System;
using System.IO;
using System.Drawing; 
using System.Linq; 
using System.Windows.Forms; 
using IniParser;
using IniParser.Model;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private PictureBox pictureBoxPinYinSingers;
        private Button[] letterButtonsForPinYinSingers;
        private Button modifyButtonPinYinSingers;
        private Button clearButtonPinYinSingers;
        private Button closeButtonPinYinSingers;

        private (int X, int Y, int Width, int Height) modifyButtonPinYinCoords;
        private (int X, int Y, int Width, int Height) clearButtonPinYinCoords;
        private (int X, int Y, int Width, int Height) closeButtonPinYinCoords;

        private RichTextBox inputBoxPinYinSingers;

        private void PinyinSingerSearchButton_Click(object sender, EventArgs e)
        {
            zhuyinSearchButton.BackgroundImage = zhuyinSearchNormalBackground;
            englishSearchButton.BackgroundImage = englishSearchNormalBackground;
            pinyinSearchButton.BackgroundImage = pinyinSearchActiveBackground;
            wordCountSearchButton.BackgroundImage = wordCountSearchNormalBackground;
            handWritingSearchButton.BackgroundImage = handWritingSearchNormalBackground;

            
            var configData = LoadConfigData();
            string pinyinImagePath = Path.Combine(Application.StartupPath, configData["ImagePaths"]["PinYinSingers"]);

            ShowImageOnPictureBoxPinYinSingers(Path.Combine(Application.StartupPath, pinyinImagePath));

            
            
            SetZhuYinSingersAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(true);
            SetPictureBoxArtistSearchAndButtonsVisibility(false);
            pictureBoxPinYinSingers.Visible = true;
        }

        private void InitializeLetterButtonsForPinYinSingers()
        {
            var data = LoadConfigData();
            var buttonImages = LoadButtonImages(data, "PinYinLetterButtonImages", 26);
            string qwertyLayout = "QWERTYUIOPASDFGHJKLZXCVBNM";
            letterButtonsForPinYinSingers = new Button[26];

            for (int i = 0; i < 26; i++)
            {
                var coords = data["PinYinLetterButtonCoordinates"][$"button{i}"].Split(',');
                letterButtonsForPinYinSingers[i] = CreateButton(
                    $"letterButton_{qwertyLayout[i]}",
                    (int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]), int.Parse(coords[3])),
                    buttonImages[$"button{i}"].normal,
                    buttonImages[$"button{i}"].mouseDown,
                    buttonImages[$"button{i}"].mouseOver,
                    LetterButtonPinYinSingers_Click
                );
                letterButtonsForPinYinSingers[i].Tag = qwertyLayout[i];
                this.Controls.Add(letterButtonsForPinYinSingers[i]);
            }
        }

        private void LetterButtonPinYinSingers_Click(object sender, EventArgs e)
        {
            
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                if (inputBoxPinYinSingers.Visible)
                {
                    inputBoxPinYinSingers.Text += button.Tag.ToString();
                }
            }
        }

        private void InitializeButtonsForPinYinSingers()
        {
            InitializeLetterButtonsForPinYinSingers();
            InitializeSpecialButtonsForPinYinSingers();
            InitializeInputBoxPinYinSingers();
        }

        private void InitializeSpecialButtonsForPinYinSingers()
        {
            
            InitializeModifyButtonPinYinSingers();

            
            InitializeClearButtonPinYinSingers();

            
            InitializeCloseButtonPinYinSingers();
        }

        private void InitializeModifyButtonPinYinSingers()
        {
            var data = LoadConfigData();
            modifyButtonPinYinCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "modifyButtonPinYinSingers");
            var buttonImages = LoadButtonImages(data, "ModifyButtonImagesPinYin");

            modifyButtonPinYinSingers = CreateSpecialButton(
                "btnModifyPinYinSingers",
                modifyButtonPinYinCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ModifyButtonPinYinSingers_Click
            );
        }

        private void ModifyButtonPinYinSingers_Click(object sender, EventArgs e)
        {
            
            if (this.Controls.Contains(inputBoxPinYinSingers) && inputBoxPinYinSingers.Text.Length > 0)
            {
                inputBoxPinYinSingers.Text = inputBoxPinYinSingers.Text.Substring(0, inputBoxPinYinSingers.Text.Length - 1);
            }
        }

        private void InitializeClearButtonPinYinSingers()
        {
            var data = LoadConfigData();
            clearButtonPinYinCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "clearButtonPinYinSingers");
            var buttonImages = LoadButtonImages(data, "ClearButtonImagesPinYin");

            clearButtonPinYinSingers = CreateSpecialButton(
                "btnClearPinYinSingers",
                clearButtonPinYinCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ClearButtonPinYinSingers_Click
            );
        }

        private void ClearButtonPinYinSingers_Click(object sender, EventArgs e)
        {            
            if (this.Controls.Contains(inputBoxPinYinSingers) && inputBoxPinYinSingers.Text.Length > 0)
            {
                inputBoxPinYinSingers.Text = "";
            }
        }

        private void InitializeCloseButtonPinYinSingers()
        {
            var data = LoadConfigData();
            closeButtonPinYinCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "closeButtonPinYinSingers");
            var buttonImages = LoadButtonImages(data, "CloseButtonImagesPinYin");

            closeButtonPinYinSingers = CreateSpecialButton(
                "btnClosePinYinSingers",
                closeButtonPinYinCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                CloseButtonPinYinSingers_Click
            );
        }

        private void CloseButtonPinYinSingers_Click(object sender, EventArgs e)
        {
            pictureBoxPinYinSingers.Visible = false;
            SetPinYinSingersAndButtonsVisibility(false);
        }

        private void InitializeInputBoxPinYinSingers()
        {
            try
            {
                var parser = new FileIniDataParser();
                parser.Parser.Configuration.AssigmentSpacer = "";
                parser.Parser.Configuration.CommentString = "#";
                parser.Parser.Configuration.CaseInsensitive = true;

                
                IniData data;
                using (var reader = new StreamReader("config.ini", System.Text.Encoding.UTF8))
                {
                    data = parser.ReadData(reader);
                }

                int x = int.Parse(data["InputBoxPinYinSingers"]["X"]);
                int y = int.Parse(data["InputBoxPinYinSingers"]["Y"]);
                int width = int.Parse(data["InputBoxPinYinSingers"]["Width"]);
                int height = int.Parse(data["InputBoxPinYinSingers"]["Height"]);
                string fontName = data["InputBoxPinYinSingers"]["FontName"];
                float fontSize = float.Parse(data["InputBoxPinYinSingers"]["FontSize"]);
                FontStyle fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), data["InputBoxPinYinSingers"]["FontStyle"]);
                Color foreColor = Color.FromName(data["InputBoxPinYinSingers"]["ForeColor"]);

                inputBoxPinYinSingers = new RichTextBox
                {
                    Visible = false,
                    Name = "inputBoxPinYinSingers",
                    ForeColor = foreColor,
                    Font = new Font(fontName, fontSize / 900 * Screen.PrimaryScreen.Bounds.Height, fontStyle)
                };

                ResizeAndPositionControl(inputBoxPinYinSingers, x, y, width, height);

                inputBoxPinYinSingers.TextChanged += (sender, e) =>
                {
                    string searchText = inputBoxPinYinSingers.Text;
                    var searchResults = allSongs.Where(song => song.ArtistAPinyin.Replace(" ", "").StartsWith(searchText))
                                                .Union(allSongs.Where(song => song.ArtistBPinyin.Replace(" ", "").StartsWith(searchText)))
                                                .ToList();
                    currentPage = 0;
                    currentSongList = searchResults;
                    totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                    multiPagePanel.currentPageIndex = 0;
                    multiPagePanel.LoadSongs(currentSongList);

                    
                };

                this.Controls.Add(inputBoxPinYinSingers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private (int X, int Y, int Width, int Height) pictureBoxPinYinSingerCoords;

        private void LoadPictureBoxPinYinSingerCoordsFromConfig()
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("config.ini");

            var coords = data["PictureBoxPinYinSingers"];
            pictureBoxPinYinSingerCoords = (
                int.Parse(coords["X"]),
                int.Parse(coords["Y"]),
                int.Parse(coords["Width"]),
                int.Parse(coords["Height"])
            );
        }

        private void ShowImageOnPictureBoxPinYinSingers(string imagePath)
        {
            
            LoadPictureBoxPinYinSingerCoordsFromConfig();

            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle displayArea = new Rectangle(pictureBoxPinYinSingerCoords.X, pictureBoxPinYinSingerCoords.Y, pictureBoxPinYinSingerCoords.Width, pictureBoxPinYinSingerCoords.Height);

            
            pictureBoxPinYinSingers.Image = originalImage;

            
            ResizeAndPositionPictureBox(pictureBoxPinYinSingers, displayArea.X, displayArea.Y, displayArea.Width, displayArea.Height);
            
            pictureBoxPinYinSingers.Visible = true;
        }

        private void SetPinYinSingersAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                SuspendLayout();

                if (pictureBoxPinYinSingers == null)
                {
                    Console.WriteLine("pictureBoxPinYinSingers is null");
                }
                else
                {
                    pictureBoxPinYinSingers.Visible = isVisible;
                    if (isVisible) pictureBoxPinYinSingers.BringToFront();
                    pictureBoxPinYinSingers.Refresh();
                }

                if (letterButtonsForPinYinSingers == null)
                {
                    Console.WriteLine("letterButtonsForPinYinSingers is null");
                }
                else
                {
                    foreach (var button in letterButtonsForPinYinSingers)
                    {
                        if (button == null)
                        {
                            Console.WriteLine("A button in letterButtonsForPinYinSingers is null");
                        }
                        else
                        {
                            button.Visible = isVisible;
                            if (isVisible) button.BringToFront();
                            button.Refresh();
                        }
                    }
                }

                if (modifyButtonPinYinSingers == null)
                {
                    Console.WriteLine("modifyButtonPinYinSingers is null");
                }
                else
                {
                    modifyButtonPinYinSingers.Visible = isVisible;
                    if (isVisible) modifyButtonPinYinSingers.BringToFront();
                    modifyButtonPinYinSingers.Refresh();
                }

                if (clearButtonPinYinSingers == null)
                {
                    Console.WriteLine("clearButtonPinYinSingers is null");
                }
                else
                {
                    clearButtonPinYinSingers.Visible = isVisible;
                    if (isVisible) clearButtonPinYinSingers.BringToFront();
                    clearButtonPinYinSingers.Refresh();
                }

                if (closeButtonPinYinSingers == null)
                {
                    Console.WriteLine("closeButtonPinYinSingers is null");
                }
                else
                {
                    closeButtonPinYinSingers.Visible = isVisible;
                    if (isVisible) closeButtonPinYinSingers.BringToFront();
                    closeButtonPinYinSingers.Refresh();
                }

                if (inputBoxPinYinSingers == null)
                {
                    Console.WriteLine("inputBoxPinYinSingers is null");
                }
                else
                {
                    inputBoxPinYinSingers.Visible = isVisible;
                    if (isVisible) inputBoxPinYinSingers.BringToFront();
                    inputBoxPinYinSingers.Refresh();
                }

                ResumeLayout();
                PerformLayout(); 
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