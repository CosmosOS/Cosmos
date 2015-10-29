using System;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class MyClass
{
	public static void RunSnippet()
	{
		Regex r = new Regex("test_\\d+_");
		StreamWriter s = new StreamWriter("Mthds.txt");
		Assembly a = Assembly.LoadFrom("CsTests.dll");
		foreach (Type t in a.GetTypes())
		{
			foreach (MethodInfo m in t.GetMethods())
			{
				if (r.IsMatch(m.Name))
				{
					s.WriteLine("if (Tests." + m.Name + "() != " + r.Match(m.Name).Value.Substring(5, r.Match(m.Name).Value.Length - 6) + ")");	
					s.WriteLine("{");
					s.WriteLine("\tConsole.WriteLine(\"Test '" + m.Name + "' didn't return expected value. Expected: '" + r.Match(m.Name).Value.Substring(5, r.Match(m.Name).Value.Length - 6) + "' Got: '\" + Tests." + m.Name + "().ToString() + \"'\");");
					s.WriteLine("}");
				}
			}
		}
		s.Flush();
		s.Close();
	}
	
	#region Helper methods
	
	public static void Main()
	{
		try
		{
			RunSnippet();
		}
		catch (Exception e)
		{
			string error = string.Format("---\nThe following error occurred while executing the snippet:\n{0}\n---", e.ToString());
			Console.WriteLine(error);
		}
		finally
		{
			Console.Write("Press any key to continue...");
			Console.ReadKey();
		}
	}

	private static void WL(object text, params object[] args)
	{
		Console.WriteLine(text.ToString(), args);	
	}
	
	private static void RL()
	{
		Console.ReadLine();	
	}
	
	private static void Break() 
	{
		System.Diagnostics.Debugger.Break();
	}

	#endregion
}