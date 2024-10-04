using System;
using System.Collections.Generic;

namespace MegaLib.Asm;

public enum Register
{
  // 64-битные регистры
  RAX,
  RBX,
  RCX,
  RDX,
  RSI,
  RDI,
  RBP,
  RSP,
  R8,
  R9,
  R10,
  R11,
  R12,
  R13,
  R14,
  R15,

  // 32-битные регистры
  EAX,
  EBX,
  ECX,
  EDX,
  ESI,
  EDI,
  EBP,
  ESP,
  R8D,
  R9D,
  R10D,
  R11D,
  R12D,
  R13D,
  R14D,
  R15D
}

public class AsmProgram
{
  private readonly List<byte> _code = [];

  // MOV (работает с imm32 для 32-битных регистров и с imm64 для 64-битных регистров)
  public void MOV(Register register, long value)
  {
    var is64Bit = Is64BitRegister(register);
    AddREXPrefix(register, is64Bit);

    switch (register)
    {
      // 64-битные регистры
      case Register.RAX:
        _code.Add(0xB8); // MOV RAX, imm64
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.RBX:
        _code.Add(0xBB); // MOV RBX, imm64
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.RCX:
        _code.Add(0xB9); // MOV RCX, imm64
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.RDX:
        _code.Add(0xBA); // MOV RDX, imm64
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.R8:
        _code.Add(0xB8 + 0x08); // MOV R8, imm64
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.R9:
        _code.Add(0xB8 + 0x09); // MOV R9, imm64
        _code.AddRange(BitConverter.GetBytes(value));
        break;

      // 32-битные регистры
      case Register.EAX:
        _code.Add(0xB8); // MOV EAX, imm32
        _code.AddRange(BitConverter.GetBytes((int)value));
        break;
      case Register.EBX:
        _code.Add(0xBB); // MOV EBX, imm32
        _code.AddRange(BitConverter.GetBytes((int)value));
        break;
      case Register.ECX:
        _code.Add(0xB9); // MOV ECX, imm32
        _code.AddRange(BitConverter.GetBytes((int)value));
        break;
      case Register.EDX:
        _code.Add(0xBA); // MOV EDX, imm32
        _code.AddRange(BitConverter.GetBytes((int)value));
        break;
      case Register.R8D:
        _code.Add(0xB8 + 0x08); // MOV R8D, imm32
        _code.AddRange(BitConverter.GetBytes((int)value));
        break;
      case Register.R9D:
        _code.Add(0xB8 + 0x09); // MOV R9D, imm32
        _code.AddRange(BitConverter.GetBytes((int)value));
        break;

      default:
        throw new ArgumentException("Unsupported register for MOV instruction.");
    }
  }

  // Пример MOV инструкции (MOV reg1, reg2)
  public void MOV(Register dest, Register src)
  {
    var is64BitDest = Is64BitRegister(dest);
    var is64BitSrc = Is64BitRegister(src);

    // Если оба регистра 64-битные
    if (is64BitDest && is64BitSrc)
    {
      AddREXPrefix(dest, src, true); // Префикс для 64-бит
      _code.Add(0x89); // MOV регистр-регистр
      _code.Add(GetModRMByte(dest, src));
    }
    // Если оба регистра 32-битные
    else if (!is64BitDest && !is64BitSrc)
    {
      AddREXPrefix(dest, src, false); // Префикс для 32-бит (если нужны расширенные регистры)
      _code.Add(0x89); // MOV регистр-регистр
      _code.Add(GetModRMByte(dest, src));
    }
    else
    {
      throw new InvalidOperationException("Нельзя перемещать значения между регистрами разной битности.");
    }
  }

  // ADD (работает с imm32 для 32-битных и 64-битных регистров)
  public void ADD(Register register, int value)
  {
    AddREXPrefix(register, Is64BitRegister(register));

    switch (register)
    {
      // 64-битные регистры
      case Register.RAX:
        _code.Add(0x05); // ADD RAX, imm32
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.RBX:
        _code.Add(0x81); // ADD RBX, imm32
        _code.Add(0xC3); // ModRM для RBX
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.R8:
        _code.Add(0x81); // Префикс для R8
        _code.Add(0xC0); // ModRM для R8
        _code.AddRange(BitConverter.GetBytes(value));
        break;

      // 32-битные регистры
      case Register.EAX:
        _code.Add(0x05); // ADD EAX, imm32
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.EBX:
        _code.Add(0x81); // Префикс для EBX
        _code.Add(0xC3); // ModRM для EBX
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.R8D:
        _code.Add(0x81); // Префикс для R8D
        _code.Add(0xC0); // ModRM для R8D
        _code.AddRange(BitConverter.GetBytes(value));
        break;

      default:
        throw new ArgumentException("Unsupported register for ADD instruction.");
    }
  }

