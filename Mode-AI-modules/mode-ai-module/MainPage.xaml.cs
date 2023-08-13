using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.Storage.Search;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Net;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System;
using Windows.UI.ViewManagement;
using Newtonsoft.Json;
using Windows.Networking.Connectivity;
using Windows.Data.Pdf;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Ideafixxxer.CsvParser;
using System.IO;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace doc_onlook
{

    
    public sealed partial class MainPage : Page
    {
        Workspace workspace;
        List<StorageFile> localFilesList;
        List<FileItem> fileItemList;
        List<string> _matchingNamesList;
        List<string> fileNameList;
        StreamSocketListener _listener;
        string IP_info;
        int _preSelectedIndex;
        int collapseThresholdWidth;
        bool userCollapse;

        public MainPage()
        {
            InitializeComponent();
            InitializeData();

            FillWorkspace();
            RunTCPListener();
        }

        public void FillWorkspace()
        {
            UpdateLocalList();
            workspace = new Workspace(WorkspacePivot);
            WorkspacePivot.Focus(FocusState.Pointer);   
        }

        private void InitializeIPInfo()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostNamesList = NetworkInformation.GetHostNames();
                string domainName = null;
                string v4Name = null;
                foreach (var entry in hostNamesList)
                {
                    if (entry.Type == Windows.Networking.HostNameType.DomainName && domainName == null)
                    {
                        domainName = entry.CanonicalName;
                    }
                    if (entry.Type == Windows.Networking.HostNameType.Ipv4 && v4Name == null)
                    {
                        v4Name = entry.CanonicalName;
                    }
                }
                if (domainName != null && v4Name != null)
                {
                    IPInfo.Text = domainName + "  |  " + v4Name;
                    IP_info = IPInfo.Text;
                }
                else
                {
                    IPInfo.Text = "Couldn't retrieve IP information.";
                }
            }
            else
            {
                IPInfo.Text = "Couldn't retrieve IP information";
            }
        }

        private void InitializeData()
        {
            localFilesList = new List<StorageFile>();
            fileItemList = new List<FileItem>();

            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("name", "hello");
            dict.Add("type", ".html");
            dict.Add("data", "<html><body>Hello doc-onlook</body></html>");
            WriteUniqueFileToLocal(dict);

            PivotItemHeaderExit.Completed += PivotItemHeaderExit_Completed;
            SideBar_Collapse.Completed += SideBar_Collapse_Completed;
            Window.Current.SizeChanged += Current_SizeChanged;

            userCollapse = false;
            collapseThresholdWidth = 825;

            fileNameList = new List<string>();
            _matchingNamesList = new List<string>();
            LoadSampleFiles();
            InitializeIPInfo();
        }

        private async void LoadSampleFiles()
        {
            StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            folder = await folder.GetFolderAsync("SampleFiles");
            StorageFile photo1 = await folder.GetFileAsync("Mountains.jpg");
            StorageFile photo2 = await folder.GetFileAsync("Lake.jpg");
            StorageFile csv1 = await folder.GetFileAsync("Books.csv");
            StorageFile pdf1 = await folder.GetFileAsync("Sample PDF.pdf");
            StorageFile txt1 = await folder.GetFileAsync("ArtOfWar.txt");
            await photo1.CopyAsync(localCacheFolder,photo1.DisplayName,NameCollisionOption.ReplaceExisting);
            await photo2.CopyAsync(localCacheFolder, photo2.DisplayName, NameCollisionOption.ReplaceExisting);
            await csv1.CopyAsync(localCacheFolder, csv1.DisplayName, NameCollisionOption.ReplaceExisting);
            await pdf1.CopyAsync(localCacheFolder, pdf1.DisplayName, NameCollisionOption.ReplaceExisting);
            await txt1.CopyAsync(localCacheFolder, txt1.DisplayName, NameCollisionOption.ReplaceExisting);
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            var windowHeight = Window.Current.Bounds.Height;

            if (e.Size.Width < collapseThresholdWidth)
            {
                Collapse.IsChecked = true;
            }
            else
            {

                SetPrimaryButtonsVisibility(Visibility.Visible);
                SetSecondaryButtonsVisibility(Visibility.Collapsed);
                Collapse.IsChecked = userCollapse;
            }

            WorkspacePivot.Height = windowHeight;

            PivotItem selectedPivotItem = (PivotItem)(WorkspacePivot.SelectedItem);

            if (selectedPivotItem != null)
            {
                selectedPivotItem.Height = windowHeight - FileCommandBar.ActualHeight - 50;
                Debug.WriteLine("PivotItem size changed." + ((Grid)(selectedPivotItem.Content)).Height);
            }
        }

        public class FileItem
        {
            public StorageFile file
            {
                get; set;
            }
            public string fileDateCreated
            {
                get; set;
            }
            public string fileDisplayName
            {
                get; set;
            }
            public string fileType
            {
                get; set;
            }
            public string fileSize
            {
                get; set;
            }
            public double fileSizeBytes
            {
                get; set;
            }
            public string fileIcon
            {
                get; set;
            }
            public DateTime fileDateCreated_DateTime
            {
                get; set;
            }

            public FileItem(StorageFile file)
            {
                this.file = file;
                fileDateCreated = file.DateCreated.LocalDateTime.ToString();
                fileDateCreated_DateTime = file.DateCreated.LocalDateTime;
                fileDisplayName = file.DisplayName;
                fileType = file.FileType;
                switch (fileType)
                {
                    case ".html": fileIcon = "Globe";
                        break;
                    case ".pdf":
                    case ".txt":
                        fileIcon = "Document";
                        break;
                    case ".jpg":
                    case ".png":
                        fileIcon = "Pictures";
                        break;
                    case ".csv":
                        fileIcon = "Calculator";
                        break;
                }
            }
            public static implicit operator FileItem(StorageFile s)
            {
                return new FileItem(s);
            }
            public static explicit operator StorageFile(FileItem f)
            {
                if (f == null)
                {
                    return null;
                }
                return f.file;
            }
        }

        class Workspace
        {
            private List<string> workspaceList { get; set; }
            private Pivot workspacePivot { get; set; }
            PdfDocument _pdfDocument;
            StackPanel _pdfStack;

            public void AddToList(StorageFile file, double commandBarHeight)
            {
                Debug.WriteLine("AddToList: "+file.DisplayName+file.FileType);
                PivotItem item = CreatePivotItem(file.DisplayName + file.FileType, commandBarHeight);
                workspacePivot.Items.Add(item);

                SetPivotItemContent(workspacePivot.Items.Count-1, file);
            }

            public int InList(string name)
            {
                for (var i=0; i<workspacePivot.Items.Count; i++)
                {
                    string s = GetPivotFileName((PivotItem)workspacePivot.Items[i]);
                    if (s == name)
                    {
                        return i;
                    }
                }
                return -1;
            }
            
            private PivotItem CreatePivotItem(string fileName, double commandBarHeight)
            {
                PivotItem newItem = new PivotItem();
                newItem.Height = Window.Current.Bounds.Height - commandBarHeight - 50;
                newItem.Header = CreatePivotItemHeader(fileName.Split('.')[0], "."+fileName.Split('.')[1]);
                
                newItem.Content = new Grid();
                Grid grid = (Grid)newItem.Content;
                grid.Children.Add(new Image());
                grid.VerticalAlignment = VerticalAlignment.Stretch;

                newItem.RenderTransform = new CompositeTransform();

                return newItem;
            }
            
            private StackPanel CreatePivotItemHeader(string fileDisplayName, string fileType)
            {
                StackPanel headerStack = new StackPanel();
                headerStack.Orientation = Orientation.Horizontal;
                TextBlock fileNameBlock = new TextBlock();
                fileNameBlock.Opacity = 0.6;

                Run displayName = new Run();
                displayName.FontSize = 21;
                displayName.Text = fileDisplayName;
                displayName.FontWeight = Windows.UI.Text.FontWeights.SemiBold;

                Run type = new Run();
                type.FontSize = 0.1;
                type.Foreground = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(1, 231, 231, 231));
                type.Text = fileType;
                type.FontWeight = Windows.UI.Text.FontWeights.ExtraLight;

                fileNameBlock.Inlines.Add(displayName);
                fileNameBlock.Inlines.Add(type);

                headerStack.Children.Add(fileNameBlock);
                headerStack.RenderTransform = new CompositeTransform();
                headerStack.Children.Add(new Canvas());
                return headerStack;
            }

            private string GetHeaderStackName(PivotItem pivotItem)
            {
                return ((StackPanel)pivotItem.Header).Name;
            }

            public void DeleteFromList(string s)
            {
                Debug.WriteLine("DeleteFromList");
                List<int> itemIndex = new List<int>();
                foreach(PivotItem item in workspacePivot.Items)
                {
                    if(GetPivotFileName(item) == s)
                    {
                        workspaceList.Remove(s);
                        workspacePivot.Items.Remove(item);
                    }
                }
            }


            private string GetPivotFileName(PivotItem pivotItem)
            {
                Debug.WriteLine("GetPivotFileName");
                string fileName = null;
                if(pivotItem != null)
                {
                    StackPanel stackHeader = (StackPanel)pivotItem.Header;
                    fileName = ((TextBlock)(stackHeader.Children[0])).Text;
                }
                return fileName;
            }
            
            public void RemoveCurrent()
            {
                if (workspacePivot.Items.Count > 1)
                {
                    if(workspacePivot.SelectedIndex == 0)
                    {
                        workspacePivot.SelectedIndex = 1;
                        workspacePivot.Items.RemoveAt(0);
                    }
                    else
                    {
                        workspacePivot.SelectedIndex = workspacePivot.SelectedIndex - 1;
                        workspacePivot.Items.RemoveAt(workspacePivot.SelectedIndex + 1);
                    }
                }
            }

            public void SetCurrent(int index)
            {
                if (index < workspaceList.Count)
                {
                    workspacePivot.SelectedIndex = index;
                }
            }

            public int GetPivotItemCount()
            {
                return workspacePivot.Items.Count;
            }

            private async void NotifyUser(string message)
            {
                MessageDialog dialog = new MessageDialog(message);
                await dialog.ShowAsync();
            }

            private void SetScrollViewerProperties(ScrollViewer scrollView, Grid grid, string type)
            {
                scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                scrollView.ZoomMode = ZoomMode.Enabled;

                switch(type)
                {
                    case "PDF": scrollView.ViewChanged += PdfScrollView_ViewChanged;
                                scrollView.SizeChanged += PdfScrollView_SizeChanged;
                                break;
                    case "PNG":
                    case "JPEG":scrollView.SizeChanged += ImageScrollView_SizeChanged;
                                break;
                    case "TXT": scrollView.SizeChanged += TxtScrollView_SizeChanged;
                                break;
                }
                
            }

            private void PdfScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                var scrollView = (ScrollViewer)sender;
                var panelWidth = ((StackPanel)scrollView.Content).ActualWidth;
                scrollView.ZoomToFactor((float)scrollView.ActualWidth / (float)panelWidth);
            }

            private void TxtScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                TextBlock block = (TextBlock)(((ScrollViewer)sender).Content);
                block.Width = ((ScrollViewer)sender).ActualWidth;
            }

            private void ImageScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                var scrollView = (ScrollViewer)sender;
                var imageWidth = ((Image)scrollView.Content).ActualWidth;
                scrollView.ZoomToFactor((float)scrollView.ActualWidth / (float)imageWidth);
            }
            
            private ProgressRing CreateProgressRing()
            {
                ProgressRing progressRing = new ProgressRing();
                progressRing.Foreground = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 0, 143, 174));
                progressRing.Height = 100;
                progressRing.Width = 100;
                progressRing.IsActive = true;
                return progressRing;
            }

            private async void LoadPdfImage(int i, Grid imageContainer, Image img)
            {
                try
                {
                    if (imageContainer.Children.Count < 3)
                    {
                        ProgressRing progressRing = CreateProgressRing();
                        imageContainer.Children.Add(progressRing);
                        PdfPage pdfPage = _pdfDocument.GetPage((uint)i);
                        var stream = new InMemoryRandomAccessStream();
                        PdfPageRenderOptions options = new PdfPageRenderOptions();
                        options.BitmapEncoderId = BitmapEncoder.JpegXREncoderId;
                        options.DestinationHeight = (uint)(pdfPage.Dimensions.ArtBox.Height);
                        options.DestinationWidth = (uint)(pdfPage.Dimensions.ArtBox.Width);
                        await pdfPage.RenderToStreamAsync(stream, options);
                        BitmapImage pdfImg = new BitmapImage();
                        pdfImg.SetSource(stream);
                        img.Source = pdfImg;
                        
                        imageContainer.Height = pdfImg.PixelHeight;
                        imageContainer.Width = pdfImg.PixelWidth;

                        progressRing.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("LoadPdfImage: "+e.ToString());
                }
            }

            private void PdfScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
            {
                if (e.IsIntermediate == false)
                {
                    ScrollViewer pdfScrollView = (ScrollViewer)sender;
                    StackPanel pdfStackPanel = (StackPanel)pdfScrollView.Content;

                    var ttv = pdfScrollView.TransformToVisual(Window.Current.Content);
                    Point scrollViewCoords = ttv.TransformPoint(new Point(0, 0));

                    if ((pdfScrollView).Content != null)
                    {
                        for (var i = 0; i < pdfStackPanel.Children.Count; i++)
                        {
                            Grid imageContainer = (Grid)pdfStackPanel.Children[i];
                            var ttv2 = imageContainer.TransformToVisual(Window.Current.Content);
                            Point imageCoords = ttv2.TransformPoint(new Point(0, 0));
                            var img = ((Image)(imageContainer).Children[0]);

                            double imageBottom = imageCoords.Y + imageContainer.ActualHeight;
                            double imageTop = imageCoords.Y;
                            double scrollViewTop = scrollViewCoords.Y;
                            double scrollViewBottom = scrollViewCoords.Y+pdfScrollView.ActualHeight;

                            if(imageTop<scrollViewBottom && imageTop>scrollViewTop)
                            {
                                LoadPdfImage(i, imageContainer, img);
                                if (i<pdfStackPanel.Children.Count-1)
                                {
                                    LoadPdfImage(i + 1, (Grid)pdfStackPanel.Children[i + 1], ((Image)((Grid)pdfStackPanel.Children[i + 1]).Children[0]));
                                }
                                if (i>0)
                                {
                                    LoadPdfImage(i - 1, (Grid)pdfStackPanel.Children[i - 1], ((Image)((Grid)pdfStackPanel.Children[i - 1]).Children[0]));
                                }
                            }
                        }
                    }
                }
            }

            Grid CreateImageContainer(double height, double width, int currentPageNum, int totalPageCount)
            {
                Grid imageContainer = new Grid();
                imageContainer.Height = height;
                imageContainer.Width = width;

                imageContainer.Background = new SolidColorBrush(Windows.UI.Colors.White);
                imageContainer.Margin = new Thickness(5);

                imageContainer.Children.Add(new Image());
                imageContainer.Children.Add(CreatePageNumberContainer(currentPageNum, totalPageCount));

                return imageContainer;
            }

            StackPanel CreatePageNumberContainer(int current, int total)
            {
                StackPanel pageNumContainer = new StackPanel();
                pageNumContainer.Background = new SolidColorBrush(Windows.UI.Colors.DimGray);
                pageNumContainer.Padding = new Thickness(4);

                TextBlock pageNumBlock = new TextBlock();

                Run currentNum = new Run();
                currentNum.Text = current.ToString();
                currentNum.FontSize = 20;

                Run totalNum = new Run();
                totalNum.Text = " / " + total.ToString();
                totalNum.FontSize = 12;

                pageNumBlock.Inlines.Add(currentNum);
                pageNumBlock.Inlines.Add(totalNum);
                pageNumBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                pageNumContainer.Children.Add(pageNumBlock);

                pageNumContainer.VerticalAlignment = VerticalAlignment.Top;
                pageNumContainer.HorizontalAlignment = HorizontalAlignment.Left;
                Canvas.SetZIndex(pageNumContainer, 2);

                return pageNumContainer;
            }

            private UIElement SetContentType(Grid grid, string type, uint count)
            {

                switch (type)
                {
                    case ".html":
                        grid.Children.Clear();
                        WebView webView = new WebView();
                        webView.VerticalAlignment = VerticalAlignment.Stretch;
                        webView.HorizontalAlignment = HorizontalAlignment.Stretch;
                        
                        grid.Children.Add(webView);
                        return grid.Children[0];

                    case ".png":
                    case ".jpg":
                        grid.Children.Clear();
                        ScrollViewer scrollView = new ScrollViewer();
                        SetScrollViewerProperties(scrollView, grid,"JPEG");

                        Image image = new Image();
                        image.SizeChanged += ScrollViewImage_Loaded;
                        image.Stretch = Stretch.Uniform;

                        scrollView.Content = image;                        
                        grid.Children.Add(scrollView);           
                        return image;

                    case ".pdf":
                        grid.Children.Clear();
                        ScrollViewer pdfScrollView = new ScrollViewer();
                        SetScrollViewerProperties(pdfScrollView, grid,"PDF");
                        grid.Children.Add(pdfScrollView);
                        StackPanel pdfStack = new StackPanel();
                        pdfScrollView.Content = pdfStack;
                        pdfStack.Width = pdfScrollView.Width;
                        return grid;

                    case ".csv":
                        grid.Children.Clear();
                        ScrollViewer csvScrollView = new ScrollViewer();
                        SetScrollViewerProperties(csvScrollView, grid, "CSV");
                        grid.Children.Add(csvScrollView);
                        ListView csvListView = new ListView();
                        csvListView.SelectionChanged += CsvListView_SelectionChanged;
                        csvScrollView.Content = csvListView;
                        return grid;

                    case ".txt":
                        grid.Children.Clear();
                        ScrollViewer txtScrollView = new ScrollViewer();
                        SetScrollViewerProperties(txtScrollView, grid, "TXT");
                        grid.Children.Add(txtScrollView);
                        TextBlock txtBlock = new TextBlock();
                        txtBlock.TextWrapping = TextWrapping.WrapWholeWords;
                        txtScrollView.Content = txtBlock;
                        return grid;

                    default:
                        return null;
                }
            }

            private void CsvListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (sender != null)
                {
                    ((ListView)sender).SelectedIndex = -1;
                }
            }

            private void ScrollViewImage_Loaded(object sender, RoutedEventArgs e)
            {
                Image image = (Image)sender;
                ScrollViewer scrollView = (ScrollViewer)image.Parent;

                if (image.ActualHeight > image.ActualWidth)
                {
                    scrollView.ZoomToFactor((float)scrollView.ActualHeight / (float)image.ActualHeight);
                }
                else
                {
                    scrollView.ZoomToFactor((float)scrollView.ViewportWidth / (float)image.ActualWidth);
                }
            }
            
            private void LoadPdf(PdfDocument pdfDocument, StackPanel stackPanel, ScrollViewer pdfScrollView)
            {
                _pdfDocument = pdfDocument;
                _pdfStack = stackPanel;
                for (uint i = 0; i < pdfDocument.PageCount; i++)
                {
                    stackPanel.Children.Add(CreateImageContainer(pdfDocument.GetPage(i).Dimensions.ArtBox.Height, pdfDocument.GetPage(i).Dimensions.ArtBox.Width, (int)i+1, (int)pdfDocument.PageCount));
                }
                LoadPdfImage(0, (Grid)stackPanel.Children[0], (Image)((Grid)(stackPanel.Children[0])).Children[0]);
                pdfScrollView.Loaded += PdfScrollView_Loaded;
            }

            private void PdfScrollView_Loaded(object sender, RoutedEventArgs e)
            {
                var pdfScrollView = (ScrollViewer)sender;
                var panelWidth = ((StackPanel)pdfScrollView.Content).ActualWidth;
                pdfScrollView.ZoomToFactor((float)pdfScrollView.ActualWidth / (float)panelWidth);
            }

            public async void SetPivotItemContent(int index, StorageFile file)
            {
                try
                {
                    Debug.WriteLine("SetPivotItemContent");
                    PivotItem pivotItem = (PivotItem)workspacePivot.Items[index];
                    
                    Grid grid = (Grid)pivotItem.Content;
                    grid.Children.Clear();
                    pivotItem.Header = CreatePivotItemHeader(file.DisplayName.Split('.')[0],file.FileType);

                    switch (file.FileType)
                    {
                        case ".html":
                            Debug.WriteLine("SetPivotItemContent .html");
                            WebView webView = (WebView)SetContentType(grid, ".html", 0);
                            var buffer = await FileIO.ReadTextAsync(file);
                            webView.NavigateToString(buffer);
                            break;
                        case ".jpg":
                            Debug.WriteLine("SetPivotItemContent .jpg");
                            Image image = (Image)SetContentType(grid, ".jpg", 0);
                            var fileStream = await file.OpenAsync(FileAccessMode.Read);
                            var img = new BitmapImage();
                            img.SetSource(fileStream);
                            image.Source = img;
                            break;
                        case ".pdf":
                            Debug.WriteLine("SetPivotItemContent .pdf");
                            PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(file);
                            grid = (Grid)SetContentType(grid, ".pdf", pdfDocument.PageCount);
                            LoadPdf(pdfDocument, ((StackPanel)(((ScrollViewer)(grid.Children[0])).Content)), (ScrollViewer)(grid.Children[0]));
                            break;
                        case ".csv":
                            Debug.WriteLine("SetPivotItemContent .csv");                            
                            grid = (Grid)SetContentType(grid, ".csv", 0);
                            LoadCsv((ListView)(((ScrollViewer)(grid.Children[0])).Content),file);
                            break;
                        case ".txt":
                            Debug.WriteLine("SetPivotItemContent .txt");
                            grid = (Grid)SetContentType(grid, ".txt", 0);
                            LoadTxt((TextBlock)(((ScrollViewer)grid.Children[0]).Content), file);
                            break;
                        case ".png":
                            Debug.WriteLine("SetPivotItemContent .png");
                            Image image_png = (Image)SetContentType(grid, ".png", 0);
                            var stream = await file.OpenAsync(FileAccessMode.Read);
                            var img_png = new BitmapImage();
                            img_png.SetSource(stream);
                            image_png.Source = img_png;
                            break;
                        default:
                            NotifyUser("Error: File extension not found.");
                            break;
                    }
                    
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
                
            }

            private async void LoadTxt(TextBlock textBlock, StorageFile file)
            {
                string txtContent = await FileIO.ReadTextAsync(file);
                textBlock.Text = txtContent;
            }

            private async void LoadCsv(ListView csvListView,StorageFile file)
            {
                string csvText = await FileIO.ReadTextAsync(file);
                TextReader reader = new StringReader(csvText);
                CsvParser csvParser = new CsvParser();
                string[][] results = csvParser.Parse(reader);

                int columnCount = results[0].Length;
                double[] maxWidth = new double[columnCount];
                Array.Clear(maxWidth, 0, columnCount);

                int i = 0;
                foreach (string[] result in results)
                {
                    csvListView.Items.Add(GenerateRow(result,i));
                    i++;
                }
            }

            private ListViewItem GenerateRow(string[] stringArray, int index)
            {
                ListViewItem listViewItem = new ListViewItem();

                SolidColorBrush listItemBackground;
                SolidColorBrush listItemForeground;
                if (index != 0)
                {
                    listItemBackground = new SolidColorBrush(Windows.UI.Colors.White);
                }
                else
                {
                    listItemBackground = new SolidColorBrush(Windows.UI.Colors.LightGray);
                }
                listItemForeground = new SolidColorBrush(Windows.UI.Colors.DimGray);
                listViewItem.Background = listItemBackground;    
                listViewItem.Padding = new Thickness(10);

                StackPanel rowPanel = new StackPanel();
                rowPanel.Orientation = Orientation.Horizontal;

                int i = 0;
                foreach(string val in stringArray)
                {
                    TextBlock block = new TextBlock();
                    block.Text = val;
                    block.Width = 100;
                    block.Tapped += Block_Tapped;
                    block.FontWeight = Windows.UI.Text.FontWeights.SemiBold;
                    block.Foreground = listItemForeground;
                    block.Margin = new Thickness(10);
                    rowPanel.Children.Add(block);
                    i++;
                }

                listViewItem.Content = rowPanel;
                return listViewItem;
            }

            private void Block_Tapped(object sender, TappedRoutedEventArgs e)
            {
                TextBlock block = (TextBlock)sender;
                if(block.Width == 100)
                {
                    block.Width = Double.NaN;
                    block.Foreground = new SolidColorBrush(Windows.UI.ColorHelper.FromArgb(255, 0, 99, 120));
                }
                else
                {
                    block.Width = 100;
                    block.Foreground = new SolidColorBrush(Windows.UI.Colors.DimGray);
                }
                
            }

            public void ShowDoc(StorageFile file)
            {
                Debug.WriteLine("ShowDoc");
                SetPivotItemContent(workspacePivot.SelectedIndex, file);
            }

            public string GetCurrentDoc()
            {
                Debug.WriteLine("GetCurrentDoc");
                StackPanel header = (StackPanel)(((PivotItem)(workspacePivot.SelectedItem)).Header);
                string fileName = ((TextBlock)(header.Children[0])).Text;
                return fileName;
            }

            public Workspace(Pivot workspacePivot)
            {
                var bounds = Window.Current.Bounds;
                this.workspacePivot = workspacePivot;
                this.workspacePivot.VerticalAlignment = VerticalAlignment.Top;
                this.workspacePivot.Height = bounds.Height;
                workspaceList = new List<string>();
            }
        };


        // Fill the local files list:
        public async void UpdateLocalList()
        {

            var preSelectedFile = (StorageFile)((FileItem)(LocalListView.SelectedItem));
            string preSelectedFileName = "";
            if (preSelectedFile != null)
            {
                preSelectedFileName = preSelectedFile.DisplayName + preSelectedFile.FileType;
            }

            Debug.WriteLine("UpdateLocalList");
            localFilesList = await GetLocalFiles();
            fileNameList.Clear();

            var i = 0;
            var nextSelectedIndex = 0;

            foreach (StorageFile file in localFilesList)
            {
                if (fileNameList.IndexOf(file.DisplayName) == -1)
                {
                    fileNameList.Add(file.DisplayName);
                }
                if ((file.DisplayName + file.FileType) == preSelectedFileName)
                {
                    nextSelectedIndex = i;
                }
                i++;
            }
            LocalListView.ItemsSource = null;
            fileItemList.Clear();
            foreach(StorageFile item in localFilesList)
            {
                FileItem newFileItem = new FileItem(item);

                BasicProperties properties = await item.GetBasicPropertiesAsync();
                double size = properties.Size;
                newFileItem.fileSizeBytes = size;
                string suffix = "B";
                if (size > 1024)
                {
                    size /= 1024;
                    suffix = "KB";
                }
                if (size > 1024)
                {
                    size /= 1024;
                    suffix = "MB";
                }
                size = Math.Round(size, 1);
                newFileItem.fileSize = (size.ToString() + " " + suffix);
                
                fileItemList.Add(newFileItem);
            }
            
            LocalListView.ItemsSource = fileItemList;

            Filter_ComboBox.SelectedIndex = -1;
            if(Sort_ComboBox.SelectedIndex != -1)
            {
                SortBy((string)((ComboBoxItem)Sort_ComboBox.SelectedItem).Content);
            }

            LocalListView.SelectedIndex = 0;
        }

        private void SetReadStatus(string fileName, ListViewItem lvItem)
        {
            if (lvItem != null)
            {
                if ((string)ApplicationData.Current.LocalSettings.Values[fileName] == "read")
                {
                    lvItem.ContentTemplate = (DataTemplate)Resources["ReadItemTemplate"];
                }
                else
                {
                    lvItem.ContentTemplate = (DataTemplate)Resources["UnreadItemTemplate"];
                }
            }
        }

        public async void UpdateLocalList_Thread()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                UpdateLocalList();
            });
        }
        

        public async Task<List<StorageFile>> GetLocalFiles()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;
            StorageFileQueryResult queryResult =  localFolder.CreateFileQuery();
            IReadOnlyList<StorageFile> fileList = await queryResult.GetFilesAsync();
            List<StorageFile> localList = new List<StorageFile>();
            foreach (StorageFile file in fileList)
            {
                localList.Add(file);
            }
            return localList;
        }

        async private void SaveToDeviceBtn_Click(object sender, RoutedEventArgs args)
        {

            if(LocalListView.SelectionMode == ListViewSelectionMode.Single)
            {
                try
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    StorageFile file = (StorageFile)((FileItem)(LocalListView.SelectedItem));
                    savePicker.FileTypeChoices.Add(file.DisplayType, new List<string>() { file.FileType });
                    savePicker.SuggestedFileName = file.DisplayName;

                    StorageFile newFile = await savePicker.PickSaveFileAsync();
                    if (newFile != null)
                    {
                        await file.CopyAndReplaceAsync(newFile);
                        NotifyUser("File saved successfully.");
                    }
                }
                catch (Exception e)
                {
                    MessageDialog dialog = new MessageDialog("Something occured: " + e.ToString());
                    await dialog.ShowAsync();
                    return;
                }
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                async () =>
                {
                    try
                    {
                        var folderPicker = new FolderPicker();
                        folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                        folderPicker.FileTypeFilter.Add(".jpeg");
                        folderPicker.ViewMode = PickerViewMode.Thumbnail;
                        folderPicker.SettingsIdentifier = "FolderPicker";
                        var folder = await folderPicker.PickSingleFolderAsync();
                        var i = 1;
                        foreach (FileItem file in LocalListView.SelectedItems)
                        {
                            StorageFile storageItem = (StorageFile)file;
                            await storageItem.CopyAsync(folder,storageItem.DisplayName+storageItem.FileType,option:NameCollisionOption.GenerateUniqueName);
                            i++;
                        }
                        NotifyUser("All files saved successfully.");
                    }
                    catch (Exception e)
                    {
                        NotifyUser("Something occured: " + e.ToString());
                        return;
                    }
                });
                
            }

        }

        public async Task<StorageFile> GetLocalFile(string fileName)
        {
            try
            {
                StorageFile localFile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(fileName);
                return localFile;
            }
            catch(Exception exc){
                Debug.WriteLine("Exception: " + exc.ToString());
                return null;
            }
            
        }

        public async void RunTCPListener()
        {
            _listener = new StreamSocketListener();
            await _listener.BindServiceNameAsync("2112");
            _listener.ConnectionReceived += OnConnection;
        }

        public async void NotifyUser(string Message)
        {
            MessageDialog dialog = new MessageDialog(Message);
            await dialog.ShowAsync();
        }

        public async void NotifyUser_Thread(string Message)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                MessageDialog dialog = new MessageDialog(Message);
                dialog.ShowAsync();
            });
        }        

        public async void UpdateFileReceptionStatus(string status)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                FileReception_Status.Text = status;
            });
        }


        public int GetContentLength(string content)
        {
            Regex regex = new Regex("Content-Length: ([0-9]*)");
            Match match = regex.Match(content);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return 0;   
        }

        public IDictionary<string,string> ParseContent(string content)
        {
            Regex r = new Regex("([A-Za-z]*)=([^&]*)");
            IDictionary<string, string> contentDict = new Dictionary<string,string>();
            
            Match m = r.Match(content);
            while (m.Success)
            {
                contentDict.Add(new KeyValuePair<string, string>(WebUtility.UrlDecode(m.Groups[1].Value), WebUtility.UrlDecode(m.Groups[2].Value)));
                m = m.NextMatch();
            }

            return contentDict;

        }

        public async void WriteFileToLocal(IDictionary<string,string> ContentData)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;
            
            StorageFile newFile = await localFolder.CreateFileAsync(ContentData["name"] + ContentData["type"], CreationCollisionOption.GenerateUniqueName);

            switch (ContentData["type"])
            {
                case ".csv":
                case ".txt":
                case ".html": using (IRandomAccessStream textStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
                              {
                                    using (DataWriter textWriter = new DataWriter(textStream))
                                    {
                                        textWriter.WriteString(ContentData["data"]);
                                        await textWriter.StoreAsync();
                                    }
                              }
                            break;
                case ".png":
                case ".pdf":
                case ".jpg":   using (IRandomAccessStream textStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    using (DataWriter textWriter = new DataWriter(textStream))
                                    {
                                        var bytes = Convert.FromBase64String(ContentData["data"]);
                                        textWriter.WriteBytes(bytes);
                                        await textWriter.StoreAsync();
                                        textWriter.DetachStream();
                                        textWriter.Dispose();
                                    }
                                    await textStream.FlushAsync();
                                    textStream.Dispose();
                                }
                            break;

                default: NotifyUser_Thread("Can't write file");
                    break;
            }
            ApplicationData.Current.LocalSettings.Values[newFile.DisplayName+newFile.FileType] = "unread";
            UpdateLocalList_Thread();
        }

        public async void WriteUniqueFileToLocal(IDictionary<string, string> ContentData)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;

            StorageFile newFile = await localFolder.CreateFileAsync(ContentData["name"] + ContentData["type"], CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStream textStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter textWriter = new DataWriter(textStream))
                {
                    textWriter.WriteString(ContentData["data"]);
                    await textWriter.StoreAsync();
                }
            }

        }

        private async void UpdateFileProgressBar(double status)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                if (ProgressBarPanel.ActualHeight != 20)
                {
                    ProgressBarPanel.Height = 20;
                }
                FileReceptionBar.Value = status;
            });
        }

        private string BuildPostResponse(string responseType, string fileSize, string timeTaken)
        {
            string responseString = "HTTP/1.1 200 OK";
            responseString += "\r\nAccess-Control-Allow-Origin: *";
            responseString += "\r\nContent-Type: application/x-www-form-urlencoded; charset=UTF-8";
            responseString += "\r\n";
            responseString += "\r\n";

            Dictionary<string, string> dict = new Dictionary<string, string>();

            switch (responseType)
            {
                case "FIND_DEVICE": dict["Message"] = "SUCCESS";
                    dict["DeviceInfo"] = IP_info;
                    break;

                case "SEND_FILE":
                    dict["Message"] = "SUCCESS";
                    dict["FileSize"] = fileSize;
                    dict["TimeTaken"] = timeTaken;
                    break;
            }
            
            responseString += JsonConvert.SerializeObject(dict);
            return responseString;
        }

        private async void OpenReceptionPanel()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                ShowIpInfoBtn.IsChecked = true;
                //ProgressBarExpandAnim.Begin();
            });
        }


        private async void OnConnection( StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                OpenReceptionPanel();
                using (IInputStream inStream = args.Socket.InputStream)
                {
                    UpdateFileReception_FileName("");
                    UpdateFileReceptionStatus("Connected by " + args.Socket.Information.RemoteHostName.CanonicalName);
                    Update_ProgressPanel_RemoteHost(args.Socket.Information.RemoteHostName.CanonicalName);

                    DataReader reader = new DataReader(inStream);
                    reader.InputStreamOptions = InputStreamOptions.Partial;
                    int contentLength = 0;
                    uint numReadBytes;
                    string totalContent = "";
                    DateTime timeStart = DateTime.Now;

                    IOutputStream outStream = args.Socket.OutputStream;

                    do
                    {
                        numReadBytes = await reader.LoadAsync(1 << 20);
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {
                            if (ProgressBarPanel.Height == 0)
                            {
                                ProgressBarExpandAnim.Begin();
                            }
                        });
                        if (numReadBytes > 0)
                        {
                            byte[] tmpBuf = new byte[numReadBytes];
                            reader.ReadBytes(tmpBuf);
                            string result = Encoding.UTF8.GetString(tmpBuf).TrimEnd('\0');
                            string[] contents = Regex.Split(result, "\r\n\r\n");
                            if (GetContentLength(result) != 0)
                            {
                                contentLength = GetContentLength(result);
                            }
                            string content;
                            if (contents.Length > 1)
                            {
                                content = contents[1];
                            }
                            else
                            {
                                content = contents[0];
                            }
                            totalContent += content;
                            UpdateFileReception_FileName("");
                            UpdateFileReceptionStatus("Receiving: " + ((ulong)totalContent.Length * 100 / (ulong)contentLength) + "%");
                            UpdateFileProgressBar(((ulong)totalContent.Length * 100 / (ulong)contentLength));
                            if (totalContent.Length == contentLength)
                            {

                                UpdateFileReceptionStatus("Processing the stuff received...");

                                var ContentData = ParseContent(totalContent);
                                string param_fileSize = "";
                                string param_timeTaken = "";
                                if (ContentData["action"] == "FIND_DEVICE")
                                {
                                    UpdateFileReception_FileName(args.Socket.Information.RemoteHostName.CanonicalName);
                                    UpdateFileReceptionStatus(" connected.");
                                }
                                else
                                {
                                    param_fileSize = (ContentData["data"].Length).ToString();
                                    TimeSpan diff = DateTime.Now - timeStart;
                                    param_timeTaken = diff.TotalSeconds.ToString();
                                    UpdateFileReceptionStatus("Writing to your storage...");
                                    WriteFileToLocal(ContentData);
                                    UpdateFileReception_FileName(ContentData["name"] + ContentData["type"]);
                                    UpdateFileReceptionStatus(" received.");
                                }
                                IBuffer replyBuff = Encoding.ASCII.GetBytes(BuildPostResponse(ContentData["action"],param_fileSize,param_timeTaken)).AsBuffer();
                                await outStream.WriteAsync(replyBuff);
                                reader.DetachStream();
                                args.Socket.Dispose();
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                                () =>
                                {
                                    ProgressBarCollapseAnim.Begin();
                                });
                                return;
                            }
                        }
                    } while (numReadBytes>0);
                }
            }
            catch(Exception e)
            {
                NotifyUser_Thread(e.Message + ", " + e.Source + ", "+ e.StackTrace);
            }
        }

        private async void UpdateFileReception_FileName(string v)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                FileReception_Name.Text = v;
            });
        }

        private async void Update_ProgressPanel_RemoteHost(string v)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
            () =>
            {
                ProgressPanel_RemoteHost.Text = v;
            });
        }

        private void NewTabBtn_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            if(LocalListView.SelectionMode == ListViewSelectionMode.Single)
            {
                workspace.AddToList((StorageFile)((FileItem)(LocalListView.SelectedItem)), FileCommandBar.ActualHeight);
                WorkspacePivot.SelectedIndex = WorkspacePivot.Items.Count - 1;
                NewItemTransition((PivotItem)WorkspacePivot.SelectedItem);
            }
            else
            {
                foreach (FileItem fileItem in LocalListView.SelectedItems)
                {
                    StorageFile file = (StorageFile)fileItem;
                    string fileName = file.DisplayName + file.FileType;
                    if (workspace.InList(fileName) == -1)
                    {
                        workspace.AddToList(file, FileCommandBar.ActualHeight);
                    }
                }

                MultiSelectBtn.IsChecked = false;
                LocalListView.SelectedIndex = _preSelectedIndex;
                ((Storyboard)Resources["WorkspacePivotEaseOut_Left"]).Begin();
            }
        }

        private void CloseTabBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (WorkspacePivot.Items.Count > 1)
            {
                MultiSelectBtn.IsChecked = false;
                CloseTabTransition((PivotItem)WorkspacePivot.SelectedItem);
            }
        }

        private void DragPanelStatus(bool status)
        {
            if (status == true)
            {
                DragPanel.SetValue(Canvas.ZIndexProperty, 1);
                ((Storyboard)Resources["DragPanel_Expand"]).Begin();
                ((Storyboard)Resources["WorkspacePivot_DragContract"]).Begin();
            }
            else
            {
                DragPanel.SetValue(Canvas.ZIndexProperty, -1);
                ((Storyboard)Resources["DragPanel_Vanish"]).Begin();
                ((Storyboard)Resources["WorkspacePivot_DragExpand"]).Begin();
            }
            
        }

        private void WorkspacePivot_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void WorkspacePivot_Drop(object sender, DragEventArgs e)
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach(StorageFile file in items)
            {
                Debug.WriteLine("e.DataView: " + file.DisplayName);
                workspace.AddToList(file, FileCommandBar.ActualHeight);
            }
            WorkspacePivot.SelectedIndex = WorkspacePivot.Items.Count - 1;
            DragPanelStatus(false);
        }

        private void LocalListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            DragPanelStatus(false);
        }
        private void ListView_DragStarting(object sender, DragItemsStartingEventArgs e)
        {
            int itemCount = e.Items.Count;
            e.Data.SetText(itemCount.ToString());               
            List<StorageFile> fileList = new List<StorageFile>();
            foreach (FileItem fileItem in e.Items)
            {
                fileList.Add((StorageFile)fileItem);
            }
            e.Data.SetStorageItems(fileList);

            e.Data.RequestedOperation = DataPackageOperation.Copy;
            DragPanelStatus(true);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            _matchingNamesList.Clear();
            foreach(FileItem fileItem in fileItemList)
            {
                string name = fileItem.fileDisplayName;
                if ((name.ToLower()).Contains((SuggestBox.Text).ToLower()))
                {
                    _matchingNamesList.Add(name);
                }
            }
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = null;
                sender.ItemsSource = _matchingNamesList;
            }
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var submittedName = args.QueryText;
            List<FileItem> fileItemList_queried = new List<FileItem>();
            foreach(FileItem fileItem in fileItemList)
            {
                if (fileItem.fileDisplayName.Contains(submittedName))
                    fileItemList_queried.Add(fileItem);
            }
            LocalListView.ItemsSource = null;
            LocalListView.ItemsSource = fileItemList_queried;
        }

        private async void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void LoadAll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            UpdateLocalList();
            ((Button)sender).Content = "";
        }

        private void ShareMailBtn_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareMailHandler;
            DataTransferManager.ShowShareUI();
        }

        private async void ShareMailHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            DataRequestDeferral deferral = request.GetDeferral();

            if(LocalListView.SelectionMode == ListViewSelectionMode.Single)
            {

                StorageFile file = (StorageFile)((FileItem)(LocalListView.SelectedItem));
                request.Data.Properties.Title = file.DisplayName;
                request.Data.Properties.Description = "Share the current file via DocOnlook.";

                switch (file.FileType)
                {
                    case ".txt":
                    case ".html":
                        string HTMLContent;
                        HTMLContent = await FileIO.ReadTextAsync(file);
                        string htmlFormat = HtmlFormatHelper.CreateHtmlFormat(HTMLContent);
                        request.Data.SetHtmlFormat(htmlFormat);
                        break;
                    case ".png":
                    case ".jpg":
                        RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
                        request.Data.SetBitmap(imageStreamRef);
                        break;
                    case ".csv":
                    case ".pdf":
                        List<StorageFile> storageList = new List<StorageFile>();
                        storageList.Add(file);
                        request.Data.SetStorageItems(storageList);
                        break;
                }
            }
            else
            {
                List<StorageFile> fileList = new List<StorageFile>();
                foreach(FileItem fileItem in LocalListView.SelectedItems)
                {
                    StorageFile file = (StorageFile)fileItem;
                    fileList.Add(file);
                    request.Data.Properties.Title += file.DisplayName + "; ";
                }
                request.Data.Properties.Description = "Share files via DocOnlook.";
                request.Data.SetStorageItems(fileList);
            }
           
            deferral.Complete();
        }

        private void ShowDebugInfoBtn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PivotItem pivotItem = (PivotItem)WorkspacePivot.Items[0];
            StackPanel stackPanel = (StackPanel)pivotItem.Content;
            Image image = (Image)(((ScrollViewer)stackPanel.Children[0]).Content);
            NotifyUser(image.Height + " " + image.Width + " " + image.ActualHeight + " " + image.ActualWidth);
        }

        private async void DeleteBtn_Tapped(object sender, TappedRoutedEventArgs args)
        {
            Debug.WriteLine("DeleteBtn");
            if (LocalListView.SelectionMode == ListViewSelectionMode.Single)
            {
                StorageFile file = (StorageFile)((FileItem)(LocalListView.SelectedItem));
                try
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    ApplicationData.Current.LocalSettings.Values[file.DisplayName + file.FileType] = "unread";
                    workspace.DeleteFromList(file.DisplayName + file.FileType);
                }
                catch (Exception e)
                {
                    NotifyUser("Sorry, we couldn't delete the file: " + e.Message + ", " + e.HelpLink);
                    return;
                }
                NotifyUser("File deleted successfully.");
                UpdateLocalList();
            }
            else
            {
                int count = LocalListView.SelectedItems.Count;
                try
                {
                    List<string> deleteFileNames = new List<string>();
                    foreach (FileItem fileItem in LocalListView.SelectedItems)
                    {
                        StorageFile file = (StorageFile)fileItem;
                        ApplicationData.Current.LocalSettings.Values[file.DisplayName + file.FileType] = "unread";
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        workspace.DeleteFromList(file.DisplayName + file.FileType);
                    }
                }
                catch (Exception e)
                {
                    NotifyUser("Sorry, we couldn't delete the files: " + e.Message + ", " + e.HelpLink);
                    return;
                }
                NotifyUser(count + " file(s) deleted successfully.");
                MultiSelectBtn.IsChecked = false;
                UpdateLocalList();
            }
        }

        private async void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if(LocalListView.SelectionMode == ListViewSelectionMode.Single)
            {
                StorageFile file = (StorageFile)((FileItem)(LocalListView.SelectedItem));
                await Launcher.LaunchFileAsync(file, new LauncherOptions
                {
                    DisplayApplicationPicker = true,
                    DesiredRemainingView = ViewSizePreference.UseHalf
                });
                LauncherOptions options = new LauncherOptions();
                return;
            }
        }

        private void LocalListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            Debug.WriteLine("SelectionChanged "+LocalListView.SelectedIndex);
            if (LocalListView.ItemsSource!=null && LocalListView.SelectionMode!=ListViewSelectionMode.Multiple && LocalListView.SelectedIndex!=-1)
            {
                try
                {
                    FileItem fileItem = ((FileItem)(LocalListView.SelectedItem));
                    var file = (StorageFile)((FileItem)(LocalListView.SelectedItem));
                    ApplicationData.Current.LocalSettings.Values[fileItem.fileDisplayName+fileItem.fileType] = "read";

                    if (LocalListView.SelectedItem != null)
                    {
                        ListViewItem lvItem = (ListViewItem)LocalListView.ContainerFromItem(LocalListView.SelectedItem);
                        SetReadStatus(fileItem.fileDisplayName + fileItem.fileType, lvItem);
                    }
                    if (workspace.GetPivotItemCount() == 0)
                    {
                        workspace.AddToList(file, FileCommandBar.ActualHeight);
                    }
                    else if (workspace.GetCurrentDoc() != (file.DisplayName + file.FileType))
                    {
                        Debug.WriteLine("GetCurrentDoc: " + workspace.GetCurrentDoc());
                        workspace.ShowDoc(file);
                        NewItemTransition((PivotItem)WorkspacePivot.SelectedItem);
                    }
                }
                catch (Exception e)
                {
                    NotifyUser("Sorry, we couldn't open the file: " + e.Message + ", " + e.HelpLink);
                }
            }
        }

        private void ChangeReadStatus(ListViewItem lvItem)
        {
            VisualStateManager.GoToState(lvItem, "Read", false);
        }

        public class PdfPageListItem
        {
            public BitmapImage BmImage { get; set; }
        }

        private void testOutput_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pdfScrollView = (ScrollViewer)(((StackPanel)(((PivotItem)(WorkspacePivot.SelectedItem)).Content)).Children[0]);
            NotifyUser(pdfScrollView.ActualWidth.ToString());
        }

        private void WorkspacePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Pivot_SelectionChanged");
            if (WorkspacePivot.Items.Count > 0)
            {
                var header = (StackPanel)(((PivotItem)WorkspacePivot.SelectedItem).Header);
                String fileName = ((TextBlock)header.Children[0]).Text;
                foreach (FileItem fileItem in LocalListView.Items)
                {
                    StorageFile storageItem = (StorageFile)fileItem;
                    if (storageItem.DisplayName + storageItem.FileType == fileName)
                    {
                        LocalListView.SelectedIndex = LocalListView.Items.IndexOf(fileItem);
                    }
                }
            }
        }

        private void FileCommandBar_Opening(object sender, object e)
        {
            CommandBar bar = (CommandBar)sender;
            AppBarToggleButton collapseBtn = (AppBarToggleButton)bar.Content;
            collapseBtn.Label = "Toggle files";
        }

        private void FileCommandBar_Closed(object sender, object e)
        {
            CommandBar bar = (CommandBar)sender;
            AppBarToggleButton collapseBtn = (AppBarToggleButton)bar.Content;
            collapseBtn.Label = "";
        }

        private void IPInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {             
            NotifyUser(LocalListView.ItemContainerGenerator.ToString());
        }

        private void LocalListView_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (FileItem fileItem in LocalListView.Items)
            {
                ListViewItem lvItem = (ListViewItem)LocalListView.ContainerFromItem(fileItem);
                SetReadStatus(fileItem.fileDisplayName + fileItem.fileType, lvItem);
            }
        }
        private void LocalListView_LayoutUpdated(object sender, object e)
        {
            foreach (FileItem fileItem in LocalListView.Items)
            {
                ListViewItem lvItem = (ListViewItem)LocalListView.ContainerFromItem(fileItem);
                SetReadStatus(fileItem.fileDisplayName + fileItem.fileType, lvItem);
            }
        }

        private void RefreshList()
        {
            LocalListView.ItemsSource = null;
            LocalListView.ItemsSource = fileItemList;
        }

        private void Filter_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Filter_ComboBox.SelectedIndex != -1)
            {
                string selectedContent = (string)((ComboBoxItem)(Filter_ComboBox.SelectedItem)).Content;
                FilterListBy(selectedContent);
            }
        }

        private void FilterListBy(string filter)
        {
            List<FileItem> fileItemList_filter = new List<FileItem>();
            string filterExtension = "other";
            switch (filter)
            {
                case "HTML": filterExtension = ".html";
                    break;
                case "PDF":
                    filterExtension = ".pdf";
                    break;
                case "CSV":
                    filterExtension = ".csv";
                    break;
                case "TXT":
                    filterExtension = ".txt";
                    break;
                case "JPG":
                    filterExtension = ".jpg";
                    break;
                case "PNG":
                    filterExtension = ".png";
                    break;
            }
            if (filterExtension != "other")
            {
                foreach(FileItem fileItem in fileItemList)
                {
                    if(fileItem.fileType == filterExtension)
                    {
                        fileItemList_filter.Add(fileItem);
                    }
                }
            }
            else if(filter == "Unread")
            {
                foreach (FileItem fileItem in fileItemList)
                {
                    if ((string)ApplicationData.Current.LocalSettings.Values[fileItem.fileDisplayName+fileItem.fileType] == "unread")
                    {
                        fileItemList_filter.Add(fileItem);
                    }
                }
            }
            else
            {
                UpdateLocalList();
            }
            if (Filter_ComboBox.SelectedIndex != -1)
            {
                LocalListView.ItemsSource = null;
                LocalListView.ItemsSource = fileItemList_filter;
                if (Sort_ComboBox.SelectedIndex != -1)
                {
                    string sortContent = (string)((ComboBoxItem)(Sort_ComboBox.SelectedItem)).Content;
                    SortBy(sortContent);
                }
            }
        }

        private void Sort_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Sort_ComboBox.SelectedIndex != -1)
            {
                string sortContent = (string)((ComboBoxItem)(Sort_ComboBox.SelectedItem)).Content;
                SortBy(sortContent);
            }
        }

        private void SortBy(string sortBy)
        {
            string order = "";
            string criteria = "";

            List<FileItem> currentFileItemList = GetCurrentList();

            switch (sortBy)
            {
                case "Name Asc.": order = "ASC";
                    criteria = "Name";
                    break;
                case "Name Desc.":
                    order = "DESC";
                    criteria = "Name";
                    break;
                case "Size Asc.":
                    order = "ASC";
                    criteria = "Size";
                    break;
                case "Size Desc.":
                    order = "DESC";
                    criteria = "Size";
                    break;
                case "Oldest":
                    order = "ASC";
                    criteria = "Date";
                    break;
                case "Latest":
                    order = "DESC";
                    criteria = "Date";
                    break;
                case "None":
                    
                    break;
            }
            switch (criteria)
            {
                case "Name": 
                        currentFileItemList.Sort((x, y) => string.Compare(x.fileDisplayName,y.fileDisplayName));
                    break;
                case "Size":
                    currentFileItemList.Sort((x, y) => (int)x.fileSizeBytes - (int)y.fileSizeBytes);
                    break;
                case "Date":
                    currentFileItemList.Sort((x, y) => DateTime.Compare(x.fileDateCreated_DateTime, y.fileDateCreated_DateTime));
                    break;
            }
            if (order == "DESC")
            {
                currentFileItemList.Reverse();
            }

            LocalListView.ItemsSource = null;
            LocalListView.ItemsSource = currentFileItemList;
            
            if(LocalListView.Items.Count > 0)
            LocalListView.SelectedIndex = 0;
        }

        private List<FileItem> GetCurrentList()
        {
            List<FileItem> currentFileItemList = new List<FileItem>();
            foreach (FileItem fileItem in LocalListView.Items)
            {
                currentFileItemList.Add(fileItem);
            }
            return currentFileItemList;
        }
        
        private void ShowIpInfoBtn_Checked(object sender, RoutedEventArgs e)
        {
            BottomPanelExpandAnim.Begin();
        }
       
        private void ShowIpInfoBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateFileReception_FileName("");
            UpdateFileReceptionStatus("No activity");
            BottomPanelCollapseAnim.Begin();
        }

        private void MultiSelectBtn_Checked(object sender, RoutedEventArgs e)
        {
            _preSelectedIndex = LocalListView.SelectedIndex;
            LocalListView.SelectionMode = ListViewSelectionMode.Multiple;
            LocalListView.SelectedIndex = _preSelectedIndex;
        }

        private void MultiSelectBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            LocalListView.SelectionMode = ListViewSelectionMode.Single;
            LocalListView.SelectedIndex = _preSelectedIndex;
        }

        private void Collapse_Checked(object sender, RoutedEventArgs e)
        {
            var windowWidth = Window.Current.CoreWindow.Bounds.Width;
            SideBar_Collapse.Begin();

            SetPrimaryButtonsVisibility(Visibility.Visible);
            SetSecondaryButtonsVisibility(Visibility.Collapsed);
        }
        private void SideBar_Collapse_Completed(object sender, object e)
        {
            SideBar_ColDef.MinWidth = 0;
            SideBar_ColDef.Width = new GridLength(0, GridUnitType.Star);
            WorkspaceEaseOut_Right.Begin();
        }

        private void SetPrimaryButtonsVisibility(Visibility visibility)
        {
            foreach(AppBarButton btn in FileCommandBar.PrimaryCommands)
            {
                btn.Visibility = visibility;
            }
        }

        private void SetSecondaryButtonsVisibility(Visibility visibility)
        {
            foreach (AppBarButton btn in FileCommandBar.SecondaryCommands)
            {
                btn.Visibility = visibility;
            }
        }

        private void Collapse_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.Bounds.Width >= collapseThresholdWidth)
            {
                SideBar_ColDef.MinWidth = 325;
                SideBar_ColDef.Width = new GridLength(3.5, GridUnitType.Star);
            }
            else
            {
                SideBar_ColDef.Width = new GridLength(325, GridUnitType.Pixel);
                SetPrimaryButtonsVisibility(Visibility.Collapsed);
                SetSecondaryButtonsVisibility(Visibility.Visible);
            }
            SideBar_Expand.Begin();
            WorkspaceEaseOut_Right_Reverse.Begin();
        }

        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            if(Collapse.IsChecked == true)
            {
                userCollapse = true;
            }
            else
            {
                userCollapse = false;
            }
        }

        private void NewItemTransition(PivotItem item)
        {
            if (item!=null)
            {
                ClearAllStoryboards();
                foreach (DoubleAnimation anim in PivotItemEnter.Children)
                {
                    Storyboard.SetTarget(anim, item);
                }
                foreach (DoubleAnimation anim in PivotItemHeaderEnter.Children)
                {
                    Storyboard.SetTarget(anim, (StackPanel)item.Header);
                }
                PivotItemEnter.Begin();
                PivotItemHeaderEnter.Begin(); 
            }
        }
        
        private void CloseTabTransition(PivotItem item)
        {
            if (item != null)
            {
                ClearAllStoryboards();
                foreach (DoubleAnimation anim in PivotItemExit.Children)
                {
                    Storyboard.SetTarget(anim, item);
                }
                foreach (DoubleAnimation anim in PivotItemHeaderExit.Children)
                {
                    Storyboard.SetTarget(anim, (StackPanel)item.Header);
                }
                
                PivotItemExit.Begin();
                PivotItemHeaderExit.Begin();
            }
        }

        private void PivotItemHeaderExit_Completed(object sender, object e)
        {
            workspace.RemoveCurrent();
        }

        private void ClearAllStoryboards()
        {
            PivotItemEnter.Stop();
            PivotItemExit.Stop();
            PivotItemHeaderEnter.Stop();
            PivotItemHeaderExit.Stop();
        }
    };
    
}
