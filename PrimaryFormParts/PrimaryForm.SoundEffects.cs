using System;
using System.Drawing; 
using System.IO; 
using System.Windows.Forms; 
using NAudio.Wave; 
using WMPLib; 
using System.Collections.Generic; 

namespace DualScreenDemo
{
    public partial class PrimaryForm : Form
    {
        private WindowsMediaPlayer mediaPlayer;
        private IWavePlayer waveOut;
        private AudioFileReader audioFileReader;
        private PictureBox pictureBoxSceneSoundEffects;
        private Button constructionButton;
        private Button marketButton;
        private Button drivingButton;
        private Button airportButton;
        private Button officeButton;
        private Button closeButton;

        private void InitializeMediaPlayer()
        {
            mediaPlayer = new WindowsMediaPlayer();
        }

        private void InitializeSoundEffectButtons()
        {
            
            constructionButton = new Button
            {
                Name = "constructionButton",
            };
            ConfigureButton(constructionButton, 876, 494, 148, 64, 
                                        resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, 
                                        ConstructionButton_Click);
            this.Controls.Add(constructionButton);

            
            marketButton = new Button
            {
                Name = "marketButton",
            };
            ConfigureButton(marketButton, 1037, 495, 148, 63, 
                            resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, 
                            MarketButton_Click);
            this.Controls.Add(marketButton);

            
            drivingButton = new Button
            {
                Name = "drivingButton",
            };
            ConfigureButton(drivingButton, 876, 570, 148, 63, 
                            resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, 
                            DrivingButton_Click);
            this.Controls.Add(drivingButton);

            
            airportButton = new Button
            {
                Name = "airportButton",
            };
            ConfigureButton(airportButton, 1037, 570, 148, 63, 
                            resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, 
                            AirportButton_Click);
            this.Controls.Add(airportButton);

            
            officeButton = new Button
            {
                Name = "officeButton",
            };
            ConfigureButton(officeButton, 876, 646, 148, 64, 
                            resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, 
                            OfficeButton_Click);
            this.Controls.Add(officeButton);

            
            closeButton = new Button
            {
                Name = "closeButton",
            };
            
            ConfigureButton(closeButton, 1036, 646, 150, 63, 
                            resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, resizedNormalStateImageForSceneSoundEffects, 
                            CloseButton_Click);
            this.Controls.Add(closeButton);
        }

        private void SoundEffectButton_Click(object sender, EventArgs e)
        {
            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);

            if (!pictureBoxSceneSoundEffects.Visible)
            {
                ShowImageOnPictureBoxSceneSoundEffects(Path.Combine(Application.StartupPath, @"themes\superstar\555022.jpg"));
                SetPictureBoxSceneSoundEffectsAndButtonsVisibility(true);
            }
            else
            {
                TogglePictureBoxSceneSoundEffectsButtonsVisibility();
            }
        }
        
        private void ConstructionButton_Click(object sender, EventArgs e) => PlaySound(@"sounds\1857.mp3");
        private void MarketButton_Click(object sender, EventArgs e) => PlaySound(@"sounds\13472_Audio Trimmer.mp3");
        private void DrivingButton_Click(object sender, EventArgs e) => PlaySound(@"sounds\kc1.mp3");
        private void AirportButton_Click(object sender, EventArgs e) => PlayMediaSound(@"sounds\xm2401.m4a");
        private void OfficeButton_Click(object sender, EventArgs e) => PlayMediaSound(@"sounds\y1640.m4a");
        private void CloseButton_Click(object sender, EventArgs e) => TogglePictureBoxSceneSoundEffectsButtonsVisibility();

        private void PlaySound(string filePath)
        {
            waveOut?.Dispose();
            audioFileReader?.Dispose();

            waveOut = new WaveOutEvent();
            audioFileReader = new AudioFileReader(Path.Combine(Application.StartupPath, filePath));
            waveOut.Init(audioFileReader);
            waveOut.Play();
        }

        private void PlayMediaSound(string filePath)
        {
            mediaPlayer.URL = Path.Combine(Application.StartupPath, filePath);
            mediaPlayer.controls.play();
        }

        public void PlayApplauseSound()
        {
            mediaPlayer.URL = Path.Combine(Application.StartupPath, "zs.m4a");
            mediaPlayer.controls.play();
        }

        private void ShowImageOnPictureBoxSceneSoundEffects(string imagePath)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle cropArea = new Rectangle(859, 427, 342, 295);

            
            Bitmap croppedImage = CropImage(originalImage, cropArea);

            
            pictureBoxSceneSoundEffects.Image = croppedImage;
    
            
            ResizeAndPositionPictureBox(pictureBoxSceneSoundEffects, cropArea.X, cropArea.Y, cropArea.Width, cropArea.Height);
            
            pictureBoxSceneSoundEffects.Visible = true;
        }

        private void TogglePictureBoxSceneSoundEffectsButtonsVisibility()
        {
            
            bool areButtonsVisible = pictureBoxSceneSoundEffects.Visible; 

            
            SetPictureBoxSceneSoundEffectsAndButtonsVisibility(!areButtonsVisible);
        }

        
        private void SetPictureBoxSceneSoundEffectsAndButtonsVisibility(bool isVisible)
        {
            
            pictureBoxSceneSoundEffects.Visible = isVisible;

            if (isVisible)
            {
                
                pictureBoxSceneSoundEffects.BringToFront();
            }

            
            List<Button> soundEffectButtons = new List<Button>
            {
                constructionButton,
                marketButton,
                drivingButton,
                airportButton,
                officeButton,
                closeButton
            };

            
            foreach (Button button in soundEffectButtons)
            {
                button.Visible = isVisible;
                if (isVisible)
                {
                    button.BringToFront();
                }
            }
        }
    }
}