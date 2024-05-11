using Avalonia.Input;

namespace Bogaculta.Tools
{
    public sealed class DragEventX
    {
        private readonly DragEventArgs? _real;

        private readonly object? _sender;
        private readonly DataObject? _data;
        private DragDropEffects? _effects;

        public DragEventX(DragEventArgs real)
        {
            _real = real;
        }

        public DragEventX(object? sender, DataObject data, DragDropEffects effects)
        {
            _sender = sender;
            _data = data;
            _effects = effects;
        }

        public object? Source => _sender ?? _real?.Source;
        public IDataObject? Data => _data ?? _real?.Data;

        public DragDropEffects DragEffects
        {
            get
            {
                if (_real != null) return _real.DragEffects;
                return _effects.GetValueOrDefault();
            }
            set
            {
                if (_real != null) _real.DragEffects = value;
                else _effects = value;
            }
        }
    }
}