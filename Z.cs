/*
 * This file is part of ZTransport, copyright ©2020 BloodyRum.
 *
 * ZTransport is free software: you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free Soft-
 * ware Foundation, either version 3 of the License, or (at your option) any
 * later version.
 *
 * ZTransport is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FIT-
 * NESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
 * details.
 *
 * You should have received a copy of the GNU General Public License along with
 * ZTransport. If not, see <https://www.gnu.org/licenses/>.
 */

namespace STRINGS {
    public static class ZTRANSPORT {
        public static LocString SETTINGS_BUTTON = (LocString)"ZTransport";
        public static LocString SETTINGS_TOOLTIP = (LocString)"Configure ZTransport connection settings for this world. (NOT IMPLEMENTED YET)";
        public static class STATUSITEMS {
            public static class ZCOORDINATES {
                public static LocString NAME = (LocString)"Coordinates: {X}, {Y}";
                public static LocString TOOLTIP = (LocString)"This can connect with ZTransporters on other Z-levels at coordinates {X}, {Y}";
            }
            public static class ZNOTCONNECTED {
                public static LocString NAME = (LocString)"ZTransport: Connection Error";
                public static LocString TOOLTIP = (LocString)"{REASON}";
            }
        }
    }
}

namespace ZTransport
{
    public static class Z
    {
        public const ushort DEFAULT_PORT = 5496;
        public static string address;
        public static ushort port = DEFAULT_PORT;
        public static StatusItem coordinates;

        public static StatusItemCategory serverStatus;

        public static StatusItem notConnected;

        public static Network net;
    }
}
