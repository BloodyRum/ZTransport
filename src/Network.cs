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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net.Sockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Ionic.Zlib;

namespace ZTransport
{
    class Point : IEquatable<Point> {
        public int x { get; }
        public int y { get; }

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public static Point extract_from_message(JObject message) {
            // ONI's JSON.NET is really ancient... ugh.
            IDictionary<string, JToken> dict
                = (IDictionary<string, JToken>)message;

            if (dict.ContainsKey("x") && dict.ContainsKey("y")) {
                int x_value = (int)message["x"];
                int y_value = (int)message["y"];
                return new Point(x_value, y_value);
            }
            else return null;
        }

        public bool Equals(Point other) {
            return this.x == other.x && this.y == other.y;
        }

        // witchcraft
        public override int GetHashCode() {
            int y_hash = y.GetHashCode();
            return x.GetHashCode() ^ (y_hash << 7) ^ (y_hash >> 25);
        }
    }

    class ServerDiedException : Exception {
        public ServerDiedException() {}
        public ServerDiedException(string message)
            : base(message) {}
        public ServerDiedException(string message, Exception inner)
            : base(message, inner) {}
    }
    public class Network {
        Stream stream;
        Thread read_thread, write_thread, ping_thread;

        // access to any of the following variables must be protected by a lock
        // on "this":
        TcpClient client;
        string connection_error;
        BlockingCollection<JObject> outgoing_messages
            = new BlockingCollection<JObject>(new ConcurrentQueue<JObject>());
        Dictionary<Point, JObject> last_request_for_tile
            = new Dictionary<Point, JObject>();
        Dictionary<Point, JObject> last_response_for_tile
            = new Dictionary<Point, JObject>();
        Dictionary<Point, List<string>> registered_remote_devices
            = new Dictionary<Point, List<string>>();
        Dictionary<Point, string> registered_local_devices
            = new Dictionary<Point, string>();

        bool connection_active = false;
        bool reconnecting = false;
        // exception to locking rule: outgoing_messages
        // do not need a lock in order to get a message *out of* them
        uint cookie = 0;

        string address;
        ushort port;

        public Network() {
            read_thread = new Thread(new ThreadStart(() => read_thread_function()));
            read_thread.IsBackground = true;
            read_thread.Start();
        }

        public void connect(string address, ushort port) {
            lock(this) {
                if (client != null) {
                    reconnecting = true;
                    client.Close();
                }
                this.address = address;
                this.port = port;
                Monitor.Pulse(this);
            }
        }

