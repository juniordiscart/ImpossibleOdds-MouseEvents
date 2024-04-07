using UnityEngine;

namespace ImpossibleOdds.MouseEvents
{
    internal struct Event
    {
        public MouseButton button;
        public EventType type;
        public EventModifiers modifiers;
        public Vector2 mousePosition;

        public bool alt
        {
            get => (modifiers & EventModifiers.Alt) == EventModifiers.Alt;
            set => modifiers = value ? (modifiers | EventModifiers.Alt) : (modifiers & ~EventModifiers.Alt);
        }
        public bool shift
        {
            get => (modifiers & EventModifiers.Shift) == EventModifiers.Shift;
            set => modifiers = value ? (modifiers | EventModifiers.Shift) : (modifiers & ~EventModifiers.Shift);
        }
        public bool control
        {
            get => (modifiers & EventModifiers.Control) == EventModifiers.Control;
            set => modifiers = value ? (modifiers | EventModifiers.Control) : (modifiers & ~EventModifiers.Control);
        }
        public bool command
        {
            get => (modifiers & EventModifiers.Command) == EventModifiers.Command;
            set => modifiers = value ? (modifiers | EventModifiers.Command) : (modifiers & ~EventModifiers.Command);
        }
    }
}
