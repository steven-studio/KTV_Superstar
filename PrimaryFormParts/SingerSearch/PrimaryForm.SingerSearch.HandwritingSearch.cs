using System;
using System.Drawing;
using System.IO;
using System.Linq;  
using System.Windows.Forms;
using Microsoft.Ink;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private PictureBox pictureBoxHandWritingSingers;

        private Button refillButtonHandWritingSingers;
        private Button clearButtonHandWritingSingers;
        private Button closeButtonForSingers;

        private (int X, int Y, int Width, int Height) refillButtonHandWritingCoords;
        private (int X, int Y, int Width, int Height) clearButtonHandWritingCoords;
        private (int X, int Y, int Width, int Height) closeButtonHandWritingCoords;

        private void HandWritingSearchButtonForSingers_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            zhuyinSearchButton.BackgroundImage = zhuyinSearchNormalBackground;
            englishSearchButton.BackgroundImage = englishSearchNormalBackground;
            pinyinSearchButton.BackgroundImage = pinyinSearchNormalBackground;
            wordCountSearchButton.BackgroundImage = wordCountSearchNormalBackground;
            handWritingSearchButton.BackgroundImage = handWritingSearchActiveBackground;

            
            EnableDoubleBuffering(handWritingPanelForSingers);
            EnableDoubleBuffering(handwritingInputBoxForSingers);
            EnableDoubleBuffering(candidateListBoxForSingers);
            EnableDoubleBuffering(pictureBoxHandWritingSingers);
            EnableDoubleBuffering(refillButtonHandWritingSingers);
            EnableDoubleBuffering(closeButtonForSingers);

            
            var configData = LoadConfigData();
            string handWritingImagePath = Path.Combine(Application.StartupPath, configData["ImagePaths"]["HandWritingSingers"]);

            ShowImageOnPictureBoxHandWritingSingers(Path.Combine(Application.StartupPath, handWritingImagePath));
            SetZhuYinSingersAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPictureBoxArtistSearchAndButtonsVisibility(false);
            SetHandWritingForSingersAndButtonsVisibility(true);

            this.ResumeLayout();
        }
        
        private Panel handWritingPanelForSingers;
        private InkOverlay inkOverlayForSingers;
        private RichTextBox handwritingInputBoxForSingers;
        private ListBox candidateListBoxForSingers;


        private void InitializeHandWritingForSingers()
        {
            InitializeHandWritingPanelForSingers();
            InitializeInkOverlayForSingers();
            InitializeHandwritingInputBoxForSingers();
            InitializeCandidateListBoxForSingers();
            InitializeSpecialButtonsForHandWritingSingers();
        }
        
        private void InitializeHandWritingPanelForSingers()
        {
            
            handWritingPanelForSingers = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false 
            };

            
            ResizeAndPositionControl(handWritingPanelForSingers, 366, 448, 650, 260);

            
            this.Controls.Add(handWritingPanelForSingers);
        }

        private void InitializeInkOverlayForSingers()
        {
            try
            {
                
                inkOverlayForSingers = new InkOverlay(handWritingPanelForSingers);
                inkOverlayForSingers.Enabled = false;
                inkOverlayForSingers.Ink = new Ink();
                inkOverlayForSingers.DefaultDrawingAttributes.Color = Color.Black;
                inkOverlayForSingers.DefaultDrawingAttributes.Width = 100;
                inkOverlayForSingers.Stroke += new InkCollectorStrokeEventHandler(InkOverlayForSingers_Stroke);

                
                inkOverlayForSingers.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialize ink overlay for singers: " + ex.Message);
            }
        }

        private void InkOverlayForSingers_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            
            RecognizeInk(inkOverlayForSingers, candidateListBoxForSingers);
        }

        private void InitializeHandwritingInputBoxForSingers()
        {
            
            handwritingInputBoxForSingers = new RichTextBox
            {
                Font = new Font("微軟正黑體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular), 
                Visible = false
            };
            ResizeAndPositionControl(handwritingInputBoxForSingers, 366, 373, 541, 62);
            this.Controls.Add(handwritingInputBoxForSingers);

            handwritingInputBoxForSingers.TextChanged += (sender, e) =>
            {
                string searchText = handwritingInputBoxForSingers.Text;
                
                
                var searchResults = allArtists.Where(artist => artist.Name.StartsWith(searchText)).ToList();

                currentPage = 0;
                currentArtistList = searchResults;
                totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                multiPagePanel.currentPageIndex = 0;
                multiPagePanel.LoadSingers(currentArtistList);
            };
        }    

        private void InitializeCandidateListBoxForSingers()
        {
            
            candidateListBoxForSingers = new ListBox
            {
                Font = new Font("微軟正黑體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular),
                Visible = false
            };
            ResizeAndPositionControl(candidateListBoxForSingers, 350 + 679, 448, 115, 260);
            candidateListBoxForSingers.SelectedIndexChanged += CandidateListBoxForSingers_SelectedIndexChanged;
            this.Controls.Add(candidateListBoxForSingers);
        }

        private void CandidateListBoxForSingers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (candidateListBoxForSingers.SelectedIndex != -1)
            {
                string selectedWord = candidateListBoxForSingers.SelectedItem.ToString();
                handwritingInputBoxForSingers.Text += selectedWord;  
                candidateListBoxForSingers.Visible = false; 
                
                
                if (inkOverlayForSingers != null)
                {
                    inkOverlayForSingers.Ink.DeleteStrokes();
                    handWritingPanelForSingers.Invalidate(); 
                }
            }
        }

        private void ShowImageOnPictureBoxHandWritingSingers(string imagePath)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle displayArea = new Rectangle(350, 360, 810, 360);

            
            pictureBoxHandWritingSingers.Image = originalImage;

            
            ResizeAndPositionPictureBox(pictureBoxHandWritingSingers, displayArea.X, displayArea.Y, displayArea.Width, displayArea.Height);

            pictureBoxHandWritingSingers.Visible = true;
        }

        private void SetHandWritingForSingersAndButtonsVisibility(bool isVisible)
        {
            
            EnableDoubleBuffering(handWritingPanelForSingers);
            EnableDoubleBuffering(handwritingInputBoxForSingers);
            EnableDoubleBuffering(candidateListBoxForSingers);
            EnableDoubleBuffering(pictureBoxHandWritingSingers);
            EnableDoubleBuffering(refillButtonHandWritingSingers);
            EnableDoubleBuffering(clearButtonHandWritingSingers);
            EnableDoubleBuffering(closeButtonForSingers);

            
            handWritingPanelForSingers.Visible = isVisible;
            handwritingInputBoxForSingers.Visible = isVisible;
            inkOverlayForSingers.Enabled = isVisible;
            candidateListBoxForSingers.Visible = isVisible; 
            pictureBoxHandWritingSingers.Visible = isVisible;
            refillButtonHandWritingSingers.Visible = isVisible;
            clearButtonHandWritingSingers.Visible = isVisible;
            closeButtonForSingers.Visible = isVisible;

            if (isVisible)
            {
                
                pictureBoxHandWritingSingers.BringToFront();
                handWritingPanelForSingers.BringToFront();
                handwritingInputBoxForSingers.BringToFront();
                candidateListBoxForSingers.BringToFront();
                refillButtonHandWritingSingers.BringToFront();
                clearButtonHandWritingSingers.BringToFront();
                closeButtonForSingers.BringToFront();
            }
        }

        private void InitializeSpecialButtonsForHandWritingSingers()
        {
            
            InitializeRefillButtonHandwritingSingers();

            
            InitializeClearButtonHandWritingSingers();

            
            InitializeCloseButtonForSingers();
        }

        private void InitializeRefillButtonHandwritingSingers()
        {
            var data = LoadConfigData();
            refillButtonHandWritingCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "refillButtonHandWritingSingers");
            var buttonImages = LoadButtonImages(data, "RefillButtonImagesHandWriting");

            refillButtonHandWritingSingers = CreateSpecialButton(
                "refillButtonHandWritingSingers",
                refillButtonHandWritingCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                RefillButtonHandWritingSingers_Click
            );
        }

        private void RefillButtonHandWritingSingers_Click(object sender, EventArgs e)
        {
            handwritingInputBoxForSingers.Text = "";
        }

        private void InitializeClearButtonHandWritingSingers()
        {
            var data = LoadConfigData();
            clearButtonHandWritingCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "clearButtonHandWritingSingers");
            var buttonImages = LoadButtonImages(data, "ClearButtonImagesHandWriting");

            clearButtonHandWritingSingers = CreateSpecialButton(
                "clearButtonHandWritingSingers",
                clearButtonHandWritingCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                
                ClearButtonHandWritingSingers_Click
            );
        }

        private void ClearButtonHandWritingSingers_Click(object sender, EventArgs e)
        {
            if (this.Controls.Contains(handWritingPanelForSingers) && inkOverlayForSingers != null)
            {
                inkOverlayForSingers.Ink.DeleteStrokes();
                handWritingPanelForSingers.Invalidate(); 
            }
        }

        private void InitializeCloseButtonForSingers()
        {
            var data = LoadConfigData();
            closeButtonHandWritingCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "closeButtonForSingers");
            var buttonImages = LoadButtonImages(data, "CloseButtonImagesHandWriting");

            closeButtonForSingers = CreateSpecialButton(
                "closeButtonForSingers",
                closeButtonHandWritingCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                CloseButtonForSingers_Click
            );
        }

        private void CloseButtonForSingers_Click(object sender, EventArgs e)
        {
            
            this.SuspendLayout();

            SetHandWritingForSingersAndButtonsVisibility(false);

            
            this.ResumeLayout();
        }
    }
}