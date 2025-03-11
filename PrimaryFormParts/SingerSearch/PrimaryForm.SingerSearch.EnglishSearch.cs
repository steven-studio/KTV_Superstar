using System;
using System.IO; 
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms; 
using System.Drawing; 
using IniParser;
using IniParser.Model;
using System.Text; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private PictureBox pictureBoxEnglishSingers;
        
        private Button[] numberButtonsForSingers;
        private Button[] letterButtonsForEnglishSingers;
        private Button modifyButtonEnglishSingers;
        private Button clearButtonEnglishSingers;
        private Button closeButtonEnglishSingers;

        private (int X, int Y, int Width, int Height) modifyButtonEnglishCoords;
        private (int X, int Y, int Width, int Height) clearButtonEnglishCoords;
        private (int X, int Y, int Width, int Height) closeButtonEnglishCoords;

        private RichTextBox inputBoxEnglishSingers;

        private void EnglishSearchSingersButton_Click(object sender, EventArgs e)
        {
            zhuyinSearchButton.BackgroundImage = zhuyinSearchNormalBackground;
            englishSearchButton.BackgroundImage = englishSearchActiveBackground;
            pinyinSearchButton.BackgroundImage = pinyinSearchNormalBackground;
            wordCountSearchButton.BackgroundImage = wordCountSearchNormalBackground;
            handWritingSearchButton.BackgroundImage = handWritingSearchNormalBackground;

            bool shouldBeVisible = !pictureBoxEnglishSingers.Visible;

            
            var configData = LoadConfigData();
            string imagePath = Path.Combine(Application.StartupPath, configData["ImagePaths"]["EnglishSingers"]);

            ShowImageOnPictureBoxEnglishSingers(Path.Combine(Application.StartupPath, imagePath));

            SetZhuYinSingersAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetHandWritingForSingersAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(true);
            SetPictureBoxArtistSearchAndButtonsVisibility(false);
            pictureBoxEnglishSingers.Visible = true;
        }

        private (int X, int Y, int Width, int Height)[] numberButtonCoords;

        private void LoadNumberButtonCoordsFromConfig()
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("config.ini");

            var buttonList = new List<(int X, int Y, int Width, int Height)>();

            for (int i = 1; i <= 10; i++)
            {
                var coordString = data["NumberButtonCoordinates"][$"button{i}"];
                var coords = coordString.Split(',');
                buttonList.Add((int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]), int.Parse(coords[3])));
            }

            numberButtonCoords = buttonList.ToArray();
        }

        private Button CreateButton(string name, (int X, int Y, int Width, int Height) coords, string normalImagePath, string mouseDownImagePath, string mouseOverImagePath, EventHandler clickEventHandler)
        {
            var button = new Button
            {
                Name = name,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseDownBackColor = Color.Transparent, MouseOverBackColor = Color.Transparent },
                BackgroundImageLayout = ImageLayout.Stretch,
                BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, normalImagePath))
            };

            ResizeAndPositionButton(button, coords.X, coords.Y, coords.Width, coords.Height);

            button.MouseEnter += (sender, e) => button.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, mouseOverImagePath));
            button.MouseLeave += (sender, e) => button.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, normalImagePath));
            button.MouseDown += (sender, e) => button.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, mouseDownImagePath));
            button.MouseUp += (sender, e) => button.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, normalImagePath));

            button.Click += clickEventHandler;

            return button;
        }

        
        private void InitializeNumberButtonsForSingers()
        {
            var data = LoadConfigData();
            numberButtonCoords = LoadButtonCoordinates(data, "NumberButtonCoordinates", 10);
            var buttonImages = LoadButtonImages(data, "NumberButtonImages", 10);

            numberButtonsForSingers = new Button[10];
            for (int i = 0; i < 10; i++)
            {
                string normalImagePath = buttonImages[$"button{i}"].normal;
                string mouseDownImagePath = buttonImages[$"button{i}"].mouseDown;
                string mouseOverImagePath = buttonImages[$"button{i}"].mouseOver;

                
                if (normalImagePath == null || mouseDownImagePath == null || mouseOverImagePath == null)
                {
                    Console.WriteLine($"Error: One or more image paths for button{i} are null.");
                    continue; 
                }

                
                numberButtonsForSingers[i] = CreateButton(
                    $"numberButton_{i}",
                    numberButtonCoords[i],
                    normalImagePath,
                    mouseDownImagePath,
                    mouseOverImagePath,
                    NumberButtonForSingers_Click
                );
                numberButtonsForSingers[i].Tag = (i + 1) % 10;
                this.Controls.Add(numberButtonsForSingers[i]);
            }
        }

        private void NumberButtonForSingers_Click(object sender, EventArgs e)
        {
            
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                if (inputBoxEnglishSingers.Visible)
                {
                    inputBoxEnglishSingers.Text += button.Tag.ToString();
                }
            }
        }

        private void InitializeLetterButtonsForEnglishSingers()
        {
            var data = LoadConfigData();
            var buttonImages = LoadButtonImages(data, "EnglishLetterButtonImages", 26);
            string qwertyLayout = "QWERTYUIOPASDFGHJKLZXCVBNM";
            letterButtonsForEnglishSingers = new Button[26];

            for (int i = 0; i < 26; i++)
            {
                var coords = data["EnglishLetterButtonCoordinates"][$"button{i}"].Split(',');
                letterButtonsForEnglishSingers[i] = CreateButton(
                    $"letterButton_{qwertyLayout[i]}",
                    (int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]), int.Parse(coords[3])),
                    buttonImages[$"button{i}"].normal,
                    buttonImages[$"button{i}"].mouseDown,
                    buttonImages[$"button{i}"].mouseOver,
                    LetterButtonEnglishSingers_Click
                );
                letterButtonsForEnglishSingers[i].Tag = qwertyLayout[i];
                this.Controls.Add(letterButtonsForEnglishSingers[i]);
            }
        }

        private void LetterButtonEnglishSingers_Click(object sender, EventArgs e)
        {
            
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                if (inputBoxEnglishSingers.Visible)
                {
                    inputBoxEnglishSingers.Text += button.Tag.ToString();
                }
            }
        }

        private void InitializeButtonsForEnglishSingers()
        {
            InitializeNumberButtonsForSingers();
            InitializeLetterButtonsForEnglishSingers();

            
            InitializeModifyButtonEnglishSingers();

            
            InitializeClearButtonEnglishSingers();

            
            InitializeCloseButtonEnglishSingers();

            InitializeInputBoxEnglishSingers();
        }

        private void InitializeModifyButtonEnglishSingers()
        {
            var data = LoadConfigData();
            modifyButtonEnglishCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "modifyButtonEnglishSingers");
            var buttonImages = LoadButtonImages(data, "ModifyButtonImagesEnglish");

            modifyButtonEnglishSingers = CreateSpecialButton(
                "btnModifyEnglishSingers",
                modifyButtonEnglishCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ModifyButtonEnglishSingers_Click
            );

            this.Controls.Add(modifyButtonEnglishSingers);
        }

        private void ModifyButtonEnglishSingers_Click(object sender, EventArgs e)
        {
            
            if (this.Controls.Contains(inputBoxEnglishSingers) && inputBoxEnglishSingers.Text.Length > 0)
            {
                inputBoxEnglishSingers.Text = inputBoxEnglishSingers.Text.Substring(0, inputBoxEnglishSingers.Text.Length - 1);
            }
        }

        private void InitializeClearButtonEnglishSingers()
        {
            var data = LoadConfigData();
            clearButtonEnglishCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "clearButtonEnglishSingers");
            var buttonImages = LoadButtonImages(data, "ClearButtonImagesEnglish");

            clearButtonEnglishSingers = CreateSpecialButton(
                "btnClearEnglishSingers",
                clearButtonEnglishCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ClearButtonEnglishSingers_Click
            );

            this.Controls.Add(clearButtonEnglishSingers);
        }

        private void ClearButtonEnglishSingers_Click(object sender, EventArgs e)
        {
            if (this.Controls.Contains(inputBoxEnglishSingers) && inputBoxEnglishSingers.Text.Length > 0)
            {
                inputBoxEnglishSingers.Text = "";
            }
        }

        private void InitializeCloseButtonEnglishSingers()
        {
            var data = LoadConfigData();
            closeButtonEnglishCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "closeButtonEnglishSingers");
            var buttonImages = LoadButtonImages(data, "CloseButtonImagesEnglish");

            closeButtonEnglishSingers = CreateSpecialButton(
                "btnCloseEnglishSingers",
                closeButtonEnglishCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                CloseButtonEnglishSingers_Click
            );

            this.Controls.Add(closeButtonEnglishSingers);
        }

        private void CloseButtonEnglishSingers_Click(object sender, EventArgs e)
        {
            
            pictureBoxEnglishSingers.Visible = false;
            SetEnglishSingersAndButtonsVisibility(false);
        }

        private void InitializeInputBoxEnglishSingers()
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

                int x = int.Parse(data["InputBoxEnglishSingers"]["X"]);
                int y = int.Parse(data["InputBoxEnglishSingers"]["Y"]);
                int width = int.Parse(data["InputBoxEnglishSingers"]["Width"]);
                int height = int.Parse(data["InputBoxEnglishSingers"]["Height"]);
                string fontName = data["InputBoxEnglishSingers"]["FontName"];
                float fontSize = float.Parse(data["InputBoxEnglishSingers"]["FontSize"]);
                FontStyle fontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), data["InputBoxEnglishSingers"]["FontStyle"]);
                Color foreColor = Color.FromName(data["InputBoxEnglishSingers"]["ForeColor"]);

                inputBoxEnglishSingers = new RichTextBox
                {
                    Visible = false,
                    Name = "inputBoxEnglishSingers",
                    ForeColor = foreColor,
                    Font = new Font(fontName, fontSize / 900 * Screen.PrimaryScreen.Bounds.Height, fontStyle)
                };

                ResizeAndPositionControl(inputBoxEnglishSingers, x, y, width, height);

                inputBoxEnglishSingers.TextChanged += (sender, e) =>
                {
                    string searchText = inputBoxEnglishSingers.Text;
                    var searchResults = allArtists.Where(artist => artist.Name.Replace(" ", "").StartsWith(searchText)).ToList();
                    
                    currentPage = 0;
                    currentArtistList = searchResults;
                    totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                    multiPagePanel.currentPageIndex = 0;
                    multiPagePanel.LoadSingers(currentArtistList);
                };

                this.Controls.Add(inputBoxEnglishSingers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void ShowImageOnPictureBoxEnglishSingers(string imagePath)
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

                int x = int.Parse(data["PictureBoxEnglishSingers"]["X"]);
                int y = int.Parse(data["PictureBoxEnglishSingers"]["Y"]);
                int width = int.Parse(data["PictureBoxEnglishSingers"]["Width"]);
                int height = int.Parse(data["PictureBoxEnglishSingers"]["Height"]);

                
                Bitmap originalImage = new Bitmap(imagePath);

                
                pictureBoxEnglishSingers.Image = originalImage;
            
                
                ResizeAndPositionPictureBox(pictureBoxEnglishSingers, x, y, width, height);
                    
                pictureBoxEnglishSingers.Visible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void SetEnglishSingersAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                SuspendLayout();

                pictureBoxEnglishSingers.Visible = isVisible;
                if (isVisible) pictureBoxEnglishSingers.BringToFront();

                foreach (var button in numberButtonsForSingers)
                {
                    button.Visible = isVisible;
                    if (isVisible) button.BringToFront();
                }

                foreach (var button in letterButtonsForEnglishSingers)
                {
                    button.Visible = isVisible;
                    if (isVisible) button.BringToFront();
                }

                
                if (modifyButtonEnglishSingers != null)
                {
                    modifyButtonEnglishSingers.Visible = isVisible;
                    if (isVisible) modifyButtonEnglishSingers.BringToFront();
                }

                if (clearButtonEnglishSingers != null)
                {
                    clearButtonEnglishSingers.Visible = isVisible;
                    if (isVisible) clearButtonEnglishSingers.BringToFront();
                }

                closeButtonEnglishSingers.Visible = isVisible;
                if (isVisible) closeButtonEnglishSingers.BringToFront();

                inputBoxEnglishSingers.Visible = isVisible;
                if (isVisible) inputBoxEnglishSingers.BringToFront();

                ResumeLayout();
                PerformLayout(); 

                
                pictureBoxEnglishSingers.Refresh();
                foreach (var button in numberButtonsForSingers.Concat(letterButtonsForEnglishSingers))
                {
                    button.Refresh();
                }

                
                modifyButtonEnglishSingers.Refresh();
                clearButtonEnglishSingers.Refresh();
                closeButtonEnglishSingers.Refresh();
                inputBoxEnglishSingers.Refresh();
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