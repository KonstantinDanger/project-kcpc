using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectKCPC.Scripts.UI
{
    public class GameUI : UI
    {
        private readonly List<UI> _ui = new();
        private readonly Stack<UIView> _viewStack = new();

        private Action OnViewOpen;
        private Action OnAllViewsClose;

        public bool HasStackedUIViews => _viewStack.Count > 0;

        public GameUI Initialize(Action onViewOpen, Action onAllViewsClose)
        {
            OnViewOpen = onViewOpen;
            OnAllViewsClose = onAllViewsClose;

            return this;
        }

        public GameUI Add(UI ui)
        {
            if (_ui.Contains(ui))
                return this;

            _ui.Add(ui);

            if (ui is UIView view)
                view.Initialize(this);

            return this;
        }

        public void OpenViewOfType(Type viewType)
        {
            if (!typeof(UIView).IsAssignableFrom(viewType))
                return;

            UIView selected = null;

            foreach (UI ui in _ui)
            {
                if (ui is UIView view && view.GetType() == viewType)
                {
                    selected = view;
                    break;
                }
            }

            if (!selected || _viewStack.Contains(selected))
                return;

            _viewStack.Push(selected);
            _viewStack.Peek().Open();

            OnViewOpen?.Invoke();
        }

        /// <summary>
        /// Closes last (peek) element of the stack
        /// </summary>
        public void CloseView()
        {
            if (_viewStack.Count == 0)
                return;

            _viewStack.Pop().Close();

            if (_viewStack.Count == 0)
                OnAllViewsClose?.Invoke();
        }
    }
}
