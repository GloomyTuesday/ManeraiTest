using System;
using UnityEngine;

namespace Scripts.BaseSystems.Core
{
    [CreateAssetMenu(fileName = "FlagHolder", menuName = "Scriptable Obj/Base systems/Core/Banks/Flag holder")]
    public class FlagHolderSrc : ScriptableObject, IFlagHolder
    {
        [SerializeField]
        private bool _flag;

        private bool _flagPreviousValue; 

        public bool Value
        {
            get => _flag; 
            
            set
            {
                if (_flagPreviousValue == value) return;

                _flag = value;
                _flagPreviousValue = value;

                OnNewValueAssigned?.Invoke(_flag); 
            }
        }

        public event Action<bool> OnNewValueAssigned;

        private void OnValidate()
        {
            Value = _flag;
        }

    }
}

