using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace clasificadorva.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "Nosotros";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://docs.microsoft.com/en-us/azure/cognitive-services/Custom-Vision-Service/"));
        }

        public ICommand OpenWebCommand { get; }
    }
}