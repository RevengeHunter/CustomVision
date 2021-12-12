using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using clasificadorva.Models;
using clasificadorva.Views;
using Plugin.Media.Abstractions;
using Plugin.Media;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using Acr.UserDialogs;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System.IO;
using RestSharp;
using System.Net.Http.Headers;

namespace clasificadorva.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public const string ServiceApiUrl = "https://frutas0001-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/805d7473-fe24-468f-aaba-df4ceea587cc/classify/iterations/fruit_classify_0021/image";
        public const string ApiKey = "08112512dd8f46a18907bf1865373daf";//"PON_TU_API_KEY_AQUI";

        private MediaFile _foto = null;
        private Item _selectedItem;
        private string pathImg;
        private string answerText;
        private float progressValue;

        //private CustomVisionPredictionClient predictionApi = AuthenticatePrediction(predictionEndpoint, predictionKey);

        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<Item> ItemTapped { get; }
        public Command SelectImagenCommand { get; }
        public Command TakePictureCommand { get; }
        public Command ClassifyCommand { get; }

        public ItemsViewModel()
        {
            Title = "Clasificar";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            ItemTapped = new Command<Item>(OnItemSelected);
            AddItemCommand = new Command(OnAddItem);
            SelectImagenCommand = new Command(SelectImagen);
            TakePictureCommand = new Command(TakePicture);
            ClassifyCommand = new Command(Classify);
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }

        public string PathImg
        {
            get => pathImg;
            set => SetProperty(ref pathImg, value);
        }

        public string AnswerText
        {
            get => answerText;
            set => SetProperty(ref answerText, value);
        }

        public float ProgressValue
        {
            get => progressValue;
            set => SetProperty(ref progressValue, value);
        }

        private async void SelectImagen(object obj)
        {
            await CrossMedia.Current.Initialize();
            _foto = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions());
            PathImg = _foto.Path;
        }


        private async void TakePicture(object obj)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                return;
            }

            _foto = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = true,
                Directory = "Pictures",
                Name = "Target.jpg"
            });

            if (_foto == null) return;

            PathImg = _foto.Path;
        }

        private static byte[] GetImageAsByteArray(Stream blob)
        {
            var binaryReader = new BinaryReader(blob);
            return binaryReader.ReadBytes((int)blob.Length);
        }

        private async void Classify(object obj)
        {
            using (UserDialogs.Instance.Loading("Clasificando..."))
            {
                if (_foto == null) return;

                var stream = _foto.GetStream();
                var byteData = GetImageAsByteArray(stream);

                var httpClient = new HttpClient();
                var url = ServiceApiUrl;
                httpClient.DefaultRequestHeaders.Add("Prediction-Key", ApiKey);

                //var content = new StreamContent(stream);
                var content = new ByteArrayContent(byteData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    UserDialogs.Instance.Toast("Hubo un error en la deteccion...");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();

                var c = JsonConvert.DeserializeObject<ClasificationResponse>(json);

                var p = c.Predictions.FirstOrDefault();
                if (p == null)
                {
                    UserDialogs.Instance.Toast("Image no reconocida.");
                    return;
                }
                AnswerText = $"Es {p.tagName} - estoy seguro en un {p.Probability:p0}";
                ProgressValue = p.Probability;
            }

            UserDialogs.Instance.Toast("Clasificacion terminada...");
        }


    }

    public class ClasificationResponse
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public DateTime Created { get; set; }
        public Prediction[] Predictions { get; set; }
    }

    public class Prediction
    {
        public string TagId { get; set; }
        public string tagName { get; set; }
        public float Probability { get; set; }
    }
}