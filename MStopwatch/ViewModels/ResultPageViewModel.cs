using MStopwatch.FSharp;
using Prism.Commands;
using Prism.Regions;
using Reactive.Bindings;
using System.Reactive.Disposables;

namespace MStopwatch.ViewModels
{
    class ResultPageViewModel : INavigationAware
    {
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public ReadOnlyReactiveCollection<LapTimeViewModel> LapTimes { get; }

        public DelegateCommand BackCommand { get; }

        readonly IRegionManager _regionManager;

        public ResultPageViewModel(Stopwatch model, IRegionManager regionManager)
        {
            this._regionManager = regionManager;

            this.LapTimes = model.Items
                .ToReadOnlyReactiveCollection(x => new LapTimeViewModel(x));

            BackCommand = new DelegateCommand(Back);
        }

        private void Back()
        {
            _regionManager.RequestNavigate("ContentRegion", "MainPageView");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
