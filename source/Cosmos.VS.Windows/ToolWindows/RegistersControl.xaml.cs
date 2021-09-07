using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Cosmos.VS.Windows
{
    public partial class RegistersControl : DebuggerUC
    {
        private RegistersViewModel mViewModel;

        public RegistersControl()
        {
            InitializeComponent();
            DataContext = mViewModel = new RegistersViewModel();
        }

        protected override void DoUpdate(string aTag)
        {
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    if (mData != null)
                    {
                        mViewModel.UpdateData(mData);
                    }
                }, DispatcherPriority.Normal);
        }
    }

    internal class RegistersViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Push All
        //  Temp = (ESP);
        //  Push(EAX); 28
        //  Push(ECX); 24
        //  Push(EDX); 20
        //  Push(EBX); 16
        //  Push(Temp); 12 // Have to get from another source, ESP is already in DS when we pushall
        //  Push(EBP); 8
        //  Push(ESI); 4
        //  Push(EDI); 0
        // We get them from bottom up, so we receive them in reverse order as shown above. That is 0-3 is EDI.
        //
        // Additional ones sent manually (at end, not from stack)
        // ESP 32
        // EIP 36
        //
        //TODO: Flags
        private byte[] mData;
        private object mSyncObject = new object();

        public void UpdateData(byte[] data)
        {
            lock (mSyncObject)
            {
                mData = data;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(String.Empty));
            }
        }

        public string EAX => GetRegisterValue(28).ToHexString();
        public string AX => EAX.Substring(4, 4);
        public string AH => EAX.Substring(4, 2);
        public string AL => EAX.Substring(6, 2);

        public string EBX => GetRegisterValue(16).ToHexString();
        public string BX => EBX.Substring(4, 4);
        public string BH => EBX.Substring(4, 2);
        public string BL => EBX.Substring(6, 2);

        public string ECX => GetRegisterValue(24).ToHexString();
        public string CX => ECX.Substring(4, 4);
        public string CH => ECX.Substring(4, 2);
        public string CL => ECX.Substring(6, 2);

        public string EDX => GetRegisterValue(20).ToHexString();
        public string DX => EDX.Substring(4, 4);
        public string DH => EDX.Substring(4, 2);
        public string DL => EDX.Substring(6, 2);

        public string ESI => GetRegisterValue(8).ToHexString();
        public string EDI => GetRegisterValue(4).ToHexString();

        public string EBP => GetRegisterValue(0).ToHexString();
        public string ESP => GetRegisterValue(32).ToHexString();

        public string EIP => GetRegisterValue(36).ToHexString();
        public string Flags => ((uint?)null).ToHexString();

        private uint? GetRegisterValue(int offset)
        {
            lock (mSyncObject)
            {
                if (mData == null)
                {
                    return null;
                }

#if DEBUG
                if (offset >= mData.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
#endif

                return BitConverter.ToUInt32(mData, offset);
            }
        }
    }

    internal static class ExtensionMethods
    {
        public static string ToHexString(this uint? value)
        {
            if (value.HasValue)
            {
                return value.Value.ToString("X8");
            }

            return "--------";
        }
    }
}
