using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
  class TestClass
  {
    public static string ReturnString()
    {
      return "Original string";
    }

    public static string ReturnStringHijacked()
    {
      return "Modified string";
    }

    public static string StringProperty { get; set; }

    public string NonStaticReturnStringHijacked()
    {
      return "Nonstatic modified string";
    }

    public string NonStaticStringProperty { get; set; }
  }

  class Program
  {
    public static void HijackMethod(MethodInfo source, MethodInfo target)
    {
      RuntimeHelpers.PrepareMethod(source.MethodHandle);
      RuntimeHelpers.PrepareMethod(target.MethodHandle);

      var sourceAddress = source.MethodHandle.GetFunctionPointer();
      var targetAddress = (long)target.MethodHandle.GetFunctionPointer();

      int offset = (int)(targetAddress - (long)sourceAddress - 4 - 1); // four bytes for relative address and one byte for opcode

      byte[] instruction = {
        0xE9, // Long jump relative instruction
        (byte)(offset & 0xFF),
        (byte)((offset >> 8) & 0xFF),
        (byte)((offset >> 16) & 0xFF),
        (byte)((offset >> 24) & 0xFF)
      };

      Marshal.Copy(instruction, 0, sourceAddress, instruction.Length);
    }

    static void Main()
    {
      // String method
      Console.WriteLine(TestClass.ReturnString());

      HijackMethod(typeof(TestClass).GetMethod(nameof(TestClass.ReturnString)),
        typeof(TestClass).GetMethod(nameof(TestClass.ReturnStringHijacked)));

      Console.WriteLine(TestClass.ReturnString());

      // String property
      TestClass.StringProperty = "Test";

      Console.WriteLine(TestClass.StringProperty);

      HijackMethod(typeof(TestClass).GetProperty(nameof(TestClass.StringProperty)).GetMethod,
        typeof(TestClass).GetMethod(nameof(TestClass.ReturnStringHijacked)));

      Console.WriteLine(TestClass.StringProperty);

      // Nonstatic property
      var instance = new TestClass();

      instance.NonStaticStringProperty = "Test nonstatic";

      Console.WriteLine(instance.NonStaticStringProperty);

      HijackMethod(typeof(TestClass).GetProperty(nameof(TestClass.NonStaticStringProperty)).GetMethod,
        typeof(TestClass).GetMethod(nameof(TestClass.NonStaticReturnStringHijacked)));

      Console.WriteLine(instance.NonStaticStringProperty);
    }
  }
}
