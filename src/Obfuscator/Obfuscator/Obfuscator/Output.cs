namespace Obfuscator;

public class Input
{
 static void m3()
    {
        Console.WriteLine(123);
    }

static void m1()
    {
        m2();
        m3();
    }

static void m2()
    {
        Console.WriteLine("Hello!");
    }
}