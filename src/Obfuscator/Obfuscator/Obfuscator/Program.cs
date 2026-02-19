using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectDir = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\"));
        string inputPath = Path.Combine(projectDir, "Input.cs");
        string outputPath = Path.Combine(projectDir, "Output.cs");

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("Input.cs not found.");
            return;
        }

        string code = File.ReadAllText(inputPath);
        var methodMatches = Regex.Matches(code, @"static\s+void\s+(\w+)\s*\([^)]*\)\s*\{", RegexOptions.Multiline);

        Dictionary<string, string> renameMap = new Dictionary<string, string>();
        int counter = 1;

        foreach (Match match in methodMatches)
        {
            string originalName = match.Groups[1].Value;

            if (!renameMap.ContainsKey(originalName))
            {
                renameMap[originalName] = "m" + counter++;
            }
        }
        
        foreach (var pair in renameMap)
        {
            code = Regex.Replace(code, $@"\b{pair.Key}\b", pair.Value);
        }
        
        var methodBlocks = Regex.Matches(code, @"static\s+void\s+\w+\s*\([^)]*\)\s*\{[^}]*\}",
            RegexOptions.Singleline)
            .Cast<Match>()
            .Select(m => m.Value)
            .ToList();
        
        foreach (var block in methodBlocks)
        {
            code = code.Replace(block, "");
        }
        
        Random rnd = new Random();
        methodBlocks = methodBlocks.OrderBy(x => rnd.Next()).ToList();
        int lastBraceIndex = code.LastIndexOf("}");
        string newMethods = "\n" + string.Join("\n\n", methodBlocks) + "\n";
        code = code.Insert(lastBraceIndex, newMethods);

        File.WriteAllText(outputPath, code);
        Console.WriteLine("Obfuscation complete.");
        Console.WriteLine("Output file: Obfuscated.cs");
    }
}
