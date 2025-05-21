
using System;

namespace Scripts.BaseSystems.Core
{
    public interface IFlagHolder
    {
        public event Action<bool> OnNewValueAssigned; 
        public bool Value { get; set; }
    }
}
