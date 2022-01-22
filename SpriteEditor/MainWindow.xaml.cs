using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpriteEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateSpriteSheetHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            bool? res = ofd.ShowDialog();
            if (res != false)
            {
                Stream[] myStreams;
                if ((myStreams = ofd.OpenFiles()) != null)
                {
                    DrawSpriteSheet();
                }

            }
        }

        private void SaveSpriteSheetHandler(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "Новый спрайтшит";
            sfd.DefaultExt = ".png";
            sfd.Filter = "PNG images (.png)|*.png";
            bool? res = sfd.ShowDialog();
            if (res != false)
            {

                using (Stream s = File.Open(sfd.FileName, FileMode.OpenOrCreate))
                {
                    pngEncoder.Save(s);
                }
            }
        }

        public void DrawSpriteSheet()
        {
            float spriteXCoord = 0;
            float spriteYCoord = 0;
            foreach (string fileName in ofd.FileNames)
            {
                Rectangle sprite = new Rectangle();
                System.Drawing.Image uploadedImage = System.Drawing.Image.FromFile(fileName);
                float spriteWidth = uploadedImage.Width;
                float spriteHeight = uploadedImage.Height;
                bool isWrapSprites = canvas.ActualWidth < spriteXCoord + spriteWidth;
                if (isWrapSprites)
                {
                    spriteYCoord += spriteHeight;
                    spriteXCoord = 0;
                }
                sprite.Width = spriteWidth;
                sprite.Height = spriteHeight;
                Canvas.SetLeft(sprite, spriteXCoord);
                Canvas.SetTop(sprite, spriteYCoord);
                ImageBrush spriteBrush = new ImageBrush();
                BitmapImage spriteSource = new BitmapImage();
                spriteSource.BeginInit();
                spriteSource.UriSource = new Uri(fileName, UriKind.Absolute);
                spriteSource.EndInit();
                spriteBrush.ImageSource = spriteSource;
                sprite.Fill = spriteBrush;
                canvas.Children.Add(sprite);
                spriteXCoord += spriteWidth;
            }
        }

    }
}
