using Prism.Mvvm;

namespace Skeleton.Xamarin.Prism.Core.Mvvm
{
    public class ViewModelBase : BindableBase
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}
