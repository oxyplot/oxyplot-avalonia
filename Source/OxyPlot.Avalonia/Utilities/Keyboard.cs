﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Keyboard.cs" company="OxyPlot">
//   Copyright (c) 2014 OxyPlot contributors
// </copyright>
// <summary>
//   Provides utility methods related to the keyboard.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot.Avalonia
{
    using global::Avalonia.Input;
    using global::Avalonia.Input.Raw;
    using System;

    /// <summary>
    /// Provides utility methods related to the keyboard.
    /// </summary>
    internal class Keyboard
    {
        public static Keyboard Instance { get; } = new Keyboard();

        private RawInputModifiers currentModifiers;

        public Keyboard()
        {
            InputManager.Instance.PreProcess.Subscribe(ProcessModifierKeys);
        }

        /// <summary>
        /// Gets the current modifier keys.
        /// </summary>
        /// <returns>A <see cref="OxyModifierKeys" /> value.</returns>
        public OxyModifierKeys GetModifierKeys()
        {
            var modifiers = OxyModifierKeys.None;
            if ((currentModifiers & RawInputModifiers.Shift) != 0)
            {
                modifiers |= OxyModifierKeys.Shift;
            }

            if ((currentModifiers & RawInputModifiers.Control) != 0)
            {
                modifiers |= OxyModifierKeys.Control;
            }

            if ((currentModifiers & RawInputModifiers.Alt) != 0)
            {
                modifiers |= OxyModifierKeys.Alt;
            }

            if ((currentModifiers & RawInputModifiers.Meta) != 0)
            {
                modifiers |= OxyModifierKeys.Windows;
            }

            return modifiers;
        }

        private void ProcessModifierKeys(RawInputEventArgs args)
        {
            if (args is RawKeyEventArgs keyArgs)
            {
                currentModifiers = keyArgs.Modifiers;
            }
        }
    }
}