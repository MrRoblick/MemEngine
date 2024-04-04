using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryEngine;

public abstract class MemEngine
{
    private const int ProcessWmRead = 0x0010;
    
    private const int ProcessVmWrite = 0x0020;
    private const int ProcessVmOperation = 0x0008;
    
    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(long hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize,
        ref int lpNumberOfBytesRead);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(long hProcess, long lpBaseAddress, 
        byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
    
    public class MemoryApplication(long handle, ProcessModuleCollection modules)
    {
        public int ReadInt(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToInt32(buffer);
        }
        public int ReadInt16(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[2];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToInt16(buffer);
        }
        public ushort ReadUint16(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[2];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToUInt16(buffer);
        }
        public uint ReadUint32(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToUInt32(buffer);
        }
        public ulong ReadUint64(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[8];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToUInt64(buffer);
        }
        public bool ReadBool(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[1];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToBoolean(buffer);
        }
        public byte ReadByte(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[1];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return buffer[0];
        }
        public byte[] ReadArrayOfBytes(long address, int length)
        {
            var bytesRead = 0;
            var buffer = new byte[length];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return buffer;
        }
        public float ReadFloat(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToSingle(buffer);
        }
        public Vector3 ReadFloatVector3(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4*3];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            
            return new Vector3(
                BitConverter.ToSingle(buffer, 0),
                BitConverter.ToSingle(buffer, 1*4),
                BitConverter.ToSingle(buffer, 2*4)
                );
        }
        public Vector3 ReadIntVector3(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4*3];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            
            return new Vector3(
                BitConverter.ToInt32(buffer, 0),
                BitConverter.ToInt32(buffer, 1*4),
                BitConverter.ToInt32(buffer, 2*4)
            );
        }
        