        private void write_directly(Stream stream, JObject message) {
            string message_as_string
                = JsonConvert.SerializeObject(message) + "\n";
            Byte[] data
                = System.Text.Encoding.UTF8.GetBytes(message_as_string);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public void write_thread_function() {
            Stream stream = this.stream;
            while(true) {
                JObject next_message_to_send = outgoing_messages.Take();
                write_directly(stream, next_message_to_send);
            }
        }

        public void ping_thread_function() {
            JObject ping = new JObject();
            ping.Add("type", "ping");
            while(true) {
                int ping_interval = Z.ping_interval;
                if (ping_interval > 0) {
                    Thread.Sleep(ping_interval * 1000);
                    // *1000 because sleep is expecting ms, not s
                    send_message(ping);
                } else {
                    Thread.Sleep(5000);
                }
            }
        }

        public JObject get_message_for(string type, int x, int y) {
            Point requested_tile = new Point(x, y);
            JObject response;
            lock(this) {
                if (last_response_for_tile.TryGetValue(requested_tile,
                                                       out response)
                    && response != null && ((string)response["type"]) == type){
                    last_response_for_tile.Remove(requested_tile);
                    return response;
                } else {
                    return null;
                }
            }
        }

        public void send_message(JObject message) {
            Point target_point = Point.extract_from_message(message);
            bool needs_response = target_point != null && (string)message["type"] != "register" && (string)message["type"] != "unregister";

            lock(this) {
                if (connection_active) {
                    if(needs_response) {
                        // We only need a cookie if:
                        // - The message "belongs to" a particular point
                        //   AND is not a registration message
                        // AND
                        // - The message is going to be sent over the network
                        //   (i.e. the server isn't dead)
                        message["cookie"] = cookie++;
                        // doing it this way instead of using .Add will make it
                        // overwrite any existing request
                        last_request_for_tile[target_point] = message;
                        last_response_for_tile.Remove(target_point);
                    }
                    outgoing_messages.Add(message);
                } else {
                    if (needs_response) {
                        last_response_for_tile[target_point] = generate_missing_response(message);
                    }
                }
            }
        }

        private JObject read_directly(ProtoReader sr) {
            string line = sr.ReadLine();
            if(line == null) {
                // FUCK THE WORLD! ALL HELLS GONE LOOSE!
                throw new ServerDiedException("Bye bye");
            }
            //Debug.Log("Received a message from the server. " + line);
            return JObject.Parse(line);
        }

        private JObject copy_coords(JObject to_get_from, JObject to_set) {
            to_set.Add("x", to_get_from["x"]);
            to_set.Add("y", to_get_from["y"]);
            return to_set;
        }

        private JObject generate_missing_response(JObject missing_response) {
            JObject fake_returned = new JObject();
            fake_returned = copy_coords(missing_response, fake_returned);

            switch((string)missing_response["type"]) {
                case "send_joules":
                    // Pretend that NO energy was sucsufully sent,
                    // therefor the return amount equals the sent amount

                    fake_returned.Add("type", "sent_joules");
                    fake_returned.Add("spare", missing_response["joules"]);

                    return fake_returned;

                case "recv_joules":
                    // Pretend that NO energy was recieved
                    // therefor the return amount is 0

                    fake_returned.Add("type", "got_joules");
                    fake_returned.Add("joules", 0);

                    return fake_returned;
                case "send_packet":
                    // Pretend that the server rejects the packet

                    fake_returned.Add("type", "sent_packet");
                    fake_returned.Add("phase", missing_response["phase"]);
                    fake_returned.Add("accepted", false);

                    return fake_returned;
                case "recv_packet":
                    // Pretend that we sent back a null packet

                    fake_returned.Add("type", "got_packet");
                    fake_returned.Add("phase", missing_response["phase"]);
                    fake_returned.Add("packet", null);
                    // I assume here that null works to make the value
                    // recieved from the packet to be null

                    return fake_returned;
                case "send_object":
                    fake_returned.Add("type", "sent_object");
                    fake_returned.Add("accepted", "false");

                    return fake_returned;
                case "recv_object":
                    fake_returned.Add("type", "got_object");
                    fake_returned.Add("object", null);

                    return fake_returned;
                default:
                    throw new Exception(STRINGS.ZTRANSPORT.NETWORK.GENERATE_MISSING_PASSED_UNKNOWN);
            }
        }

        private void register_remote_device(Point point, string device_id) {
            lock(this) {
                List<string> devices;
                bool exists = registered_remote_devices.TryGetValue(point, out devices);

                if (!exists) {
                    devices = new List<string>();
                    registered_remote_devices.Add(point, devices);
                }
                devices.Add(device_id);
            }
        }

        private void unregister_remote_device(Point point, string device_id) {
            lock(this) {
                List<string> devices;
                bool exists = registered_remote_devices.TryGetValue(point, out devices);

                if (!exists) {
                    // Attempted to unregister a device at a point where we
                    // don't know about any registered devices.
                    // Since unregistering a device that doesn't exist is not
                    // an error, and a NULL list is being treated as an empty
                    // list, we don't have to do anything. -SB
                    return;
                }

                devices.Remove(device_id);

                if (devices.Count == 0) {
                    registered_remote_devices.Remove(point);
                }
            }
        }

        public bool remote_device_exists(int x, int y, string expected_id) {

            Point point = new Point(x, y);

            lock(this) {
                List<string> devices;
                bool exists = registered_remote_devices.TryGetValue(point, out devices);
                if (!exists) {
                    return false;
                }

                foreach (string device_id in devices) {
                    if (device_id == expected_id) { return true; }
                }

                return false;
            }
        }

        public void register_local_device(int x, int y, string device_id) {
            Point location = new Point(x, y);

            JObject register_message = new JObject();
            register_message.Add("type", "register");
            register_message.Add("x", x);
            register_message.Add("y", y);
            register_message.Add("what", device_id);

            lock(this) {
                registered_local_devices.Remove(location);
                registered_local_devices.Add(location, device_id);
                if (connection_active) {
                    send_message(register_message);
                }
            }
        }

        public void unregister_local_device(int x, int y, string device_id) {
            string existing_device;
            Point location = new Point(x, y);

            JObject unregister_message = new JObject();
            unregister_message.Add("type", "unregister");
            unregister_message.Add("x", x);
            unregister_message.Add("y", y);
            unregister_message.Add("what", device_id);

            lock(this) {
                if (registered_local_devices.TryGetValue(location, out existing_device)) {
                    if (existing_device == device_id) {
                        registered_local_devices.Remove(location);
                    }
                }
                if (connection_active) {
                    send_message(unregister_message);
                }
            }
        }

        private void send_registered_list() {
            lock(this) {
                foreach(KeyValuePair<Point,string> entry in registered_local_devices) {
                    JObject register_message = new JObject();
                    register_message.Add("type", "register");
                    register_message.Add("x", entry.Key.x);
                    register_message.Add("y", entry.Key.y);
                    register_message.Add("what", entry.Value);
                    send_message(register_message);
                }
            }
        }

        private void handle_message(JObject message) {
            switch((string)message["type"]) {
                case "ping":
                    JObject pong = new JObject();
                    pong.Add("type", "pong");
                    send_message(pong);
                    break;
                case "registered":
                    register_remote_device(new Point((int)message["x"],
                                          (int)message["y"]),
                                    (string)message["what"]);
                    break;
                case "unregistered":
                    unregister_remote_device(new Point((int)message["x"],
                                            (int)message["y"]),
                                      (string)message["what"]);
                    break;
                default:
                    lock(this) {
                        Point tile_got = Point.extract_from_message(message);
                        if (tile_got != null) {
                            JObject last_request;
                            last_request_for_tile.TryGetValue(tile_got, out last_request);

                            last_request_for_tile.Remove(tile_got);

                            if(last_request == null) {
                                // We didn't ask for this!
                                Debug.Log(STRINGS.ZTRANSPORT.NETWORK.DEBUG_UNEXPECTED_TILE);
                                break;
                            } else if (!last_request["cookie"]
                                       .Equals(message["cookie"])) {
                                // Cookie doesn't match, this message isn't
                                // a response to the last message sent for
                                // this tile. Discard it.
                                Debug.Log(STRINGS.ZTRANSPORT.NETWORK.DEBUG_COOKIE_MISMATCH);
                                break;
                            }
                            last_response_for_tile[tile_got] = message;
                        }

                        //  ___
                        // (X-X)
                        //   |
                        // --+--
                        //   |
                        //  / \
                    }
                    break;
            }
        }

        private void raise_connection_error(String error) {
            Debug.Log(STRINGS.ZTRANSPORT.NETWORK.CONNECTION_ERROR.Replace("{ERROR}", error));
            lock(this) {
                connection_error = error;
            }
        }

        public void read_thread_function() {
            connection_active = false;

            while(true) {
                try {
                    string saved_address;
                    ushort saved_port;
                    lock(this) {
                        connection_error = null;
                        while(address == null) Monitor.Wait(this);
                        saved_address = address;
                        saved_port = port;
                    }

                    // Try to make the client outside the lock, otherwise will
                    // game will hang if a Ztransport item tries to access the
                    // network while trying to connect
                    client = new TcpClient(saved_address, saved_port);

                    lock(this) {
                        reconnecting = false;
                    }
                }
                catch (SocketException e) {
                    lock(this) {
                        connection_error = e.Message;
                    }
                    Debug.Log(STRINGS.ZTRANSPORT.NETWORK.UNABLE_TO_CONNECT.Replace("{REASON}", e.Message));
                    Debug.Log(STRINGS.ZTRANSPORT.NETWORK.RETRY_CONNECTION);
                    Thread.Sleep(5000);
                    continue;
                }
                PeekableStream peekable_stream = new PeekableStream(client.GetStream(), 1);
                stream = peekable_stream;
                ProtoReader reader;

                try {
                    JObject hello = new JObject();
                    hello.Add("type", "hello");
                    hello.Add("proto", "oniz");
                    hello.Add("version", 2);
                    hello.Add("compression", "Zlib");
                    write_directly(this.stream, hello);

                    // if the first character is {
                    // then the stream is not compressed
                    byte[] buffer = new byte[1];
                    if (peekable_stream.Peek(buffer, 0, 1) != 1) {
                        throw new ServerDiedException("Couldn't peeking into servers response");
                    }

                    if (buffer[0] != '{') {
                        reader = new ProtoReader(stream, true);
                        ZlibStream zlib_stream = new ZlibStream(stream, CompressionMode.Compress);
                        zlib_stream.FlushMode = FlushType.Sync;
                        this.stream = zlib_stream;
                    } else {
                        reader = new ProtoReader(stream, false);
                    }

                    JObject response = read_directly(reader);

                    // The old server, version 0, had an undocumented response
                    // called "bad_version", which it sent out if the clients
                    // version didnt match up with the list the server could
                    // support. We are going to use another system, but the
                    // server is already out there in the wild, and we need
                    // this code to remain backwards compatible with the inital
                    // server
                    if ((string)response["type"] == "bad_version") {
                        response["type"] = "handshake_error";
                        response.Add("what", "version_too_new");
                    }

                    if ((string)response["type"] == "auth_ok") {
                        // All set
                    } else if ((string)response["type"] == "handshake_error") {
                        switch ((string)response["what"]) {
                            case "unknown_protocol":
                                raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.NOT_ZTRANSPORT_SERVER);
                                break;
                            case "version_too_old":
                                raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.UPDATE_CLIENT);
                                break;
                            case "version_too_new":
                                raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.UPDATE_SERVER);
                                break;
                            case "bad_version":
                            case "compression_type_unknown":
                            default:
                                raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.BAD_HANDSHAKE);
                                break;
                        }
                        goto cleanup_connection;
                    } else {
                        raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.BAD_HANDSHAKE);
                        Debug.Log(JsonConvert.SerializeObject(response));

