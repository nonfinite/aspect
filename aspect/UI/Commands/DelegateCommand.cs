using System;
using System.Windows.Input;

namespace Aspect.UI.Commands
{
    public sealed class DelegateCommand<TParam> : ICommand
    {
        public DelegateCommand(Action<TParam> execute, Func<TParam, bool> canExecute = null)
        {
            mExecute = execute;
            mCanExecute = canExecute;
        }

        private readonly Func<TParam, bool> mCanExecute;
        private readonly Action<TParam> mExecute;

        public bool CanExecute(object parameter)
        {
            if (mCanExecute == null)
            {
                return true;
            }

            if (parameter is TParam value)
            {
                return mCanExecute(value);
            }

            return false;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter is TParam value)
            {
                mExecute(value);
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