  // SUB (работает с imm32 для 32-битных и 64-битных регистров)
  public void SUB(Register register, int value)
  {
    AddREXPrefix(register, Is64BitRegister(register));

    switch (register)
    {
      // 64-битные регистры
      case Register.RAX:
        _code.Add(0x2D); // SUB RAX, imm32
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.RBX:
        _code.Add(0x81); // SUB RBX, imm32
        _code.Add(0xEB); // ModRM для RBX
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.R8:
        _code.Add(0x81); // Префикс для R8
        _code.Add(0xE8); // ModRM для R8
        _code.AddRange(BitConverter.GetBytes(value));
        break;

      // 32-битные регистры
      case Register.EAX:
        _code.Add(0x2D); // SUB EAX, imm32
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.EBX:
        _code.Add(0x81); // Префикс для EBX
        _code.Add(0xEB); // ModRM для EBX
        _code.AddRange(BitConverter.GetBytes(value));
        break;
      case Register.R8D:
        _code.Add(0x81); // Префикс для R8D
        _code.Add(0xE8); // ModRM для R8D
        _code.AddRange(BitConverter.GetBytes(value));
        break;

      default:
        throw new ArgumentException("Unsupported register for SUB instruction.");
    }
  }

  // Метод для добавления REX префикса
  private void AddREXPrefix(Register reg, bool is64Bit)
  {
    var rex = is64Bit ? (byte)0x48 : (byte)0x40; // Префикс REX для 64-бит или 32-бит

    if (IsExtendedRegister(reg)) rex |= 0x01; // REX.B для расширенных регистров (R8-R15)

    if (rex != 0x40 || is64Bit)
    {
      _code.Add(rex);
    }
  }

  // Метод для добавления REX префикса
  private void AddREXPrefix(Register dest, Register src, bool is64Bit)
  {
    var rex = is64Bit ? (byte)0x48 : (byte)0x40; // Префикс REX для 64-бит или 32-бит

    if (IsExtendedRegister(dest)) rex |= 0x04; // REX.R для расширенного регистра в dest
    if (IsExtendedRegister(src)) rex |= 0x01; // REX.B для расширенного регистра в src

    // Добавляем префикс только если регистр расширенный или это 64-битная операция
    if (rex != 0x40 || is64Bit)
    {
      _code.Add(rex);
    }
  }

  // Получение ModRM байта для двух регистров
  private byte GetModRMByte(Register dest, Register src)
  {
    var modrm = (byte)((GetRegisterCode(dest) << 3) | GetRegisterCode(src));
    return modrm;
  }

  // Получение кода регистра для ModRM байта
  private byte GetRegisterCode(Register reg)
  {
    switch (reg)
    {
      case Register.RAX:
      case Register.EAX: return 0b000;
      case Register.RCX:
      case Register.ECX: return 0b001;
      case Register.RDX:
      case Register.EDX: return 0b010;
      case Register.RBX:
      case Register.EBX: return 0b011;
      case Register.RSP:
      case Register.ESP: return 0b100;
      case Register.RBP:
      case Register.EBP: return 0b101;
      case Register.RSI:
      case Register.ESI: return 0b110;
      case Register.RDI:
      case Register.EDI: return 0b111;
      case Register.R8:
      case Register.R8D: return 0b000;
      case Register.R9:
      case Register.R9D: return 0b001;
      case Register.R10:
      case Register.R10D: return 0b010;
      case Register.R11:
      case Register.R11D: return 0b011;
      case Register.R12:
      case Register.R12D: return 0b100;
      case Register.R13:
      case Register.R13D: return 0b101;
      case Register.R14:
      case Register.R14D: return 0b110;
      case Register.R15:
      case Register.R15D: return 0b111;
      default: throw new ArgumentException("Unsupported register.");
    }
  }

  // Проверка, является ли регистр 64-битным
  private bool Is64BitRegister(Register reg)
  {
    return reg >= Register.RAX && reg <= Register.R15;
  }

  // Проверка, является ли регистр расширенным (R8-R15 или их 32-битные эквиваленты)
  private bool IsExtendedRegister(Register reg)
  {
    return reg >= Register.R8 && reg <= Register.R15D;
  }

  // Метод для получения сгенерированного машинного кода
  public byte[] GetMachineCode()
  {
    return _code.ToArray();
  }
}