                        goto cleanup_connection;
                    }

                    lock(this) {
                        connection_active = true;

                        // make sure we don't have any stale messages hanging
                        // around -SB
                        while(outgoing_messages.Count > 0) {
                            JObject v;
                            outgoing_messages.TryTake(out v);
                        }

                        // When we reconnect, we need to (re)send the
                        // list of registered devices, the server
                        // automatically discards registered devices from
                        // a disconnected client
                        send_registered_list();
                    }

                    write_thread = new Thread(new ThreadStart(() => write_thread_function()));
                    write_thread.IsBackground = true;
                    write_thread.Start();

                    ping_thread = new Thread(new ThreadStart(() => ping_thread_function()));
                    ping_thread.IsBackground = true;
                    ping_thread.Start();

                    while(true) {
                        JObject message = read_directly(reader);
                        handle_message(message);
                    }
                }
                catch(ServerDiedException) {
                    raise_connection_error(
                       STRINGS.ZTRANSPORT.NETWORK.SERVER_CLOSED_CONNECTION);
                }
                catch(IOException) {
                    if (!reconnecting) {
                        raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.SERVER_CONNECTION_INTERRUPTED);
                    }
                }
                catch(KeyNotFoundException e) {
                    raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.PROTOCOL_ERROR);
                    Debug.Log(e);
                }
                catch (JsonReaderException e) {
                    raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.CORRUPTED_MESSAGE);
                    Debug.Log(e);
                }
                catch (ZlibException e) {
                    raise_connection_error(STRINGS.ZTRANSPORT.NETWORK.CORRUPTED_MESSAGE);
                    Debug.Log(e);
                }
                catch(Exception e) {
                    if(e is ThreadAbortException ||
                       (e.InnerException != null
                        && e.InnerException is ThreadAbortException)) {
                        // We've been aborted because the main thread ended.
                        // This is fine.
                        break;
                    } else {
                        // Something else bad happened. This is not fine.
                        throw;
                    }
                }
            cleanup_connection:
                // If we got here, it's because the server connection was lost
                // for some reason.
                lock(this) {
                    connection_active = false;
                    foreach(KeyValuePair<Point,JObject> entry in last_request_for_tile) {
                        Point target_point = entry.Key;
                        JObject message = entry.Value;
                        last_response_for_tile[target_point]
                            = generate_missing_response(message);
                    }
                    last_request_for_tile.Clear();
                }

                // Make sure threads are stopped before resetting connection
                if(write_thread != null) {
                    write_thread.Abort();
                    write_thread.Join();
                }

                if(ping_thread != null) {
                    ping_thread.Abort();
                    ping_thread.Join();
                }

                write_thread = null;
                ping_thread = null;
                client.Close();
                lock (this) {
                    client = null;
                    if(reconnecting) continue;
                }
                Thread.Sleep(5000);
            }
        }

        public static JObject make_message(string type, int x, int y) {
            JObject new_message = new JObject();
            new_message.Add("type", type);
            new_message.Add("x", x);
            new_message.Add("y", y);
            return new_message;
        }

        public string get_connection_error() {
            lock (this) {
                return connection_error;
            }
        }
    }
}
