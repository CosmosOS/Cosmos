using System.ComponentModel;
using System.Text;

namespace Cosmos.Build.Builder.Models
{
    internal class Section : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }
        public string Log => _logBuilder.ToString();
        public bool HasLoggedErrors { get; private set; }

        private readonly StringBuilder _logBuilder;

        public Section(string name)
        {
            Name = name;
            _logBuilder = new StringBuilder();
        }

        public void LogMessage(string message)
        {
            _logBuilder.AppendLine(message);
            OnPropertyChanged(nameof(Log));
        }

        public void SetError()
        {
            if (!HasLoggedErrors)
            {
                HasLoggedErrors = true;
                OnPropertyChanged(nameof(HasLoggedErrors));
            }
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