        public Vector2 ReadFloatVector2(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4*2];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            
            return new Vector2(
                BitConverter.ToSingle(buffer, 0),
                BitConverter.ToSingle(buffer, 1*4)
            );
        }
        public Vector2 ReadIntVector2(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[4*2];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            
            return new Vector2(
                BitConverter.ToInt32(buffer, 0),
                BitConverter.ToInt32(buffer, 1*4)
            );
        }
        public double ReadDouble(long address)
        {
            var bytesRead = 0;
            var buffer = new byte[8];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToDouble(buffer);
        }
        
        public string ReadUnicodeString(long address, int length)
        {
            var bytesRead = 0;
            var buffer = new byte[length*2];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return Encoding.Unicode.GetString(buffer);
        }
        
        public string ReadAsciiString(long address, int length)
        {
            var bytesRead = 0;
            var buffer = new byte[length];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return Encoding.ASCII.GetString(buffer);
        }
        
        public string ReadUtf8String(long address, int length)
        {
            var bytesRead = 0;
            var buffer = new byte[length*4];
            ReadProcessMemory(handle, address, buffer, buffer.Length, ref bytesRead);
            return Encoding.UTF8.GetString(buffer);
        }

        public long GetModuleAddress(string name)
        {
            for (var i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module.ModuleName == name)
                {
                    return module.BaseAddress;
                }
            }

            return 0;
        }
        public int GetModuleSize(string name)
        {
            for (var i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module.ModuleName == name)
                {
                    return module.ModuleMemorySize;
                }
            }

            return 0;
        }
        
        public ProcessModuleCollection GetModules()
        {
            return modules;
        }

        public void WriteString(long address, string value)
        {
            var bytesWritten = 0;
            var buffer = Encoding.Unicode.GetBytes(value + "\0");
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteFloat(long address, float value)
        {
            var bytesWritten = 0;
            var buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteInt(long address, int value)
        {
            var bytesWritten = 0;
            var buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        public void WriteUint16(long address, ushort value)
        {
            var bytesWritten = 0;
            var buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteByte(long address, byte value)
        {
            var bytesWritten = 0;
            var buffer = new[]{value};
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteBool(long address, bool value)
        {
            var bytesWritten = 0;
            var buffer = new[]{value ? (byte)255 : (byte)0};
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteBytes(long address, byte[] value)
        {
            var bytesWritten = 0;
            WriteProcessMemory(handle, address, value, value.Length, ref bytesWritten);
        }
        
        public void WriteUint32(long address, uint value)
        {
            var bytesWritten = 0;
            var buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteUint64(long address, ulong value)
        {
            var bytesWritten = 0;
            var buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteFloatVector3(long address, Vector3 value)
        {
            var bytesWritten = 0;
            var buffer = new byte[4 * 3];
            var x = BitConverter.GetBytes(value.X);
            for (var i = 0; i < x.Length; i++)
            {
                buffer[i] = x[i];
            }
            
            var y = BitConverter.GetBytes(value.Y);
            for (var i = 0; i < y.Length; i++)
            {
                buffer[i*4] = y[i];
            }
            
            var z = BitConverter.GetBytes(value.Z);
            for (var i = 0; i < z.Length; i++)
            {
                buffer[i*4*2] = z[i];
            }
            
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }
        
        public void WriteFloatVector2(long address, Vector2 value)
        {
            var bytesWritten = 0;
            var buffer = new byte[4 * 2];
            var x = BitConverter.GetBytes(value.X);
            for (var i = 0; i < x.Length; i++)
            {
                buffer[i] = x[i];
            }
            var y = BitConverter.GetBytes(value.Y);
            for (var i = 0; i < y.Length; i++)
            {
                buffer[i*4] = y[i];
            }
            
            WriteProcessMemory(handle, address, buffer, buffer.Length, ref bytesWritten);
        }

        

        public List<long> SignatureScanning(string moduleName, int[] bytes)
        {
            var address = GetModuleAddress(moduleName);
            var size = GetModuleSize(moduleName);
            var result = new List<long>();
            var chunks = size / 4096;
            for (var i = 0; i < chunks; i++)
            {
                var addr = address + i*4096;
                var buffer = new byte[4096];
                var bytesRead = 0;
                ReadProcessMemory(handle, addr, buffer, buffer.Length, ref bytesRead);
                for (var i2 = 0; i2 < buffer.Length; i2++)
                {
                    var byteRec = buffer[i2];
                    if(bytes[0] >= 0 && byteRec != bytes[0]) continue;
                    
                    var notMatch = false;
                    for (var inner = 0; inner < bytes.Length; inner++)
                    {
                        if (i2+inner < buffer.Length && (bytes[inner] < 0 || buffer[i2+inner] == bytes[inner])) continue;
                        notMatch = true;
                        break;
                    }

                    if (notMatch) continue;
                    result.Add(addr+i2);
                }
                
            }
            return result;
        }
        public List<long> SignatureScanning(string moduleName, byte[] bytes)
        {
            var address = GetModuleAddress(moduleName);
            var size = GetModuleSize(moduleName);
            var result = new List<long>();
            var chunks = size / 4096;
            for (var i = 0; i < chunks; i++)
            {
                var addr = address + i*4096;
                var buffer = new byte[4096];
                var bytesRead = 0;
                ReadProcessMemory(handle, addr, buffer, buffer.Length, ref bytesRead);
                for (var i2 = 0; i2 < buffer.Length; i2++)
                {
                    var byteRec = buffer[i2];
                    if(byteRec != bytes[0]) continue;
                    var notMatch = false;
                    for (var inner = 0; inner < bytes.Length; inner++)
                    {
                        if (i2+inner < buffer.Length && buffer[i2+inner] == bytes[inner]) continue;
                        notMatch = true;
                        break;
                    }
                    if (notMatch) continue;
                    result.Add(addr+i2);
                }
                
            }
            return result;
        }
    }
    
    public static MemoryApplication OpenProcess(string processName)
    {
        var proc = Process.GetProcessesByName(processName)[0];
        var handle = OpenProcess(ProcessWmRead | ProcessVmWrite | ProcessVmOperation, false, proc.Id);
        return new MemoryApplication((int)handle, proc.Modules);
    }
}