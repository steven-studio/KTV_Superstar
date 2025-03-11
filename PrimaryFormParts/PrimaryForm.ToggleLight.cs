using System;
using System.IO;
using System.Windows.Forms; 
using System.Drawing; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private Button btnTurnOn;
        private Button btnTurnOff;
        private Button btnBright;
        private Button btnRomantic;
        private Button btnAuto;
        private Button btnColorTuning;
        private Button btnSoft;
        private Button btnDynamic;
        private Button btnDeskLamp;
        private Button btnStageLight;
        private Button btnShelfLight;
        private Button btnWallLight;
        private Button btnBrightnessUp1;
        private Button btnBrightnessDown1;
        private Button btnBrightnessUp2;
        private Button btnBrightnessDown2;

        private PictureBox pictureBoxToggleLight;

        private void InitializeButtonsForPictureBoxToggleLight()
        {
            btnTurnOn = new Button{ Text = "" };
            
            
            ConfigureButton(btnTurnOn, 604, 410, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnTurnOn.Click += (sender, e) =>
            {
                
                if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                {
                    
                    byte[] commandBytes = new byte[] { 0xA2, 0xDB, 0xA4 };
                    
                    
                    SerialPortManager.mySerialPort.Write(commandBytes, 0, commandBytes.Length);
                }
                else
                {
                    MessageBox.Show("Serial port is not open. Cannot send track correction command.");
                }
            };
            btnTurnOff = new Button{ Text = "" };
            
            
            ConfigureButton(btnTurnOff, 753, 411, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnTurnOff.Click += (sender, e) =>
            {
                SendCommandThroughSerialPort("a2 dc a4"); 
            };
            
            btnBright = new Button{ Text = "" };
            
            
            ConfigureButton(btnBright, 901, 411, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnBright.Click += (sender, e) => 
            {
                
                if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                {
                    
                    byte[] commandBytes = new byte[] { 0xA2, 0xD5, 0xA4 };
                    
                    
                    SerialPortManager.mySerialPort.Write(commandBytes, 0, commandBytes.Length);
                }
                else
                {
                    MessageBox.Show("Serial port is not open. Cannot send track correction command.");
                }
            };
            
            btnRomantic = new Button{ Text = "" };
            
            
            ConfigureButton(btnRomantic, 1049, 411, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 d7 a4"));
            
            btnAuto = new Button{ Text = "" };
            
            
            ConfigureButton(btnAuto, 1049, 494, 123, 63, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnColorTuning = new Button{ Text = "" };
            
            
            ConfigureButton(btnColorTuning, 1049, 579, 123, 63, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 75 a4"));
            
            btnSoft = new Button{ Text = "" };
            
            
            ConfigureButton(btnSoft, 901, 495, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 d6 a4"));
            
            btnDynamic = new Button{ Text = "" };
            
            
            ConfigureButton(btnDynamic, 901, 579, 123, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 d8 a4"));
            
            btnDeskLamp = new Button{ Text = "" };
            
            
            ConfigureButton(btnDeskLamp, 1048, 662, 124, 64, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 fb a4"));
            
            btnStageLight = new Button{ Text = "" };
            
            
            ConfigureButton(btnStageLight, 900, 662, 124, 64, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 fa a4"));
            
            btnShelfLight = new Button{ Text = "" };
            
            
            ConfigureButton(btnShelfLight, 752, 662, 124, 64, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 f9 a4"));
            
            btnWallLight = new Button{ Text = "" };
            
            
            ConfigureButton(btnWallLight, 604, 662, 124, 64, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                (sender, e) => SendCommandThroughSerialPort("a2 f8 a4"));
            
            btnBrightnessUp1 = new Button{ Text = "" };
            
            
            ConfigureButton(btnBrightnessUp1, 603, 495, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            
            btnBrightnessUp1.MouseDown += (sender, e) => 
            {
                lightControlTimer.Tag = "a2 d9 a4"; 
                lightControlTimer.Start(); 
            };

            
            btnBrightnessUp1.MouseUp += (sender, e) => 
            {
                lightControlTimer.Stop(); 
            };
            btnBrightnessDown1 = new Button{ Text = "" };
            
            
            ConfigureButton(btnBrightnessDown1, 605, 579, 122, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnBrightnessDown1.MouseDown += (sender, e) => { lightControlTimer.Tag = "a2 da a4"; lightControlTimer.Start(); };
            btnBrightnessDown1.MouseUp += (sender, e) => { lightControlTimer.Stop(); };
            btnBrightnessUp2 = new Button{ Text = "" };
            
            
            ConfigureButton(btnBrightnessUp2, 753, 495, 123, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnBrightnessUp2.MouseDown += (sender, e) => { lightControlTimer.Tag = "a2 f6 a4"; lightControlTimer.Start(); };
            btnBrightnessUp2.MouseUp += (sender, e) => { lightControlTimer.Stop(); };
            btnBrightnessDown2 = new Button{ Text = "" };
            
            
            ConfigureButton(btnBrightnessDown2, 753, 579, 123, 62, 
                resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, resizedNormalStateImageForLightControl, 
                null);
            btnBrightnessDown2.MouseDown += (sender, e) => { lightControlTimer.Tag = "a2 f7 a4"; lightControlTimer.Start(); };
            btnBrightnessDown2.MouseUp += (sender, e) => { lightControlTimer.Stop(); };

            
            this.Controls.Add(btnTurnOn);
            this.Controls.Add(btnTurnOff);
            this.Controls.Add(btnBright);
            this.Controls.Add(btnRomantic);
            this.Controls.Add(btnAuto);
            this.Controls.Add(btnColorTuning);
            this.Controls.Add(btnSoft);
            this.Controls.Add(btnDynamic);
            this.Controls.Add(btnDeskLamp);
            this.Controls.Add(btnStageLight);
            this.Controls.Add(btnShelfLight);
            this.Controls.Add(btnWallLight);
            this.Controls.Add(btnBrightnessUp1);
            this.Controls.Add(btnBrightnessDown1);
            this.Controls.Add(btnBrightnessUp2);
            this.Controls.Add(btnBrightnessDown2);
        }

        
        private void ToggleLightButton_Click(object sender, EventArgs e)
        {
            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);

            
            if (!pictureBoxToggleLight.Visible)
            {
                
                ShowImageOnPictureBoxToggleLight(Path.Combine(Application.StartupPath, @"themes\superstar\選單內介面_燈光控制.jpg"));
                SetPictureBoxToggleLightAndButtonsVisibility(true);
            }
            else
            {
                
                TogglePictureBoxToggleLightButtonsVisibility();
            }
            
            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }

        private void ShowImageOnPictureBoxToggleLight(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    MessageBox.Show($"找不到圖片文件: {imagePath}");
                    return;
                }

                using (var stream = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    var originalImage = new Bitmap(stream);
                    Rectangle cropArea = new Rectangle(570, 359, 630, 379);
                    
                    using (var croppedImage = new Bitmap(cropArea.Width, cropArea.Height))
                    {
                        using (var g = Graphics.FromImage(croppedImage))
                        {
                            g.DrawImage(originalImage, 
                                new Rectangle(0, 0, cropArea.Width, cropArea.Height),
                                cropArea,
                                GraphicsUnit.Pixel);
                        }
                        
                        pictureBoxToggleLight.Image?.Dispose();
                        pictureBoxToggleLight.Image = new Bitmap(croppedImage);
                    }
                    
                    ResizeAndPositionPictureBox(pictureBoxToggleLight, cropArea.X, cropArea.Y, cropArea.Width, cropArea.Height);
                    pictureBoxToggleLight.Visible = true;
                    pictureBoxToggleLight.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入燈光控制圖片時發生錯誤: {ex.Message}\n路徑: {imagePath}");
            }
        }

        private void TogglePictureBoxToggleLightButtonsVisibility()
        {
            
            bool areButtonsVisible = pictureBoxToggleLight.Visible; 

            
            SetPictureBoxToggleLightAndButtonsVisibility(!areButtonsVisible);
        }

        private void SetPictureBoxToggleLightAndButtonsVisibility(bool isVisible)
        {
            
            pictureBoxToggleLight.Visible = isVisible;

            
            btnTurnOn.Visible = isVisible;
            btnTurnOff.Visible = isVisible;
            btnBright.Visible = isVisible;
            btnRomantic.Visible = isVisible;
            btnAuto.Visible = isVisible;
            btnColorTuning.Visible = isVisible;
            btnSoft.Visible = isVisible;
            btnDynamic.Visible = isVisible;
            btnDeskLamp.Visible = isVisible;
            btnStageLight.Visible = isVisible;
            btnShelfLight.Visible = isVisible;
            btnWallLight.Visible = isVisible;
            btnBrightnessUp1.Visible = isVisible;
            btnBrightnessDown1.Visible = isVisible;
            btnBrightnessUp2.Visible = isVisible;
            btnBrightnessDown2.Visible = isVisible;

            if (isVisible)
            {
                
                pictureBoxToggleLight.BringToFront();

                
                btnTurnOn.BringToFront();
                btnTurnOff.BringToFront();
                btnBright.BringToFront();
                btnRomantic.BringToFront();
                btnAuto.BringToFront();
                btnColorTuning.BringToFront();
                btnSoft.BringToFront();
                btnDynamic.BringToFront();
                btnDeskLamp.BringToFront();
                btnStageLight.BringToFront();
                btnShelfLight.BringToFront();
                btnWallLight.BringToFront();
                btnBrightnessUp1.BringToFront();
                btnBrightnessDown1.BringToFront();
                btnBrightnessUp2.BringToFront();
                btnBrightnessDown2.BringToFront();
            }
        }
    }
}