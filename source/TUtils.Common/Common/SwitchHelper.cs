using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUtils.Common.Common
{
    using System;
    using System.Collections.Generic;

    public class VariableSwitch<T>
    {
        private readonly T _value;
        private bool _matched;

        public VariableSwitch(T value)
        {
            _value = value;
        }

        public VariableSwitch<T> Case(T compareValue, Action action)
        {
            if (!_matched && EqualityComparer<T>.Default.Equals(_value, compareValue))
            {
                action?.Invoke();
                _matched = true;
            }
            return this;
        }

        public void Default(Action action)
        {
            if (!_matched)
                action?.Invoke();
        }
    }

    public static class SwitchHelper
    {
        public static VariableSwitch<T> SwitchOn<T>(T value)
        {
            return new VariableSwitch<T>(value);
        }
    }
}
