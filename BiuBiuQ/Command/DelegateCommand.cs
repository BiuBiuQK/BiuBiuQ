using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiuBiuQ.Command
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                if(CanExcuteFunc != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if(CanExcuteFunc != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object? parameter)
        {
            if(CanExcuteFunc == null)
            {
                return true;
            }
            return CanExcuteFunc(parameter);
        }

        public void Execute(object? parameter)
        {
            if(ExcuteAction == null)
            {
                return;
            }
            ExcuteAction(parameter);
        }

        public Action<object> ExcuteAction { get; set; }
        public Func<object, bool> CanExcuteFunc { get; set; }
    }
}
