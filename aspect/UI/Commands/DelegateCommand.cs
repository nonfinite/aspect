using System;
using System.Windows.Input;

namespace Aspect.UI.Commands
{
    public sealed class DelegateCommand : ICommand
    {
        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            mExecute = execute;
            mCanExecute = canExecute;
        }

        private readonly Func<bool> mCanExecute;
        private readonly Action mExecute;

        bool ICommand.CanExecute(object parameter) => CanExecute();

        public event EventHandler CanExecuteChanged;

        void ICommand.Execute(object parameter) => Execute();

        public bool CanExecute() => mCanExecute?.Invoke() ?? true;

        public void Execute() => mExecute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public sealed class DelegateCommand<TParam> : ICommand
    {
        public DelegateCommand(Action<TParam> execute, Func<TParam, bool> canExecute = null)
        {
            mExecute = execute;
            mCanExecute = canExecute;
        }

        private readonly Func<TParam, bool> mCanExecute;
        private readonly Action<TParam> mExecute;

        bool ICommand.CanExecute(object parameter)
        {
            if (mCanExecute == null)
            {
                return true;
            }

            if (parameter is TParam value)
            {
                return CanExecute(value);
            }

            return false;
        }

        public event EventHandler CanExecuteChanged;

        void ICommand.Execute(object parameter)
        {
            if (parameter is TParam value)
            {
                Execute(value);
            }
        }

        public bool CanExecute(TParam parameter) => mCanExecute?.Invoke(parameter) ?? true;

        public void Execute(TParam parameter) => mExecute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
