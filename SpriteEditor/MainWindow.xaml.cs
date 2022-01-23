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
        public string selectedSpriteName = "";
        public double canvasInitialWidth;
        public double canvasInitialHeight;
        public bool isZoomPermission = false;
        public string selectedTextureFormat = "png";
        public string selectedTransparentPixelsProcessing = "remove";
        public Brush initialCanvasBackground;
        public System.Windows.Media.PixelFormat selectedPixelFormat = System.Windows.Media.PixelFormats.Default; 
        public string selectedTextureFile = "";
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
            TransparentProcessing();
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width, (int)canvas.RenderSize.Height, 96d, 96d, selectedPixelFormat);
            rtb.Render(canvas);
            var crop = new CroppedBitmap(rtb, new Int32Rect(50, 50, 250, 250));
            BitmapEncoder imageEncoder = null;
            bool isPngTextureFormat = selectedTextureFormat == "png";
            bool isBmpTextureFormat = selectedTextureFormat == "bmp";
            bool isGifTextureFormat = selectedTextureFormat == "gif";
            bool isJpegTextureFormat = selectedTextureFormat == "jpeg";
            bool isTiffTextureFormat = selectedTextureFormat == "tiff";
            string defaultExt = "";
            string filterFiles = "";
            if (isPngTextureFormat)
            {
                imageEncoder = new PngBitmapEncoder();
                defaultExt = ".png";
                filterFiles = "PNG images (.png)|*.png";
            }
            else if (isBmpTextureFormat)
            {
                imageEncoder = new BmpBitmapEncoder();
                defaultExt = ".bmp";
                filterFiles = "BMP images (.bmp)|*.bmp";
            }
            else if (isGifTextureFormat)
            {
                imageEncoder = new GifBitmapEncoder();
                defaultExt = ".gif";
                filterFiles = "GIF images (.gif)|*.gif";
            }
            else if (isJpegTextureFormat)
            {
                imageEncoder = new JpegBitmapEncoder();
                defaultExt = ".jpeg";
                filterFiles = "JPEG images (.png)|*.jpeg";
            }
            else if (isTiffTextureFormat)
            {
                imageEncoder = new TiffBitmapEncoder();
                defaultExt = ".tiff";
                filterFiles = "TIFF images (.tiff)|*.tiff";
            }
            imageEncoder.Frames.Add(BitmapFrame.Create(rtb));
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "Новый спрайтшит";
            sfd.DefaultExt = defaultExt;
            sfd.Filter = filterFiles;
            bool isHideDialog = selectedTextureFile.Length >= 1;
            bool? res = false;
            if (isHideDialog)
            {
                res = true;
                sfd.FileName = selectedTextureFile;
            }
            else
            {
                res = sfd.ShowDialog();
            }            
            if (res != false)
            {
                using (Stream saveStream = File.Open(sfd.FileName, FileMode.OpenOrCreate))
                {
                    imageEncoder.Save(saveStream);
                }
            }
            Thread.Sleep(1000);
            ClearTransparentProcessing();
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
                ContextMenu spriteContextMenu = new ContextMenu();
                MenuItem spriteContextMenuItem = new MenuItem();
                spriteContextMenuItem.Header = "Удалить спрайт";
                spriteContextMenuItem.DataContext = fileName;
                spriteContextMenuItem.Click += RemoveSpriteHandler;
                spriteContextMenu.Items.Add(spriteContextMenuItem);
                sprite.ContextMenu = spriteContextMenu;
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
            SetCurrentSprite(spriteName);
            ClearSpritesSelection();
            SelectSprite(sprite);
            SelectSpriteLabel(spriteName);
        }

        public void SetCurrentSprite (string spriteName)
        {
            selectedSpriteName = spriteName;
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
                ContextMenu spriteLabelContextMenu = new ContextMenu();
                MenuItem spriteLabelContextMenuItem = new MenuItem();
                spriteLabelContextMenuItem.Header = "Удалить спрайт";
                spriteLabelContextMenuItem.DataContext = fileName;
                spriteLabelContextMenuItem.Click += RemoveSpriteHandler;
                spriteLabelContextMenu.Items.Add(spriteLabelContextMenuItem);
                spriteLabel.ContextMenu = spriteLabelContextMenu;
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
            object rawSpriteName = sprite.DataContext;
            string spriteName = rawSpriteName.ToString();
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
                SetCurrentSprite(spriteName);
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

        public void RemoveSpriteHandler(object sender, RoutedEventArgs e)
        {
            MenuItem removeSpriteBtn = ((MenuItem)(sender));
            object rawSpriteName = removeSpriteBtn.DataContext;
            string spriteName = rawSpriteName.ToString();
            RemoveSprite(spriteName);
        }

        public void RemoveSprite(string spriteName)
        {
            RemoveSpriteLabel(spriteName);
            RemoveSpriteFromSpriteSheet(spriteName);
            selectedSpriteName = "";
        }

        public void RemoveSpriteFromSpriteSheet(string spriteName)
        {
            foreach (Rectangle sprite in canvas.Children)
            {
                object spriteData = sprite.DataContext;
                string currentSpriteName = spriteData.ToString();
                bool isSpritesNamesMatch = currentSpriteName == spriteName;
                if (isSpritesNamesMatch)
                {
                    canvas.Children.Remove(sprite);
                    break;
                }
            }
        }

        public void RemoveSpriteLabel(string spriteName)
        {
            foreach (StackPanel spriteLabel in spritesLabels.Children)
            {
                object spriteLabelData = spriteLabel.DataContext;
                string spriteLabelName = spriteLabelData.ToString();
                bool isSpritesNamesMatch = spriteLabelName == spriteName;
                if (isSpritesNamesMatch)
                {
                    spritesLabels.Children.Remove(spriteLabel);
                    break;
                }
            }
        }

        private void ExpandRemoveSpriteHandler(object sender, MouseButtonEventArgs e)
        {
            bool isSpriteSelected = selectedSpriteName.Length >= 1;
            if (isSpriteSelected)
            {
                RemoveSprite(selectedSpriteName);
            }
        }

        private void SetZoomHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = ((Slider)(sender));
            if (isZoomPermission)
            {
                double zoomRatio = Math.Floor(slider.Value) / 100;
                zoom.Width = canvasInitialWidth * zoomRatio;
                zoom.Height = canvasInitialHeight * zoomRatio;
            }
        }

        private void CanvasInitializedHandler(object sender, RoutedEventArgs e)
        {
            canvasInitialWidth = rawCanvas.ActualWidth;
            canvasInitialHeight = rawCanvas.ActualHeight;
            canvas.Width = rawCanvas.ActualWidth;
            canvas.Height = rawCanvas.ActualHeight;
            isZoomPermission = true;
            initialCanvasBackground = canvas.Background;
        }

        private void InitializeZoomHandler(object sender, MouseButtonEventArgs e)
        {
            isZoomPermission = true;
        }

        private void ResetSelectionHandler(object sender, MouseButtonEventArgs e)
        {
            ClearSpritesSelection();
        }

        private void SetTextureFormatHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int textureFormatIndex = comboBox.SelectedIndex;
            bool isPngTextureFormat = textureFormatIndex == 0;
            bool isBmpTextureFormat = textureFormatIndex == 1;
            bool isGifTextureFormat = textureFormatIndex == 2;
            bool isJpegTextureFormat = textureFormatIndex == 3;
            bool isTiffTextureFormat = textureFormatIndex == 4;
            if (isPngTextureFormat)
            {
                selectedTextureFormat = "png";
            }
            else if (isBmpTextureFormat)
            {
                selectedTextureFormat = "bmp";
            }
            else if (isGifTextureFormat)
            {
                selectedTextureFormat = "gif";
            }
            else if (isJpegTextureFormat)
            {
                selectedTextureFormat = "jpeg";
            }
            else if (isTiffTextureFormat)
            {
                selectedTextureFormat = "tiff";
            }
        }

        private void SetImageOpacity(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isZoomPermission)
            {
                Slider slider = ((Slider)(sender));
                double opacityRatio = slider.Value / 100;
                canvas.Opacity = opacityRatio;
            }
        }

        private void SetTransparentProcessingHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int transparentPixelsProcessingIndex = comboBox.SelectedIndex;
            bool isRemoveTransparentPixels = transparentPixelsProcessingIndex == 0;
            bool isLeftTransparentPixels = transparentPixelsProcessingIndex == 1;
            if (isRemoveTransparentPixels)
            {
                selectedTransparentPixelsProcessing = "remove";
            }
            else if (isLeftTransparentPixels)
            {
                selectedTransparentPixelsProcessing = "left";
            }

        }

        public void TransparentProcessing ()
        {
            bool isRemoveTransparentPixels = selectedTransparentPixelsProcessing == "remove";
            bool isLeftTransparentPixels = selectedTransparentPixelsProcessing == "left";
            if (isRemoveTransparentPixels)
            {
                canvas.Background = emptyColor;
            }
            else if (isLeftTransparentPixels)
            {
                // делать ничего не нужно (оставляем пиксели такими какие отображаются на канвасе)
            }
        }
        public void ClearTransparentProcessing()
        {
            canvas.Background = initialCanvasBackground;
        }

        private void SetPixelFormatHandler(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = ((ComboBox)(sender));
            int pixelFormatIndex = comboBox.SelectedIndex;
            bool isRgbPixelFormat = pixelFormatIndex == 0;
            bool isCmykPixelFormat = pixelFormatIndex == 1;
            bool isBgrPixelFormat = pixelFormatIndex == 2;
            bool isGrayscalePixelFormat = pixelFormatIndex == 3;
            bool isBlackWhitePixelFormat = pixelFormatIndex == 4;
            bool isIndexedPixelFormat = pixelFormatIndex == 5;
            if (isRgbPixelFormat)
            {
                selectedPixelFormat = System.Windows.Media.PixelFormats.Default;
            }
            else if (isCmykPixelFormat)
            {
                selectedPixelFormat = System.Windows.Media.PixelFormats.Cmyk32;
            }
            else if (isBgrPixelFormat)
            {
                selectedPixelFormat = System.Windows.Media.PixelFormats.Bgr101010;
            }
            else if (isGrayscalePixelFormat)
            {
                selectedPixelFormat = System.Windows.Media.PixelFormats.Gray16;
            }
            else if (isBlackWhitePixelFormat)
            {
                selectedPixelFormat = System.Windows.Media.PixelFormats.BlackWhite;
            }
            else if (isIndexedPixelFormat)
            {
                selectedPixelFormat = System.Windows.Media.PixelFormats.Indexed1;
            }
        }

        private void SetTextureFileHandler(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? res = ofd.ShowDialog();
            if (res != false)
            {
                Stream myStream;
                if ((myStream = ofd.OpenFile()) != null)
                {
                    selectedTextureFile = ofd.FileName;
                    textureFileName.Text = GetTerminatedName(ofd.FileName);
                }
            }
        }

    }
}
