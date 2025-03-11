using System;
using System.Drawing;
using System.IO; 
using System.Windows.Forms;

namespace DualScreenDemo
{
    public partial class PrimaryForm : Form
    {
        private PictureBox pictureBoxQRCode;
        private Button closeQRCodeButton;

        private void OverlayQRCodeOnImage(string randomFolderPath)
        {
            try
            {
                
                string imagePath = Path.Combine(Application.StartupPath, "themes/superstar/cropped_qrcode.jpg");
                if (!File.Exists(imagePath))
                {
                    Console.WriteLine("Base image not found: " + imagePath);
                    return;
                }

                using (Image baseImage = Image.FromFile(imagePath))
                {
                    
                    string serverAddressFilePath = Path.Combine(Application.StartupPath, "txt", "ip.txt");
                    if (!File.Exists(serverAddressFilePath))
                    {
                        Console.WriteLine("Server address file not found: " + serverAddressFilePath);
                        return;
                    }

                    string serverAddress = File.ReadAllText(serverAddressFilePath).Trim();
                    
                    // 根据地址格式生成不同的URL
                    string qrContent = serverAddress.Contains(":") ?
                        String.Format("http://{0}/{1}/windows.html", serverAddress, randomFolderPath) :
                        String.Format("http://{0}:{1}/{2}/windows.html", serverAddress, 9090, randomFolderPath);
                    Console.WriteLine("QR Content: " + qrContent);

                    
                    string qrImagePath = Path.Combine(Application.StartupPath, "themes/superstar/_www", randomFolderPath, "qrcode.png");
                    if (!File.Exists(qrImagePath))
                    {
                        Console.WriteLine("QR code image not found: " + qrImagePath);
                        return;
                    }

                    
                    Image qrCodeImage = null;
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (var fs = new FileStream(qrImagePath, FileMode.Open, FileAccess.Read))
                            {
                                qrCodeImage = Image.FromStream(fs);
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error loading QR code image: " + ex.Message);
                            System.Threading.Thread.Sleep(100); 
                        }
                    }

                    if (qrCodeImage == null)
                    {
                        Console.WriteLine("Failed to load QR code image after multiple attempts.");
                        return;
                    }

                    using (qrCodeImage)
                    {
                        
                        using (Bitmap bitmap = new Bitmap(baseImage.Width, baseImage.Height))
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                
                                g.DrawImage(baseImage, 0, 0);

                                
                                
                                Rectangle qrCodeRect = new Rectangle(32, 39, 165, 165);
                                
                                g.DrawImage(qrCodeImage, qrCodeRect);
                            }

                            
                            pictureBoxQRCode.Image = new Bitmap(bitmap);
                        }
                    }
                }

                
                ResizeAndPositionControl(pictureBoxQRCode, 975, 442, 226, 274);

                
                Bitmap originalImage = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\cropped_qrcode.jpg"));

                
                Rectangle closeQRCodeCropArea = new Rectangle(198, 6, 22, 22);

                
                Bitmap closeQRCodeCroppedImage = new Bitmap(closeQRCodeCropArea.Width, closeQRCodeCropArea.Height);
                using (Graphics g = Graphics.FromImage(closeQRCodeCroppedImage))
                {
                    g.DrawImage(originalImage, new Rectangle(0, 0, closeQRCodeCropArea.Width, closeQRCodeCropArea.Height), closeQRCodeCropArea, GraphicsUnit.Pixel);
                }

                
                closeQRCodeButton = new Button { Text = "" };
                closeQRCodeButton.Name = "closeQRCodeButton";
                ResizeAndPositionButton(closeQRCodeButton, 1173, 448, 22, 22);
                closeQRCodeButton.BackgroundImage = closeQRCodeCroppedImage;
                closeQRCodeButton.BackgroundImageLayout = ImageLayout.Stretch;
                closeQRCodeButton.FlatStyle = FlatStyle.Flat;
                closeQRCodeButton.FlatAppearance.BorderSize = 0; 
                closeQRCodeButton.Click += CloseQRCodeButton_Click;
                this.Controls.Add(closeQRCodeButton);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in OverlayQRCodeOnImage: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
            }
        }

        private void CloseQRCodeButton_Click(object sender, EventArgs e)
        {
            pictureBoxQRCode.Visible = false;
            closeQRCodeButton.Visible = false;
        }
    }
}