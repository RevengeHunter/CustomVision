using System.ComponentModel;
using Xamarin.Forms;
using clasificadorva.ViewModels;

namespace clasificadorva.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}