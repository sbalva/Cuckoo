using Cuckoo.Options;
using Cuckoo.Speaker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Cuckoo.Model
{
    public class CuckooModel: INotifyPropertyChanged
    {
        #region Propertychanged

        protected void OnPropertyChanged<T>(Expression<Func<T>> action)
        {
            var propertyName = GetPropertyName(action);
            OnPropertyChanged(propertyName);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> action)
        {
            var expression = (MemberExpression)action.Body;
            var propertyName = expression.Member.Name;
            return propertyName;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Fields
        private List<SpeakPattern> allSpeakPatterns = new List<SpeakPattern>() { SpeakPattern.HHMMSS, SpeakPattern.HHMM, SpeakPattern.MMSS };
        private TimeSpan startTime = TimeSpan.Zero;
        private TimeSpan currentTime = TimeSpan.Zero;
        private object currentTimeLocker = new object();
        private CancellationTokenSource timerToken = new CancellationTokenSource();
        private string soundSource = "Cuckoo.Sounds.";
        private SoundPlayer? soundPlayer;
        private SpeakerModel? speakerModel = null;
        #endregion

        #region Properties
        private TimerTypes timerType = TimerTypes.Clock;

        public TimerTypes TimerType
        {
            get { return timerType; }
            set
            {
                timerType = value;
                OnPropertyChanged(() => TimerType);
            }
        }

        public IEnumerable<TimerTypes> AllTimerTypes
        {
            get { return Enum.GetValues(typeof(TimerTypes)).Cast<TimerTypes>(); }
        }

        private SpeakLanguages speakLanguage = SpeakLanguages.EN;

        public SpeakLanguages SpeakLanguage
        {
            get { return speakLanguage; }
            set
            {
                speakLanguage = value;
                OnPropertyChanged(() => SpeakLanguage);
                OnPropertyChanged(() => ShowInterval);
                OnPropertyChanged(() => ShowPattern);                
            }
        }

        public IEnumerable<SpeakLanguages> AllLanguages
        {
            get { return Enum.GetValues(typeof(SpeakLanguages)).Cast<SpeakLanguages>(); }
        }

        private BeepSounds beep = BeepSounds.Beep;

        public BeepSounds Beep
        {
            get { return beep; }
            set
            {
                beep = value;
                OnPropertyChanged(() => Beep);
                OnPropertyChanged(() => ShowInterval);                
            }
        }

        public string BeepSource
        {
            get
            {
                switch (Beep)
                {
                    case BeepSounds.Beep:
                        return $"{soundSource}beep.wav";
                    case BeepSounds.Synthetic:
                        return $"{soundSource}sinbeep.wav";
                    default:
                        return String.Empty;
                }
            }
        }

        public string LanguageSource { get { return $"{soundSource}{SpeakLanguage}"; } }
        public string LanguageSchemaSource { get { return $"{LanguageSource}.schema.json"; } }


        public IEnumerable<BeepSounds> AllBeeps
        {
            get { return Enum.GetValues(typeof(BeepSounds)).Cast<BeepSounds>(); }
        }

        public List<SpeakPattern> AllSpeakPatterns
        {
            get { return allSpeakPatterns; }
        }

        private SpeakPattern pattern;

        public SpeakPattern Pattern
        {
            get { return pattern; }
            set
            {
                pattern = value;
                OnPropertyChanged(() => Pattern);
            }
        }


        public IEnumerable<int> Intervals
        {
            get { return new List<int>() {10, 20, 30, 60 }; }
        }

        private int interval = 20;

        public int Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                OnPropertyChanged(() => Interval);
            }
        }

        private bool ontop = false;

        public bool OnTop
        {
            get { return ontop; }
            set
            {
                ontop = value;
                OnPropertyChanged(() => OnTop);
                OnPropertyChanged(() => OnTopButtonColor);
            }
        }

        private string timeText = String.Empty;

        public string TimeText
        {
            get { return timeText; }
            set
            {
                timeText = value;
                OnPropertyChanged(() => TimeText);
            }
        }

        private bool isStarted = false;

        public bool IsStarted
        {
            get { return isStarted; }
            set
            {
                isStarted = value;
                OnPropertyChanged(() => IsStarted);
                OnPropertyChanged(() => ActionText);
                OnPropertyChanged(() => OptionsEnabled);
                OnPropertyChanged(() => ShowTime);
                OnPropertyChanged(() => MinHeight);
            }
        }

        public Visibility ShowTime { get { return (IsStarted ? Visibility.Visible : Visibility.Collapsed); } }
        public Visibility ShowPattern { get { return (SpeakLanguage != SpeakLanguages.NONE) ? Visibility.Visible : Visibility.Collapsed; } }
        public double MinHeight { get { return (IsStarted ? 200 : 80); } }
        public SolidColorBrush OnTopButtonColor { get { return new SolidColorBrush((OnTop) ? Colors.Red : SystemColors.ControlLightColor); } }
        public Visibility ShowInterval { get { return (Beep != BeepSounds.NoBeep || SpeakLanguage != SpeakLanguages.NONE) ? Visibility.Visible : Visibility.Hidden; } }
        public bool OptionsEnabled { get { return !IsStarted; } }
        public string ActionText { get { return (IsStarted) ? "Stop" : "Start"; } }
        #endregion

        #region Commands
        public ICommand ActionCommand { get { return new BaseCommand(action); } }
        public ICommand ChangeOnTopCommand { get { return new BaseCommand(changeOnTop); } }
        #endregion


        public CuckooModel()  
        {
            StartOptions options = OptionsLoader.LoadStartOptions();
            TimerType = options.TimerType;
            SpeakLanguage = options.SpeakLanguage;
            Beep = options.Beep;
            Interval = options.Interval;
            Pattern = allSpeakPatterns.FirstOrDefault(x => x.Display == options.Patterrn);
            OnTop = options.OnTop;
        }

        #region Methods
        private void action()
        {
            bool actionres = false;
            if (!IsStarted)
                actionres = actionStart();
            else
                actionres = actionStop();
            if (actionres)
                IsStarted = !IsStarted;
        }

        private void changeOnTop()
        {
            OnTop = !OnTop;
        }

        private bool actionStart()
        {            
            speakerModel = (SpeakLanguage != SpeakLanguages.NONE) ? SpeakerModel.Load(LanguageSource, LanguageSchemaSource) : null;
            if (SpeakLanguage == SpeakLanguages.NONE || speakerModel != null)
            {
                startTime = (TimerType == TimerTypes.Clock) ? TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks) : TimeSpan.Zero;
                soundPlayer = new SoundPlayer();
                timerToken = new CancellationTokenSource();

                Task.Factory.StartNew((object? obj) => { tickTimer(); }, null, timerToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                Task.Factory.StartNew((object? obj) => { speakTimer(); }, null, timerToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                OptionsLoader.SaveStartOptions(new StartOptions(TimerType, SpeakLanguage, Beep, Interval, Pattern.Display, OnTop));
                return true;
            }
            else
                MessageBox.Show($"Error loading schema.json for language {SpeakLanguage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        private bool actionStop()
        {            
            if (timerToken != null && timerToken.Token.CanBeCanceled)
            { 
                timerToken.Cancel();
                speakerModel?.Dispose();
                soundPlayer?.Dispose();
                return true;
            }
            else
                return false;
        }

        private void tickTimer()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            currentTime = startTime;
            TimeSpan timerLimit = TimeSpan.FromHours(24);
            while (!timerToken.Token.IsCancellationRequested)
            {
                lock (currentTimeLocker)
                {
                    if (TimerType == TimerTypes.Timer && currentTime > timerLimit)
                        watch.Restart();
                    currentTime = startTime + watch.Elapsed;
                }
                
                App.Current.Dispatcher.Invoke(() => TimeText = $"{currentTime.Hours:00}:{currentTime.Minutes:00}:{currentTime.Seconds:00}.{currentTime.Milliseconds:000}");
                timerToken.Token.WaitHandle.WaitOne(50);
            }
            watch.Stop();
            App.Current.Dispatcher.Invoke(() =>  TimeText = String.Empty);
        }

        private void speakTimer()
        {
            int h, m, s;
            bool shouldSpeak = false;
            Stream? beepStream = !String.IsNullOrEmpty(BeepSource) ? Assembly.GetExecutingAssembly().GetManifestResourceStream(BeepSource) : null;
            while (!timerToken.Token.IsCancellationRequested)
            {
                lock (currentTimeLocker)
                {
                    shouldSpeak = (currentTime.Seconds == 0 || (currentTime.Seconds % Interval) == 0);
                }
                if (shouldSpeak)
                {
                    lock (currentTimeLocker)
                    {
                        h = currentTime.Hours;
                        m = currentTime.Minutes;
                        s = currentTime.Seconds;
                    }

                    if (soundPlayer != null)
                    {
                        if (Beep != BeepSounds.NoBeep)
                            playStream(beepStream);

                        if (speakerModel != null)
                        {
                            if (Pattern.Hours) playNumberStreams(h);
                            if (Pattern.Minutes) playNumberStreams(m);
                            if (Pattern.Seconds) playNumberStreams(s);
                        }
                        else
                            timerToken.Token.WaitHandle.WaitOne(1000);
                    }
                    else
                        break;
                }
                else
                    timerToken.Token.WaitHandle.WaitOne(50);
            }
        }

        private void playNumberStreams(int number)
        {
            NumberStream? numberStream = speakerModel?.Numbers.FirstOrDefault(x => x.Value == number);
            if (numberStream != null && numberStream.Streams.Count > 0)
                numberStream.Streams.ForEach(x => playStream(x));
        }

        private void playStream(Stream? stream)
        {
            if (stream != null && stream.CanRead)
            {                
                soundPlayer.Stream = stream;
                soundPlayer.PlaySync();
                if (stream.CanSeek)
                    stream?.Seek(0, SeekOrigin.Begin);
            }
        }       
        #endregion
    }
}
