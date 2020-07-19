// This file is adapted from the byte-base64 library:
// https://github.com/enepomnyaschih/byte-base64
// (It would be just included as a client library, but I couldn't get that to work...)

/*
MIT License

Copyright (c) 2020 Egor Nepomnyaschih

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

var base64abc = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "+", "/"];
var base64codes = [255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 62, 255, 255, 255, 63, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 255, 255, 255, 0, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 255, 255, 255, 255, 255, 255, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51];

function getBase64Code(charCode) {
  if (charCode >= base64codes.length) {
    throw new Error("Unable to parse base64 string.");
  }

  var code = base64codes[charCode];

  if (code === 255) {
    throw new Error("Unable to parse base64 string.");
  }

  return code;
}

function bytesToBase64(bytes) {
  var result = '',
      i,
      l = bytes.length;

  for (i = 2; i < l; i += 3) {
    result += base64abc[bytes[i - 2] >> 2];
    result += base64abc[(bytes[i - 2] & 0x03) << 4 | bytes[i - 1] >> 4];
    result += base64abc[(bytes[i - 1] & 0x0F) << 2 | bytes[i] >> 6];
    result += base64abc[bytes[i] & 0x3F];
  }

  if (i === l + 1) {
    // 1 octet yet to write
    result += base64abc[bytes[i - 2] >> 2];
    result += base64abc[(bytes[i - 2] & 0x03) << 4];
    result += "==";
  }

  if (i === l) {
    // 2 octets yet to write
    result += base64abc[bytes[i - 2] >> 2];
    result += base64abc[(bytes[i - 2] & 0x03) << 4 | bytes[i - 1] >> 4];
    result += base64abc[(bytes[i - 1] & 0x0F) << 2];
    result += "=";
  }

  return result;
}

function base64ToBytes(str) {
  if (str.length % 4 !== 0) {
    throw new Error("Unable to parse base64 string.");
  }

  var index = str.indexOf("=");

  if (index !== -1 && index < str.length - 2) {
    throw new Error("Unable to parse base64 string.");
  }

  var missingOctets = str.endsWith("==") ? 2 : str.endsWith("=") ? 1 : 0,
      n = str.length,
      result = new Uint8Array(3 * (n / 4)),
      buffer;

  for (var i = 0, j = 0; i < n; i += 4, j += 3) {
    buffer = getBase64Code(str.charCodeAt(i)) << 18 | getBase64Code(str.charCodeAt(i + 1)) << 12 | getBase64Code(str.charCodeAt(i + 2)) << 6 | getBase64Code(str.charCodeAt(i + 3));
    result[j] = buffer >> 16;
    result[j + 1] = buffer >> 8 & 0xFF;
    result[j + 2] = buffer & 0xFF;
  }

  return result.subarray(0, result.length - missingOctets);
}

function base64encode(str) {
  var encoder = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : new TextEncoder();
  return bytesToBase64(encoder.encode(str));
}

function base64decode(str) {
  var decoder = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : new TextDecoder();
  return decoder.decode(base64ToBytes(str));
}
