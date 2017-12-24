using MStopwatch.FSharp;
using Prism.Interactivity.InteractionRequest;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace MStopwatch.ViewModels
{
    class MainPageViewModel : INavigationAware
    {

        public ReadOnlyReactiveProperty<string> NowSpan { get; }
        public ReadOnlyReactiveProperty<string> StartButtonLabel { get; }
        public ReactiveCommand StartCommand { get; }
        public ReactiveCommand LapCommand { get; }
        public ReactiveProperty<bool> IsShowed { get; } = new ReactiveProperty<bool>();
        public ReadOnlyReactiveCollection<LapTimeViewModel> Items { get; }

        private Stopwatch Model { get; }

        public InteractionRequest<IConfirmation> ConfirmationRequest { get; set; }


        IRegionManager RegionManager;

        public MainPageViewModel(Stopwatch model, IRegionManager regionManager)
        {
            this.RegionManager = regionManager;
            this.Model = model;
            ConfirmationRequest = new InteractionRequest<IConfirmation>();

            this.StartButtonLabel = this.Model.ObserveProperty(x => x.Mode)
                .Select(x =>
                {
                    switch (x)
                    {
                        case StopwatchMode.Init:
                            return "Start";
                        case StopwatchMode.Start:
                            return "Stop";
                        case StopwatchMode.Stop:
                            return "Reset";
                        default:
                            throw new InvalidOperationException();
                    }
                }).ToReadOnlyReactiveProperty();

            this.StartCommand = new ReactiveCommand();
            this.StartCommand.Subscribe(_ =>
            {
                switch (this.Model.Mode)
                {
                    case StopwatchMode.Init:
                        this.Model.Start();
                        break;
                    case StopwatchMode.Start:
                        this.Model.Stop();

                        ConfirmationRequest.Raise(new Confirmation
                        {
                            Title = "Confirmation",
                            Content = $"All time: {this.Model.NowSpan.ToString(Constants.TimeSpanFormat)}\r\nMax laptime: { this.Model.MaxLapTime.TotalMilliseconds} ms\nMin laptime: { this.Model.MinLapTime.TotalMilliseconds}ms\n\nShow all lap result?"
                        }, r =>
                        {
                            if (r.Confirmed)
                            {
                                RegionManager.RequestNavigate("ContentRegion", "ResultPageView");
                            }
                        });
                        break;
                    case StopwatchMode.Stop:
                        this.Model.Reset();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            });

            this.NowSpan =
            this.Model.ObserveProperty(x => x.NowSpan).ToUnit()
            .Merge(this.IsShowed.ToUnit())
            .Select(_ => this.Model.NowSpan)
            .Select(x => x.ToString(this.IsShowed.Value ? Constants.TimeSpanFormat : Constants.TimeSpanFormatNoMillsecond))
            .ToReadOnlyReactiveProperty();

            this.LapCommand = this.Model
                .ObserveProperty(x => x.Mode)
                .Select(x => x == StopwatchMode.Start)
                .ToReactiveCommand();
            this.LapCommand.Subscribe(_ => this.Model.Lap());

            this.Items = this.Model
                .Items
                .ToReadOnlyReactiveCollection(x => new LapTimeViewModel(x));
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

        void RaiseConfirmation(string content)
        {

        }
    }
}
