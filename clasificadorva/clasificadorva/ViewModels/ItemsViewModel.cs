using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using clasificadorva.Models;
using clasificadorva.Views;
using Plugin.Media.Abstractions;
using Plugin.Media;

namespace clasificadorva.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public const string ServiceApiUrl = "PON_TU_SERVICE_API_AQUI";//"https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/701b4eed-c0b1-4f4d-86af-873a7b4ad6a3/image";
        public const string ApiKey = "PON_TU_API_KEY_AQUI";

        private MediaFile _foto = null;
        private Item _selectedItem;
        private string pathImg;
        
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

            var _pfoto = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Vision",
                Name = "Target.jpg"
            });

            if (_foto == null) return;

            PathImg = _pfoto.Path;
        }

        
    }
}