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

        public Brush spriteBorderColor;
        public Brush spriteLabelBackgroundColor;
        public Brush emptyColor;

        public MainWindow()
        {
            InitializeComponent();

            spriteBorderColor = System.Windows.Media.Brushes.Green;
            spriteLabelBackgroundColor = System.Windows.Media.Brushes.LightSkyBlue;
            emptyColor = System.Windows.Media.Brushes.Transparent;

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
                    ClearSpriteSheet();
                    DrawSpriteSheet(ofd);
                    ClearSpritesLabels();
                    DrawSpritesLabels(ofd);
                    bool isHaveSprites = myStreams.Length >= 1;
                    ToggleSaveSpriteSheetBtn(isHaveSprites);
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

        public void DrawSpriteSheet(OpenFileDialog ofd)
        {
            float spriteXCoord = 0;
            float spriteYCoord = 0;
            List<float> spriteHeights = new List<float>();
            foreach (string fileName in ofd.FileNames)
            {
                Rectangle sprite = new Rectangle();
                sprite.DataContext = fileName.ToString();
                sprite.MouseUp += SelectSpriteHandler;
                System.Drawing.Image uploadedImage = System.Drawing.Image.FromFile(fileName);
                float spriteWidth = uploadedImage.Width;
                float spriteHeight = uploadedImage.Height;
                spriteHeights.Add(spriteHeight); 
                bool isWrapSprites = canvas.ActualWidth < spriteXCoord + spriteWidth;
                if (isWrapSprites)
                {
                    float maxSpriteHeight = spriteHeights.Max();
                    spriteYCoord += maxSpriteHeight;
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
                bool isOutlineEnabled = ((bool)(isDisplayOutline.IsChecked));
                Brush currentSpriteStroke = null;
                if (isOutlineEnabled)
                {
                    currentSpriteStroke = spriteBorderColor;
                }
                else
                {
                    currentSpriteStroke = emptyColor;
                }
                sprite.Stroke = currentSpriteStroke;
            }
        }

        public void ClearSpriteSheet()
        {
            canvas.Children.Clear();
        }

        public void SelectSpriteHandler(object sender, RoutedEventArgs e)
        {
            Rectangle sprite = ((Rectangle)(sender));
            string spriteName = sprite.DataContext.ToString();
            ClearSpritesSelection();
            SelectSprite(sprite);
            SelectSpriteLabel(spriteName);
        }

        public void ClearSpritesSelection()
        {
            UIElementCollection sprites = canvas.Children;
            foreach (Rectangle sprite in sprites)
            {
                ImageBrush spriteBrush = new ImageBrush();
                BitmapImage spriteSource = new BitmapImage();
                spriteSource.BeginInit();
                string fileName = sprite.DataContext.ToString();
                spriteSource.UriSource = new Uri(fileName, UriKind.Absolute);
                spriteSource.EndInit();
                spriteBrush.ImageSource = spriteSource;
                sprite.Fill = spriteBrush;
            }
        }

        public void DrawSpritesLabels(OpenFileDialog ofd)
        {
            foreach (string fileName in ofd.FileNames)
            {
                StackPanel spriteLabel = new StackPanel();
                spriteLabel.Orientation = Orientation.Horizontal;
                spriteLabel.DataContext = fileName;
                spriteLabel.MouseUp += SelectSpriteLabelHandler;
                Image spriteLabelIcon = new Image();
                BitmapImage spriteSource = new BitmapImage();
                spriteSource.BeginInit();
                spriteSource.UriSource = new Uri(fileName, UriKind.Absolute);
                spriteSource.EndInit();
                spriteLabelIcon.Source = spriteSource;
                spriteLabelIcon.Width = 20;
                spriteLabelIcon.Height = 20;
                spriteLabelIcon.Margin = new Thickness(10);
                spriteLabel.Children.Add(spriteLabelIcon);
                TextBlock spriteLabelName = new TextBlock();
                string onlyFileName = GetTerminatedName(fileName);
                spriteLabelName.Text = onlyFileName;
                spriteLabelName.Margin = new Thickness(10);
                spriteLabel.Children.Add(spriteLabelName);
                spritesLabels.Children.Add(spriteLabel);
            }
        }

        public void ClearSpritesLabels()
        {
            spritesLabels.Children.Clear();
        }

        public void SelectSprite(Rectangle sprite)
        {
            VisualBrush newSpriteBrush = new VisualBrush();
            Grid newSpriteBrushContent = new Grid();
            ImageBrush newSpriteBrushContentInitial = ((ImageBrush)(sprite.Fill));
            Image rawNewSpriteBrushContentInitial = new Image();
            BitmapImage spriteSource = new BitmapImage();
            spriteSource.BeginInit();
            spriteSource.UriSource = new Uri(newSpriteBrushContentInitial.ImageSource.ToString(), UriKind.Absolute);
            spriteSource.EndInit();
            rawNewSpriteBrushContentInitial.Source = spriteSource;
            newSpriteBrushContent.Children.Add(rawNewSpriteBrushContentInitial);
            Rectangle newSpriteBrushContentSelected = new Rectangle();
            SolidColorBrush newSpriteBrushContentSelectedFill = new SolidColorBrush();
            Color newSpriteBrushContentSelectedFillColor = new Color();
            newSpriteBrushContentSelectedFillColor.R = 0;
            newSpriteBrushContentSelectedFillColor.G = 0;
            newSpriteBrushContentSelectedFillColor.B = 255;
            newSpriteBrushContentSelectedFillColor.A = 100;
            newSpriteBrushContentSelectedFill.Color = newSpriteBrushContentSelectedFillColor;
            newSpriteBrushContentSelected.Fill = newSpriteBrushContentSelectedFill;
            newSpriteBrushContent.Children.Add(newSpriteBrushContentSelected);
            newSpriteBrush.Visual = newSpriteBrushContent;
            sprite.Fill = newSpriteBrush;
        }

        public void SelectSpriteLabel(string spriteName)
        {
            string onlySpriteName = GetTerminatedName(spriteName);
            foreach (StackPanel spriteLabel in spritesLabels.Children)
            {
                TextBlock rawSpriteLabelName = ((TextBlock)(spriteLabel.Children[1]));
                string spriteLabelName = rawSpriteLabelName.Text;
                bool isSpriteLabelFound = spriteLabelName == onlySpriteName;
                if (isSpriteLabelFound)
                {
                    ClearSpritesLabelsSelection();
                    spriteLabel.Background = spriteLabelBackgroundColor;
                }
            }
        }

        public void ClearSpritesLabelsSelection()
        {
            UIElementCollection labels = spritesLabels.Children;
            foreach (StackPanel spriteLabel in labels)
            {
                spriteLabel.Background = emptyColor;
            }
        }

        public void SelectSpriteLabelHandler (object sender, RoutedEventArgs e)
        {
            StackPanel sprite = ((StackPanel)(sender));
            string spriteName = sprite.DataContext.ToString();
            ClearSpritesSelection();
            Rectangle foundedSprite = null;
            bool isSpriteFound = false;
            foreach (Rectangle currentSprite in canvas.Children)
            {
                string currentSpriteName = currentSprite.DataContext.ToString();
                bool isSpritesNamesMatch = currentSpriteName == spriteName;
                if (isSpritesNamesMatch)
                {
                    isSpriteFound = true;
                    foundedSprite = currentSprite;
                }
            }
            if (isSpriteFound)
            {
                SelectSprite(foundedSprite);
                SelectSpriteLabel(spriteName);
            }
        }

        public string GetTerminatedName (string fileName)
        {
            string terminatedName = "";
            string[] separatablePart = fileName.Split(new char[] { '\\', '/' });
            int separatablePartLength = separatablePart.Length - 1;
            terminatedName = separatablePart[separatablePartLength];
            return terminatedName;
        }

        private void ToggleSpriteOutlineHandler(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = ((CheckBox)(sender));
            bool isDisplayOutline = ((bool)(checkBox.IsChecked));
            UIElementCollection sprites = canvas.Children;
            bool isSpritesInserted = sprites.Count >= 1;
            if (isSpritesInserted)
            {
                Rectangle firstSprite = ((Rectangle)(sprites[0]));
                Brush spriteOutline = firstSprite.Stroke;
                bool isOutlineEnabled = isDisplayOutline;
                Brush currentSpriteStroke = null;
                if (isOutlineEnabled)
                {
                    currentSpriteStroke = spriteBorderColor;
                }
                else
                {
                    currentSpriteStroke = emptyColor;
                }
                foreach (Rectangle sprite in sprites)
                {
                    sprite.Stroke = currentSpriteStroke;
                }
            }
        }

        public void ToggleSaveSpriteSheetBtn (bool toggler)
        {
            saveSpriteSheetBtn.IsEnabled = toggler;
        }

    }
}
