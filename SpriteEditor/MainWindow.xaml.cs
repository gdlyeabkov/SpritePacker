using MaterialDesignThemes.Wpf;
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
using System.Speech.Synthesis;
using System.Diagnostics;

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
        public Brush enabledHeaderBtnColor = System.Windows.Media.Brushes.Blue;
        public Brush disabledHeaderBtnColor = System.Windows.Media.Brushes.Gray;
        public SpeechSynthesizer debugger;
        public bool isDrawSpriteSheet = false;
        public OpenFileDialog spriteSheetDialog;
        public float spriteXCoord = 0;
        public float spriteYCoord = 0;
        public List<float> spriteHeights;
        public List<string> workFiles;
        public string tutorialLink = "https://www.codeandweb.com/texturepacker/documentation";
        public string addSpriteSound = @"C:\wpf_projects\SpriteEditor\SpriteEditor\Assets\add_new_sprite.wav";
        public string selectSpriteSound = @"C:\wpf_projects\SpriteEditor\SpriteEditor\Assets\select_sprite.wav";

        public MainWindow()
        {
            InitializeComponent();

            spriteHeights = new List<float>();
            workFiles = new List<string>();
            debugger = new SpeechSynthesizer();
            spriteBorderColor = System.Windows.Media.Brushes.Green;
            spriteLabelBackgroundColor = System.Windows.Media.Brushes.LightSkyBlue;
            emptyColor = System.Windows.Media.Brushes.Transparent;

        }

        private void CreateSpriteSheetHandler(object sender, RoutedEventArgs e)
        {
            CreateSpriteSheet();
        }

        private void SaveSpriteSheetHandler(object sender, RoutedEventArgs e)
        {
            SaveSpriteSheet();
        }

        public void DrawSpriteSheet()
        {
            spriteHeights.Clear();
            spriteXCoord = 0;
            spriteYCoord = 0;
            foreach (string fileName in workFiles)
            {
                CreateSprite(fileName);
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
            e.Handled = true;
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

        public void DrawSpritesLabels()
        {
            foreach (string fileName in workFiles)
            {
                CreateSpriteLabel(fileName);
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
            PlaySound(selectSpriteSound);
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
            SelectFullSpriteByName(spriteName);
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

        public void ToggleProject(bool toggler)
        {
            saveSpriteSheetLabel.IsEnabled = toggler;
            removeSpriteLabel.IsEnabled = toggler;
            addSpriteLabel.IsEnabled = toggler;
            toggleSpriteSheetSettingsBtn.IsEnabled = toggler;
            saveSpriteSheetBtn.IsEnabled = toggler;
            closeProjectBtn.IsEnabled = toggler;
            spriteSettingsBtn.IsEnabled = toggler;
            addSpriteBtn.IsEnabled = toggler;
            removeSpriteBtn.IsEnabled = toggler;
            PackIcon removeSpriteLabelIcon = ((PackIcon)(removeSpriteLabel.Children[0]));
            PackIcon addSpriteLabelIcon = ((PackIcon)(addSpriteLabel.Children[0]));
            PackIcon toggleSpriteSheetSettingsLabelIcon = ((PackIcon)(toggleSpriteSheetSettingsBtn.Children[0]));
            PackIcon saveSpriteSheetLabelIcon = ((PackIcon)(saveSpriteSheetBtn.Children[0]));
            if (toggler)
            {
                openedSprites.Visibility = Visibility.Visible;
                spriteSheetSettings.Visibility = Visibility.Visible;
                removeSpriteLabelIcon.Foreground = enabledHeaderBtnColor;
                addSpriteLabelIcon.Foreground = enabledHeaderBtnColor;
                toggleSpriteSheetSettingsLabelIcon.Foreground = enabledHeaderBtnColor;
                saveSpriteSheetLabelIcon.Foreground = enabledHeaderBtnColor;
            }
            else
            {
                openedSprites.Visibility = Visibility.Collapsed;
                spriteSheetSettings.Visibility = Visibility.Collapsed;
                removeSpriteLabelIcon.Foreground = disabledHeaderBtnColor;
                addSpriteLabelIcon.Foreground = disabledHeaderBtnColor;
                toggleSpriteSheetSettingsLabelIcon.Foreground = disabledHeaderBtnColor;
                saveSpriteSheetLabelIcon.Foreground = disabledHeaderBtnColor;
                ResetSettings();
            }
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
            ClearSpritesLabels();
            DrawSpritesLabels();
            selectedSpriteName = "";
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
            else
            {
                selectedTextureFile = "";
                textureFileName.Text = "";
            }
        }

        private void ClearTextureFileHandler(object sender, MouseButtonEventArgs e)
        {
            selectedTextureFile = "";
            textureFileName.Text = "";
        }

        public void ToggleSpriteSettings ()
        {
            Visibility settingsVisibility = spriteSheetSettings.Visibility;
            Visibility isVisible = Visibility.Visible;
            Visibility isHidden = Visibility.Collapsed;
            bool isSettingsVisible = settingsVisibility == isVisible;
            bool isCanToggleSettings = saveSpriteSheetLabel.IsEnabled;
            if (isCanToggleSettings)
            {
                if (isSettingsVisible)
                {
                    spriteSheetSettings.Visibility = isHidden;
                }
                else
                {
                    spriteSheetSettings.Visibility = isVisible;
                }
            }
        }

        private void ToggleSpriteSettingsLabelHandler(object sender, MouseButtonEventArgs e)
        {
            ToggleSpriteSettings();
        }

        private void CloseProjectHandler(object sender, RoutedEventArgs e)
        {
            ToggleProject(false);
        }

        public void ResetSettings()
        {
            ClearSpriteSheet();
            ClearSpritesLabels();
            selectedPixelFormat = System.Windows.Media.PixelFormats.Default;
            selectedSpriteName = "";
            selectedTextureFile = "";
            selectedTextureFormat = "png";
            opacityLevel.Value = 100;
            zoomer.Value = 100;
            spriteHeights.Clear();
            spriteXCoord = 0;
            spriteYCoord = 0;
        }

        public void CreateSpriteSheet()
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
                    workFiles = new List<string>(ofd.FileNames);
                    bool isProjectOpened = saveSpriteSheetLabel.IsEnabled;
                    if (isProjectOpened)
                    {
                        DrawSpriteSheet();
                    }
                    bool isHaveSprites = myStreams.Length >= 1;
                    ToggleProject(isHaveSprites);
                    ClearSpritesLabels();
                    DrawSpritesLabels();
                    spriteSheetDialog = ofd;
                    isDrawSpriteSheet = true;
                }

            }
        }

        private void CreateSpriteSheetLabelHandler (object sender, MouseButtonEventArgs e)
        {
            CreateSpriteSheet();
        }

        public void AddSprite()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool? res = ofd.ShowDialog();
            if (res != false)
            {
                Stream myStream;
                if ((myStream = ofd.OpenFile()) != null)
                {
                    string fileName = ofd.FileName;
                    CreateSpriteLabel(fileName);
                    CreateSprite(fileName);
                    workFiles.Add(fileName);
                }
            }
        }
        
        public void CreateSpriteLabel(string fileName)
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

        public void CreateSprite (string fileName)
        {
            Rectangle sprite = new Rectangle();
            sprite.DataContext = fileName.ToString();
            sprite.MouseUp += SelectSpriteHandler;
            System.Drawing.Image uploadedImage = System.Drawing.Image.FromFile(fileName);
            float spriteWidth = uploadedImage.Width;
            float spriteHeight = uploadedImage.Height;
            spriteHeights.Add(spriteHeight);
            bool isWrapSprites = rawCanvas.ActualWidth < spriteXCoord + spriteWidth;
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
            PlaySound(addSpriteSound);
        }

        private void AddSpriteLabelHandler(object sender, MouseButtonEventArgs e)
        {
            AddSprite();
        }

        private void CanvasStretchHandler(object sender, SizeChangedEventArgs e)
        {
            // плохой способ рабочей отрисовки спрайтшита
            if (isDrawSpriteSheet)
            {
                DrawSpriteSheet();
                isDrawSpriteSheet = false;
            }
        }

        private void SetMaxWidthHandler(object sender, SelectionChangedEventArgs e)
        {
            if (isZoomPermission)
            {
                ComboBox comboBox = ((ComboBox)(sender));
                int maxWidthIndex = comboBox.SelectedIndex;
                ComboBoxItem selectedItem = ((ComboBoxItem)(comboBox.Items[maxWidthIndex]));
                int selectedMaxWidth = Int32.Parse(selectedItem.DataContext.ToString());
                canvas.MaxWidth = selectedMaxWidth;
            }
        }

        private void SetMaxHeightHandler(object sender, SelectionChangedEventArgs e)
        {
            if (isZoomPermission)
            {
                ComboBox comboBox = ((ComboBox)(sender));
                int maxHeightIndex = comboBox.SelectedIndex;
                ComboBoxItem selectedItem = ((ComboBoxItem)(comboBox.Items[maxHeightIndex]));
                int selectedMaxHeight = Int32.Parse(selectedItem.DataContext.ToString());
                canvas.MaxHeight = selectedMaxHeight;
            }
        }

        private void GetTutorialHandler(object sender, RoutedEventArgs e)
        {
            GetTutorial();
        }

        public void GetTutorial ()
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = tutorialLink,
                UseShellExecute = true
            });
        }

        private void GetTutorialLabelHandler(object sender, MouseButtonEventArgs e)
        {
            GetTutorial();
        }

        private void SetSizeConstraintsHandler(object sender, SelectionChangedEventArgs e)
        {
            if (isZoomPermission)
            {
                ComboBox comboBox = ((ComboBox)(sender));
                int constraintSizeIndex = comboBox.SelectedIndex;
                ComboBoxItem selectedItem = ((ComboBoxItem)(comboBox.Items[constraintSizeIndex]));
                string[] selectedConstrainedSize = selectedItem.DataContext.ToString().Split(new char[] { 'X' });
                int constraintWidth = Int32.Parse(selectedConstrainedSize[0]);
                int constraintHeight = Int32.Parse(selectedConstrainedSize[1]);
                bool isNotAnySize = constraintWidth >= 1;
                if (isNotAnySize)
                {
                    canvas.Width = constraintWidth;
                    canvas.Height = constraintHeight;
                }
            }
        }

        private void SetFixedWidthHandler(object sender, SelectionChangedEventArgs e)
        {
            if (isZoomPermission)
            {
                ComboBox comboBox = ((ComboBox)(sender));
                int fixedWidthIndex = comboBox.SelectedIndex;
                ComboBoxItem selectedItem = ((ComboBoxItem)(comboBox.Items[fixedWidthIndex]));
                int selectedFixedWidth = Int32.Parse(selectedItem.DataContext.ToString());
                canvas.Width = selectedFixedWidth;
            }
        }

        private void SetFixedHeightHandler(object sender, SelectionChangedEventArgs e)
        {
            if (isZoomPermission)
            {
                ComboBox comboBox = ((ComboBox)(sender));
                int fixedHeightIndex = comboBox.SelectedIndex;
                ComboBoxItem selectedItem = ((ComboBoxItem)(comboBox.Items[fixedHeightIndex]));
                int selectedFixedHeight = Int32.Parse(selectedItem.DataContext.ToString());
                canvas.Height = selectedFixedHeight;
            }
        }

        private void ToggleSpriteSettingsHandler(object sender, RoutedEventArgs e)
        {
            ToggleSpriteSettings();
        }

        private void AddSpriteHandler(object sender, RoutedEventArgs e)
        {
            AddSprite();
        }

        public void PlaySound (string soundPath)
        {
            mainSoundPlayer.Source = new Uri(soundPath);
            mainSoundPlayer.Play();
        }

        private void SaveSpriteSheetLabelHandler(object sender, MouseButtonEventArgs e)
        {
            SaveSpriteSheet();
        }

        public void SaveSpriteSheet()
        {
            TransparentProcessing();
            ClearSpritesSelection();
            ClearSpritesLabelsSelection();
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
            SelectFullSpriteByName(selectedSpriteName);
        }

        public void SelectFullSpriteByName(string spriteName)
        {
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

    }
}
