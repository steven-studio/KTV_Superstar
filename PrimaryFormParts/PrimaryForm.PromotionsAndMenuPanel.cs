using System;
using System.Collections.Generic;
using System.Drawing; 
using System.IO;
using System.Windows.Forms;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private PromotionsAndMenuPanel promotionsAndMenuPanel;

        private void InitializePromotionsAndMenuPanel()
        {
            promotionsAndMenuPanel = new PromotionsAndMenuPanel();
            ResizeAndPositionControl(promotionsAndMenuPanel, 0, 0, 1440, 900);
            this.Controls.Add(promotionsAndMenuPanel);

            promotions = LoadPromotionsImages(); 
            menu = LoadMenuImages(); 

            
        }
        
        private List<Image> LoadMenuImages()
        {
            List<Image> images = new List<Image>();
            string foodsFolderPath = Path.Combine(Application.StartupPath, "foods");

            string[] imageFiles = Directory.GetFiles(foodsFolderPath, "*.jpg");

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
    }
}