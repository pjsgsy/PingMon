using Spectre.Console;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Linq;
class Program
{
    public static string[] addresses = { "smartie2", "212.30.17.52", "google.com", "www.fxcm.com","microsoft.com", "192.168.2.1", "178.79.190.168"  };
    public static int count = 0;
    public static long[] mins = new long[addresses.Length];
    public static long[] maxs = new long[addresses.Length];

    internal class Response
    {
        public long min = 0;
        public long max = 0;
        public Queue<long> avg = new Queue<long>();
        public long nr = 0;
    }
    public static Response[] Responses = new Response[addresses.Length];
    static void Main(string[] args)
    {
        if (args != null && args.Count() > 0)
        {
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
            }
        }
        List<Task<PingReply>> pingTasks = new List<Task<PingReply>>();

        Console.WriteLine("PingMon V1.0 (C) Paul J Smith - @pjsmith");
        Console.WriteLine("Press ESC to stop monitoring...");

        var table = new Table();
        // Add some columns
        table.AddColumn("#");
        table.AddColumn("host");
        table.AddColumn(new TableColumn("rt").RightAligned());
        table.AddColumn(new TableColumn("min").RightAligned());
        table.AddColumn(new TableColumn("max").RightAligned());
        table.AddColumn(new TableColumn("avg").RightAligned());
        table.AddColumn(new TableColumn("jit").RightAligned());
        table.AddColumn(new TableColumn("nr").RightAligned());
        table.AddColumn("-->");
        int i = 0;
        foreach (var address in addresses)
        {
            table.AddRow(i.ToString(), address, "");
            Responses[i] = new Response();
            i++;
        }
        //  table.Columns[6].PadRight(40);
        table.Columns[2].PadLeft(3);
        table.Columns[3].PadLeft(3);
        table.Columns[4].PadLeft(3);
        table.Columns[5].PadLeft(3);
        table.Columns[6].PadLeft(3);

        AnsiConsole.Live(table)
        .Start(ctx =>
        {
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                pingTasks.Clear();
                foreach (var address in addresses)
                {
                    pingTasks.Add(PingAsync(address));
                }
                //Wait for all the tasks to complete
                Task.WaitAll(pingTasks.ToArray());

                //Now you can iterate over your list of pingTasks
                i = 0;
                foreach (var pingTask in pingTasks)
                {
                    long pt = 0;
                    if (pingTask == null)
                    {
                        Responses[i].nr++;
                    }
                    else
                    {
                        pt = pingTask.Result.RoundtripTime;
                        Responses[i].avg.Enqueue(pt);
                        Responses[i].min = pt;
                        Responses[i].min = Responses[i].avg.Min();
                        Responses[i].max = Responses[i].avg.Max();
                        Responses[i].max = pt;
                    }
                        int avg = (int)Responses[i].avg.Average();
                    //pingTask.Result is whatever type T was declared in PingAsync
                    try
                    {
                        int c = (int) Math.Max(1,( pt / 5 ));
                       
                        table.UpdateCell(i, 2, pt.ToString());
                        table.UpdateCell(i, 3, Responses[i].min.ToString());
                        table.UpdateCell(i, 4, Responses[i].max.ToString());
                        table.UpdateCell(i, 5, avg.ToString());
                        if (count > 5)
                        {
                            double variance = GetVariance(Responses[i].avg);
                            table.UpdateCell(i, 6, variance.ToString("N1"));
                        }
                        if (Responses[i].nr > 0) table.UpdateCell(i, 7, Responses[i].nr.ToString());
                        string colour = "[darkseagreen]";
                        if (pt > avg * 1.5) colour = "[darkorange3_1]";
                        if (pt > avg * 2) colour = "[indianred_1]";
                        table.UpdateCell(i, 8, colour + new String('■', c) + "[/]" );
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                    while(Responses[i].avg.Count > 100)
                        Responses[i].avg.Dequeue();

                    i++;
                }
                ctx.Refresh();
                count++;
                Thread.Sleep(3000);
            }
        });
        Console.WriteLine("Stopped");
    }

    private static double GetVariance(Queue<long> numbers)
    {
        int count = numbers.Count;
        if (count == 0)
        {
            throw new InvalidOperationException("Variance cannot be calculated for an empty queue.");
        }

        // Calculate the mean of the numbers
        double mean = numbers.Average();

        // Calculate the sum of squared differences from the mean
        double sumOfSquaredDifferences = numbers.Select(n => (n - mean) * (n - mean)).Sum();

        // Calculate the variance
        double variance = sumOfSquaredDifferences / count;

        return variance;
    }

    static Task<PingReply> PingAsync(string address)
    {
        var tcs = new TaskCompletionSource<PingReply>();
        Ping ping = new Ping();
        try
        {
            ping.PingCompleted += (obj, sender) =>
            {
                tcs.SetResult(sender.Reply);
            };
            ping.SendAsync(address, new object());
        }
        catch (Exception e) {
            return tcs.Task; 
        }
        return tcs.Task;
    }
}