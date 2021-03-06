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
        public static LocString SETTINGS_TOOLTIP = (LocString)"Configure ZTransport connection settings for this world.";
        public static class STATUSITEMS {
            public static class ZCOORDINATES {
                public static LocString NAME = (LocString)"Coordinates: {X}, {Y}";
                public static LocString TOOLTIP = (LocString)"This can connect with ZTransporters on other Z-levels at coordinates {X}, {Y}";
            }
            public static class ZNOTCONNECTED {
                public static LocString NAME = (LocString)"ZTransport: Connection Error";
                public static LocString TOOLTIP = (LocString)"{REASON}";
            }

            public static class ZDROPBOXSTATUS {
                public static LocString NAME = (LocString)"Sending objects: {COUNT}";
                public static LocString TOOLTIP = (LocString)"Sending objects to the server. More objects cannot be sent until all previous objects are sent.";
            }
        }
        public static class NETWORK {
            public static LocString GENERATE_MISSING_PASSED_UNKNOWN = (LocString)"GENERATE MISSING: WAS PASSED AN UNKNOWN MESSAGE TYPE";
            public static LocString DEBUG_UNEXPECTED_TILE = (LocString)"Z-Transport: WARNING: Got a message for a tile when we weren't expecting one for that tile?";
            public static LocString DEBUG_COOKIE_MISMATCH = (LocString)"Z-Transport: WARNING: Got an extra message but filtered it out because the cookie didn't match. (Harmless.)";
            public static LocString UNABLE_TO_CONNECT = (LocString)"Z-Transport: Unable to connect: {REASON}";
            public static LocString RETRY_CONNECTION = (LocString)"Z-Transport: Trying again in about 5 seconds.";
            public static LocString BAD_HANDSHAKE = (LocString)"Bad handshake message";
            public static LocString CORRUPTED_MESSAGE = (LocString)"Server sent a corrupted message";
            public static LocString CONNECTION_ERROR = (LocString)"Z-Transport: Connection error: {ERROR}";
            public static LocString SERVER_CLOSED_CONNECTION = (LocString)"Server closed connection";
            public static LocString SERVER_CONNECTION_INTERRUPTED = (LocString)"Server connection interrupted";
            public static LocString PROTOCOL_ERROR = (LocString)"Protocol Error";
            public static LocString NOT_ZTRANSPORT_SERVER = (LocString)"The requested server is not a ZTransport server";
            public static LocString UPDATE_CLIENT = (LocString)"You need to update ZTransport.";
            public static LocString UPDATE_SERVER = (LocString)"The server you are trying to connect too needs to be updated.";
        }
        public static class UI {
            public static LocString ASDFASDFASDF = (LocString)"--TEXT--";
            public static LocString OK_TOOLTIP = (LocString)"Apply changes immediately.";
            public static LocString SERVER_ADDRESS = (LocString)"Server Address";
            public static LocString ADDRESS_TOOLTIP = (LocString)"Address of onizd server. Leave empty to disable ZTransport on this world.";
            public static LocString SERVER_PORT = (LocString)"Server Port";
            public static LocString PORT_TOOLTIP = (LocString)"Port number of onizd server. Leave empty to use the default port.";
            public static LocString PING_INTERVAL = (LocString)"Ping Interval";
            public static LocString PING_TOOLTIP = (LocString)"How often to ping the server. Useful when dealing with routers that like to drop connections. Leave empty or set to 0 to disable pinging.";
        }
        public static class DROP_BOX {
            public static LocString SEND = (LocString)"Send Contents";
            public static LocString TOOLTIP = (LocString)"Transfer contents to the server one by one";
        }
    }
}

namespace ZTransport
{
    public static class Z
    {
        public const ushort DEFAULT_PORT = 5496;
        public static ushort port = DEFAULT_PORT;

        public const int DEFAULT_PING_INTERVAL = 20;
        public static volatile int ping_interval = DEFAULT_PING_INTERVAL;

        public static string address;
        public static StatusItem coordinates;

        public static StatusItemCategory serverStatus;

        public static StatusItemCategory dropBoxStatus;

        public static StatusItem notConnected;

        public static StatusItem dropBoxSending;

        public static Network net;
    }
}
