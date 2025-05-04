public class ArgumentParser
{
    public string? Output { get; private set; }
    public string? Path { get; private set; }

    public ArgumentParser(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--output":
                    Output = args[i + 1];
                    i++;
                    break;

                case "--path":
                    Path = args[i + 1];
                    i++;
                    break;
            }
        }
    }
}
