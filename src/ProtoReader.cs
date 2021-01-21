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
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace ZTransport {
    // Solra wrote this, and shouldn't have had to. Why is C# so unique in
    // having only flawed deflate libraries? -SB
    class ProtoReader {
        private byte[] decompressed_buffer = null;
        private byte[] compressed_buffer = null;
        // start: the first byte we haven't returned yet.
        // pos: the first byte we haven't checked for a newline yet.
        // end: the first byte that doesn't exist yet.
        private int start = 0, pos = 0, end = 0;
        private bool compressed, eof = false;
        private Stream underlying_stream;
        private ZlibCodec zlib;
        public ProtoReader(Stream underlying_stream, bool compressed) {
            this.underlying_stream = underlying_stream;
            this.compressed = compressed;
            this.decompressed_buffer = new byte[1024];
            if(compressed) {
                // Obviously, we won't need these if we're not reading
                // compressed data.
                this.compressed_buffer = new byte[1024];
                this.zlib = new ZlibCodec();
                zlib.InitializeInflate(true); // true = want RFC1950 header
                zlib.InputBuffer = compressed_buffer;
                zlib.NextIn = 0;
                zlib.AvailableBytesIn = 0;
            }
        }
        // If (and only if) there is not a newline buffered, reads more data
        // from the stream until there is. Always returns a line, blocking if
        // (and only if) necessary. (Actually, doesn't always return a line;
        // returns NULL on EOF.)
        public string ReadLine() {
            while(!eof) {
                while(pos >= end && !eof) {
                    this.ReadSomeMore();
                }
                while(pos < end) {
                    if(decompressed_buffer[pos] == '\n') {
                        // found a line!
                        string ret = Encoding.UTF8.GetString(decompressed_buffer,
                                                             start, pos - start);
                        start = ++pos;
                        return ret;
                    }
                    else {
                        // didn't find a line yet...
                        ++pos;
                    }
                }
                // we need to read some more... go back around
            }
            // DO NOT return a partial line, because this is an element of a
            // protocol and an incomplete message shouldn't ever be valid.
            return null;
        }
        private void ReadSomeMore() {
            if(eof) return;
            // Compact the buffer, if we can
            if(start == end) {
                // Easy.
                start = pos = end = 0;
            }
            else if(start > 0) {
                // Not quite as easy.
                Array.Copy(decompressed_buffer, start,
                           decompressed_buffer, 0, end - start);
                pos -= start;
                end -= start;
                start = 0;
            }
            // Now, if necessary, expand the decompressed buffer.
            if(end >= decompressed_buffer.Length) {
                // assert(start == 0)
                if(decompressed_buffer.Length >= (1<<24)) {
                    throw new IOException("Buffered way more data than should ever be buffered. Giving up.");
                }
                byte[] new_buffer = new byte[decompressed_buffer.Length * 2];
                Debug.Log("ProtoReader buffer growing to "+(new_buffer.Length)+" bytes.");
                Array.Copy(decompressed_buffer, 0,
                           new_buffer, 0, end);
                decompressed_buffer = new_buffer;
            }
            // Okay, now we read. How this works depends on whether we're using
            // compression.
            if(compressed) {
                zlib.OutputBuffer = decompressed_buffer;
                zlib.NextOut = end;
                zlib.AvailableBytesOut = decompressed_buffer.Length - end;
                // assert(zlib.AvailableBytesOut > 0)
                if(zlib.AvailableBytesIn == 0) {
                    zlib.NextIn = 0;
                    zlib.AvailableBytesIn = underlying_stream.Read(compressed_buffer,
                                                                   0, compressed_buffer.Length);
                    if(zlib.AvailableBytesIn == 0) {
                        eof = true;
                    }
                }
                int result = zlib.Inflate(FlushType.Sync);
                if(result != ZlibConstants.Z_OK
                   && result != ZlibConstants.Z_STREAM_END) {
                    throw new ZlibException("inflating: " + zlib.Message);
                }
                end = zlib.NextOut; // heh... heh... heh...
            }
            else {
                // (red, as in the color)
                int red = underlying_stream.Read(decompressed_buffer, end,
                                                 decompressed_buffer.Length - end);
                if(red == 0) {
                    eof = true;
                }
                else {
                    // assert(end + red <= decompressed_buffer.Length)
                    end += red;
                }
            }
        }
    }
}
