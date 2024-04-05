/*
 * AltInput: Alternate input plugin for Kerbal Space Program
 * Copyright © 2014 Pete Batard <pete@akeo.ie>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

// IMPORTANT: To be able to work with Unity, which is *BROKEN*,
// you must have a patched version of SharpDX.DirectInput such
// as the one provided with this project (in Libraries/)
// See: https://github.com/sharpdx/SharpDX/issues/406
using SharpDX.DirectInput;

namespace AltInput
{
    public enum MappingType
    {
        Range,
        Absolute,
        Delta,
    }

    public enum ControlType
    {
        Axis,
        OneShot,
        Continuous,
    }

    /// <summary>
    /// The range of a axis
    /// </summary>
    public struct AltRange
    {
        public int Minimum;
        public int Maximum;
        /// <summary>The range expressed as a floating point value to speed up computation</summary>
        public float FloatRange;
    }

    public struct AltMapping
    {
        /// <summary>Type of the mapping</summary>
        public MappingType Type;
        /// <summary>Name of the KSP game action this control should map to</summary>
        public String Action;
        /// <summary>Value the action should take, in case of an axis</summary>
        public float Value;
    }

    public struct AltControl
    {
        public ControlType Type;
        public Boolean Inverted;
        /// <summary>Dead zone, in the range 0.0 through 1.0, where 0.0 indicates that there
        /// is no dead zone, 0.5 indicates that the dead zone extends over 50 percent of the
        /// physical range of the axis and 1.0 indicates that the entire physical range of
        /// the axis is dead. For regular axes, the dead zone is applied to the center. For
        /// sliders, to the edges.</summary>
        public float DeadZone;
        /// <summary>Factor by which to multiply this input</summary>
        public float Factor;
    }

    /// <summary>
    /// An axis on the input device
    /// </summary>
    public struct AltAxis
    {
        /// <summary>Whether this axis is available on this controller</summary>
        public Boolean isAvailable;
        /// <summary>The range of this axis</summary>
        public AltRange Range;
        /// <summary>Last recorded value (to detect transitions)</summary>
        public float LastValue;
        /// <summary>The control determines how we should handle the axis</summary>
        public AltControl[] Control;
        // TODO: move Mapping inside the control struct?
        public AltMapping[] Mapping1;   // Regular mapping or Min mapping for thresholded values
        public AltMapping[] Mapping2;   // Max mapping for thresholded values
    }

    /// <summary>
    /// A button on the input device
    /// </summary>
    public struct AltButton
    {
        public int LastValue;
        public Boolean[] Continuous;
        public AltMapping[] Mapping;
    }

    /// <summary>
    /// A Point Of View control on the input device
    /// </summary>
    public struct AltPOV
    {
        public int LastValue;
        // We consider that a POV is a set of 4 buttons
        public AltButton[] Button;  // Gotta wonder what the heck is wrong with these "high level" languages
        // when you can't do something as elementary as declaring a BLOODY FIXED SIZE ARRAY IN A STRUCT...
    }

    /// <summary>
    /// A Direct Input Device (typically a game controller)
    /// </summary>
    public class AltDirectInputDevice : AltDevice
    {
        /// <summary>POV positions</summary>
        public enum POVPosition
        {
            Up = 0,
            Right,
            Down,
            Left
        };
        public readonly static String[] POVPositionName = Enum.GetNames(typeof(POVPosition));
        public readonly static int NumPOVPositions = POVPositionName.Length;

        /// <summary>Names for the axes. Using a double string array allows to map not so
        /// user-friendly DirectInput names to more user-friendly config counterparts.</summary>
        public readonly static String[,] AxisList = new String[,] {
            { "X", "AxisX" }, { "Y", "AxisY" }, { "Z", "AxisZ" },
            { "RotationX", "RotationX" }, { "RotationY", "RotationY" }, { "RotationZ", "RotationZ" },
            { "Sliders0", "Slider1" }, { "Sliders1", "Slider2" } };
        public DeviceClass Class;
        public Guid InstanceGuid;
        /// <summary>Default dead zone, in the range 0.0 through 1.0, where 0.0 indicates that
        /// there is no dead zone, 0.5 indicates that the dead zone extends over 50 percent of
        /// the physical range of the axis and 1.0 indicates that the entire physical range of
        /// the axis is dead. For regular axes, the dead zone is applied to the center. For
        /// sliders, to the edges.</summary>
        public float DeadZone;
        /// <summary>Default sensitivity of the device</summary>
        public float Factor = 1.0f;
        public AltAxis[] Axis;
        public AltPOV[] Pov;
        public AltButton[] Button;
        public Joystick Joystick;
        public JoystickState State;

        public AltDirectInputDevice(DirectInput directInput, DeviceClass deviceClass, Guid instanceGUID)
        {
            if (deviceClass != DeviceClass.GameControl)
                throw new ArgumentException("Class must be 'GameControl'");
            this.InstanceGuid = instanceGUID;
            this.Joystick = new Joystick(directInput, instanceGUID);
        }

        public override JoystickState ProcessInput()
        {
            Joystick.Poll();
            State = Joystick.GetCurrentState();
            return State;
        }

        public bool HasState() {
            return State != null;
        }

        public float GetX() {
            return (float)State.X / ushort.MaxValue;
        }

        public float GetY() {
            return (float)State.Y / ushort.MaxValue;
        }

        public float GetZ() {
            return (float)State.Z / ushort.MaxValue;
        }

        public float GetRX() {
            return (float)State.RotationX / ushort.MaxValue;
        }

        public float GetRY() {
            return (float)State.RotationY / ushort.MaxValue;
        }

        public float GetRZ() {
            return (float)State.RotationZ / ushort.MaxValue;
        }

        public bool[] GetButtons() {
            return State.Buttons;
        }

        public override void OpenDevice()
        {
            // Device may have been already acquired - release it
            Joystick.Unacquire();
            AltInputBehaviour.Log.LogInfo("AltInput: Using Controller '" +
                Joystick.Information.InstanceName + "'");
            // Set BufferSize in order to use buffered data.
            Joystick.Properties.BufferSize = 128;
            Joystick.Acquire();
        }

        public override void CloseDevice()
        {
            Joystick.Unacquire();
        }

        /// <summary>
        /// Resets all buttons and axes. This is used when switching game modes
        /// </summary>
        public override void ResetDevice()
        {
        }
    }
}
