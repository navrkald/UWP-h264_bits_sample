using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.Media.MediaProperties;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using FFmpegInterop;
using Windows.Media.Core;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture m_mediaCapture;
        private InMemoryRandomAccessStream m_mediaStream;
        private FFmpegInteropMSS m_FFmpegMSS;

        public MainPage()
        {
            this.InitializeComponent();

            m_mediaCapture = new MediaCapture();
            m_mediaStream = new InMemoryRandomAccessStream();
            
            //
        }

        

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            await m_mediaCapture.InitializeAsync();
            MediaEncodingProfile encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga);
            //encodingProfile.Video.Subtype = "H264ES";
            //m_mediaCapture.SetEncoderProperty(MediaStreamType.VideoRecord, IMFAttributes. System.Guid.Parse("H264"), );

            //MediaEncodingSubtypes.H264Es
            //m_mediaCapture.En
            //m_mediaCapture.GetEncoderProperty();
            encodingProfile.Audio = null;
            //await m_mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoRecord, IMediaEncodingProperties.);
            await m_mediaCapture.StartRecordToStreamAsync(encodingProfile, m_mediaStream);
            System.Diagnostics.Debug.WriteLine(m_mediaCapture.ToString());
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            await m_mediaCapture.StopRecordAsync();
            m_mediaStream.Seek(0);
            byte[] bytes = new byte[m_mediaStream.Size];
            await m_mediaStream.AsStream().ReadAsync(bytes, 0, bytes.Length);
            string outStr = BitConverter.ToString(bytes);
            System.Diagnostics.Debug.WriteLine(outStr);
            SaveToFile();
        }

        private async void SaveToFile()
        {
            FileSavePicker videoSavePicker = new FileSavePicker();
            videoSavePicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            videoSavePicker.FileTypeChoices.Add(new KeyValuePair<string, IList<string>>("Video", new List<string>() { ".mp4" }));
            var mediaFile = await videoSavePicker.PickSaveFileAsync();
            m_mediaStream.Seek(0);
            byte[] result = new byte[m_mediaStream.Size];
            //await _mediaStream.ReadAsync(result, _mediaStream.Size, 0);
            await SaveStreamToFileAsync(m_mediaStream, mediaFile);
        }

        private static async Task SaveStreamToFileAsync(IRandomAccessStream stream, StorageFile mediaFile)
        {
            if (mediaFile != null)
            {
                using (var dataReader = new DataReader(stream.GetInputStreamAt(0)))
                {
                    await dataReader.LoadAsync((uint)stream.Size);
                    byte[] buffer = new byte[(int)stream.Size];
                    dataReader.ReadBytes(buffer);
                    await FileIO.WriteBytesAsync(mediaFile, buffer);
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            m_FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(m_mediaStream, false, false);
            MediaStreamSource mss = m_FFmpegMSS.GetMediaStreamSource();
            mediaElement1.SetMediaStreamSource(mss);

        }
    }
}
