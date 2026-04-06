/*
 * Audio - A swiftlys2 plugin to control counter-strike 2 in-game VoIP audio stream.
 * Copyright (C) 2025  samyyc
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
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Runtime.InteropServices;

namespace Audio.Opus;

public static class Engine2Opus {

  [DllImport("engine2", EntryPoint = "opus_encoder_create")]
  private static extern OpusSafeHandle opus_encoder_create(int sampleRate, int channels, int application, out int error);

  [DllImport("engine2", EntryPoint = "opus_encoder_destroy")]
  private static extern void opus_encoder_destroy(IntPtr encoder);

  [DllImport("engine2", EntryPoint = "opus_encode")]
  private static extern int opus_encode(IntPtr encoder, IntPtr input, int inputLength, IntPtr output, int outputLength);

  [DllImport("engine2", EntryPoint = "opus_encode_float")]
  private static extern int opus_encode_float(IntPtr encoder, IntPtr input, int inputLength, IntPtr output, int outputLength);

  [DllImport("engine2", EntryPoint = "opus_encoder_ctl")]
  private static extern int opus_encoder_ctl(IntPtr encoder, int type, nint data);

  public static OpusSafeHandle Create(int sampleRate, int channels, OpusApplication application) {
    int error;
    var handle = opus_encoder_create(sampleRate, channels, (int)application, out error);
    OpusException.ThrowIfError("opus_encoder_create", error);
    return handle;
  }

  internal static void Destroy(nint encoder) {
    if (encoder != IntPtr.Zero) {
      opus_encoder_destroy(encoder);
    }
  }

  public static void SetEncoderCTL(OpusSafeHandle encoder, int request, nint data) {
    bool releaseHandle = false;
    try {
      encoder.DangerousAddRef(ref releaseHandle);
      var result = opus_encoder_ctl(encoder.DangerousGetHandle(), request, data);
      OpusException.ThrowIfError("opus_encoder_ctl", result);
    }
    finally {
      if (releaseHandle) {
        encoder.DangerousRelease();
      }
    }
  }

  public static void Destroy(OpusSafeHandle encoder) {
    encoder.Dispose();
  }

  public static int EncodeFloat(OpusSafeHandle encoder, ReadOnlySpan<float> input, int inputLength, Span<byte> output, int outputLength) {
    if ((uint)inputLength > (uint)input.Length) {
      throw new ArgumentOutOfRangeException(nameof(inputLength));
    }
    if ((uint)outputLength > (uint)output.Length) {
      throw new ArgumentOutOfRangeException(nameof(outputLength));
    }

    bool releaseHandle = false;
    unsafe {
      try {
        encoder.DangerousAddRef(ref releaseHandle);
        fixed (float* inputPtr = input) {
          fixed (byte* outputPtr = output) {
          var result = opus_encode_float(encoder.DangerousGetHandle(), (nint)inputPtr, inputLength, (nint)outputPtr, outputLength);
          OpusException.ThrowIfError("opus_encode_float", result);
          return result;
          }
        }
      }
      finally {
        if (releaseHandle) {
          encoder.DangerousRelease();
        }
      }
    }
  }

  public static int Encode(OpusSafeHandle encoder, ReadOnlySpan<short> input, int inputLength, Span<byte> output, int outputLength) {
    if ((uint)inputLength > (uint)input.Length) {
      throw new ArgumentOutOfRangeException(nameof(inputLength));
    }
    if ((uint)outputLength > (uint)output.Length) {
      throw new ArgumentOutOfRangeException(nameof(outputLength));
    }

    bool releaseHandle = false;
    unsafe {
      try {
        encoder.DangerousAddRef(ref releaseHandle);
        fixed (short* inputPtr = input) {
          fixed (byte* outputPtr = output) {
          var result = opus_encode(encoder.DangerousGetHandle(), (nint)inputPtr, inputLength, (nint)outputPtr, outputLength);
          OpusException.ThrowIfError("opus_encode", result);
          return result;
          }
        }
      }
      finally {
        if (releaseHandle) {
          encoder.DangerousRelease();
        }
      }
    }
  }


}