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
        private PictureBox pictureBoxHandWritingSongs;

        private Button refillButtonHandWritingSongs;
        private Button clearButtonHandWritingSongs;
        private Button closeButtonForSongs;

        private void HandWritingSearchButtonForSongs_Click(object sender, EventArgs e)
        {
            this.SuspendLayout();

            zhuyinSearchSongButton.BackgroundImage = zhuyinSearchSongNormalBackground;
            englishSearchSongButton.BackgroundImage = englishSearchSongNormalBackground;
            pinyinSearchSongButton.BackgroundImage = pinyinSearchSongNormalBackground;
            wordCountSearchSongButton.BackgroundImage = wordCountSearchSongNormalBackground;
            handWritingSearchSongButton.BackgroundImage = handWritingSearchSongActiveBackground;
            numberSearchSongButton.BackgroundImage = numberSearchSongNormalBackground;

            
            EnableDoubleBuffering(handWritingPanelForSongs);
            EnableDoubleBuffering(handwritingInputBoxForSongs);
            EnableDoubleBuffering(candidateListBoxForSongs);
            EnableDoubleBuffering(pictureBoxHandWritingSongs);
            EnableDoubleBuffering(refillButtonHandWritingSongs);
            EnableDoubleBuffering(closeButtonForSongs);

            
            var configData = LoadConfigData();
            string handWritingImagePath = Path.Combine(Application.StartupPath, configData["ImagePaths"]["HandWritingSongs"]);

            ShowImageOnPictureBoxHandWritingSongs(Path.Combine(Application.StartupPath, handWritingImagePath));
            
            SetZhuYinSingersAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetHandWritingForSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetHandWritingForSongsAndButtonsVisibility(true);

            this.ResumeLayout();
        }
        
        private Panel handWritingPanelForSongs;
        private InkOverlay inkOverlayForSongs;
        private RichTextBox handwritingInputBoxForSongs;
        private ListBox candidateListBoxForSongs;


        private void InitializeHandWritingForSongs()
        {
            InitializeHandWritingPanelForSongs();
            InitializeInkOverlayForSongs();
            InitializeHandwritingInputBoxForSongs();
            InitializeCandidateListBoxForSongs();
            InitializeSpecialButtonsForHandWritingSongs();
        }
        
        private void InitializeHandWritingPanelForSongs()
        {
            
            handWritingPanelForSongs = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false 
            };

            
            ResizeAndPositionControl(handWritingPanelForSongs, 366, 448, 650, 260);

            
            this.Controls.Add(handWritingPanelForSongs);
        }

        private void InitializeInkOverlayForSongs()
        {
            try
            {
                
                inkOverlayForSongs = new InkOverlay(handWritingPanelForSongs);
                inkOverlayForSongs.Enabled = false;
                inkOverlayForSongs.Ink = new Ink();
                inkOverlayForSongs.DefaultDrawingAttributes.Color = Color.Black;
                inkOverlayForSongs.DefaultDrawingAttributes.Width = 100;
                inkOverlayForSongs.Stroke += new InkCollectorStrokeEventHandler(InkOverlayForSongs_Stroke);

                
                inkOverlayForSongs.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialize ink overlay for singers: " + ex.Message);
            }
        }

        private void InkOverlayForSongs_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            
            RecognizeInk(inkOverlayForSongs, candidateListBoxForSongs);
        }

        private void InitializeHandwritingInputBoxForSongs()
        {
            
            handwritingInputBoxForSongs = new RichTextBox
            {
                Font = new Font("微軟正黑體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular), 
                Visible = false
            };
            ResizeAndPositionControl(handwritingInputBoxForSongs, 366, 373, 541, 62);
            this.Controls.Add(handwritingInputBoxForSongs);

            handwritingInputBoxForSongs.TextChanged += (sender, e) =>
            {
                string searchText = handwritingInputBoxForSongs.Text;
                
                
                var searchResults = allSongs.Where(song => song.Song.StartsWith(searchText)).ToList();
                currentPage = 0;
                currentSongList = searchResults;
                totalPages = (int)Math.Ceiling((double)searchResults.Count / itemsPerPage);

                multiPagePanel.currentPageIndex = 0;
                multiPagePanel.LoadSongs(currentSongList);
            };
        }    

        private void InitializeCandidateListBoxForSongs()
        {
            
            candidateListBoxForSongs = new ListBox
            {
                Font = new Font("微軟正黑體", (float)26 / 900 * Screen.PrimaryScreen.Bounds.Height, FontStyle.Regular),
                Visible = false
            };
            ResizeAndPositionControl(candidateListBoxForSongs, 350 + 679, 448, 115, 260);
            candidateListBoxForSongs.SelectedIndexChanged += CandidateListBoxForSongs_SelectedIndexChanged;
            this.Controls.Add(candidateListBoxForSongs);
        }

        private void CandidateListBoxForSongs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (candidateListBoxForSongs.SelectedIndex != -1)
            {
                string selectedWord = candidateListBoxForSongs.SelectedItem.ToString();
                handwritingInputBoxForSongs.Text += selectedWord;  
                candidateListBoxForSongs.Visible = false; 
                
                
                if (inkOverlayForSongs != null)
                {
                    inkOverlayForSongs.Ink.DeleteStrokes();
                    handWritingPanelForSongs.Invalidate(); 
                }
            }
        }

        private void ShowImageOnPictureBoxHandWritingSongs(string imagePath)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle displayArea = new Rectangle(350, 360, 810, 360);

            
            pictureBoxHandWritingSongs.Image = originalImage;

            
            ResizeAndPositionPictureBox(pictureBoxHandWritingSongs, displayArea.X, displayArea.Y, displayArea.Width, displayArea.Height);

            pictureBoxHandWritingSongs.Visible = true;
        }

        private void SetHandWritingForSongsAndButtonsVisibility(bool isVisible)
        {
            
            EnableDoubleBuffering(handWritingPanelForSongs);
            EnableDoubleBuffering(handwritingInputBoxForSongs);
            EnableDoubleBuffering(candidateListBoxForSongs);
            EnableDoubleBuffering(pictureBoxHandWritingSongs);
            EnableDoubleBuffering(refillButtonHandWritingSongs);
            EnableDoubleBuffering(clearButtonHandWritingSongs);
            EnableDoubleBuffering(closeButtonForSongs);

            
            handWritingPanelForSongs.Visible = isVisible;
            handwritingInputBoxForSongs.Visible = isVisible;
            inkOverlayForSongs.Enabled = isVisible;
            candidateListBoxForSongs.Visible = isVisible; 
            pictureBoxHandWritingSongs.Visible = isVisible;
            refillButtonHandWritingSongs.Visible = isVisible;
            clearButtonHandWritingSongs.Visible = isVisible;
            closeButtonForSongs.Visible = isVisible;

            if (isVisible)
            {
                
                pictureBoxHandWritingSongs.BringToFront();
                handWritingPanelForSongs.BringToFront();
                handwritingInputBoxForSongs.BringToFront();
                candidateListBoxForSongs.BringToFront();
                refillButtonHandWritingSongs.BringToFront();
                clearButtonHandWritingSongs.BringToFront();
                closeButtonForSongs.BringToFront();
            }
        }

        private void InitializeSpecialButtonsForHandWritingSongs()
        {
            
            InitializeRefillButtonHandwritingSongs();

            
            InitializeClearButtonHandWritingSongs();

            
            InitializeCloseButtonForSongs();
        }

        private void InitializeRefillButtonHandwritingSongs()
        {
            var data = LoadConfigData();
            refillButtonHandWritingCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "refillButtonHandWritingSongs");
            var buttonImages = LoadButtonImages(data, "RefillButtonImagesHandWriting");

            refillButtonHandWritingSongs = CreateSpecialButton(
                "refillButtonHandWritingSongs",
                refillButtonHandWritingCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                RefillButtonHandWritingSongs_Click
            );
        }

        private void RefillButtonHandWritingSongs_Click(object sender, EventArgs e)
        {
            handwritingInputBoxForSongs.Text = "";
        }

        private void InitializeClearButtonHandWritingSongs()
        {
            var data = LoadConfigData();
            clearButtonHandWritingCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "clearButtonHandWritingSongs");
            var buttonImages = LoadButtonImages(data, "ClearButtonImagesHandWriting");

            clearButtonHandWritingSongs = CreateSpecialButton(
                "clearButtonHandWritingSongs",
                clearButtonHandWritingCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                ClearButtonHandWritingSongs_Click
            );
        }

        private void ClearButtonHandWritingSongs_Click(object sender, EventArgs e)
        {
            if (this.Controls.Contains(handWritingPanelForSongs) && inkOverlayForSongs != null)
            {
                inkOverlayForSongs.Ink.DeleteStrokes();
                handWritingPanelForSongs.Invalidate(); 
            }
        }

        private void InitializeCloseButtonForSongs()
        {
            var data = LoadConfigData();
            closeButtonHandWritingCoords = LoadSpecialButtonCoordinates(data, "SpecialButtonCoordinates", "closeButtonForSongs");
            var buttonImages = LoadButtonImages(data, "CloseButtonImagesHandWriting");

            closeButtonForSongs = CreateSpecialButton(
                "closeButtonForSongs",
                closeButtonHandWritingCoords,
                buttonImages.normal,
                buttonImages.mouseOver,
                buttonImages.mouseDown,
                CloseButtonForSongs_Click
            );
        }

        private void CloseButtonForSongs_Click(object sender, EventArgs e)
        {
            
            this.SuspendLayout();

            SetHandWritingForSongsAndButtonsVisibility(false);

            
            this.ResumeLayout();
        }
    }
}