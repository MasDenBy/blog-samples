Console.WriteLine("Hello, Kube! v.1.2");
Console.WriteLine("Starting background process");
Console.WriteLine("Looking for a configuration in local file");

var stepToRun = 0;

const string configFileName = "/configuration/step.config";

if (File.Exists(configFileName))
{
    stepToRun = int.Parse(File.ReadAllText(configFileName));
    Console.WriteLine($"{stepToRun} steps were completed. Continue process.");
    stepToRun++;
}
else
{
    Console.WriteLine("Configuration does not exists. Starting from 0 step");
}

while (true)
{
    Console.WriteLine($"Processing step: {stepToRun}");

    // Do some work
    Thread.Sleep(5000);
    File.WriteAllText(configFileName, stepToRun.ToString());

    stepToRun++;
}