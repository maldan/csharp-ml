using System;
using System.Runtime.InteropServices;
using MegaLib.OS.Api;

namespace MegaLib.Asm;

public class AsmRuntime
{
  public AsmRuntime()
  {
  }

  public T AllocateFunction<T>(string name, byte[] code) where T : Delegate
  {
    // Аллоцируем память и копируем туда ассемблерный код
    var pCode = Marshal.AllocHGlobal(code.Length);
    Marshal.Copy(code, 0, pCode, code.Length);

    // Устанавливаем исполняемые права доступа на память
    if (!SetExecutable(pCode, code.Length))
      throw new InvalidOperationException("Не удалось установить права доступа на память.");

    // Преобразуем указатель на функцию в делегат
    return (T)Marshal.GetDelegateForFunctionPointer(pCode, typeof(T));
  }

  // Функция для установки прав доступа на память
  private static bool SetExecutable(IntPtr address, int length)
  {
    return Kernel32.VirtualProtect(address, (UIntPtr)length, 0x40, out _);
  }

  // Прототип функции копирования данных
  public delegate void MemCopyDelegate(IntPtr dest, IntPtr src, int length);

  public static MemCopyDelegate MemCopy()
  {
    var r = new AsmRuntime();
    // Ассемблерный код для функции копирования данных
    byte[] code =
    [
      0x48, 0x89, 0xCF, 0x48, 0x89, 0xD6, 0x44, 0x89, 0xC1, 0xF3, 0xA4, 0xC3
    ];
    return r.AllocateFunction<MemCopyDelegate>("memcpy", code);
  }
}