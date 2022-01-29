using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

List<string> argsList = new(args);
WebClient webClient = new();
if (!argsList.Contains("-nologo"))
{
    Console.WriteLine("██████╗ ██████╗ ███████╗ ██████╗ ██╗  ██╗\n██╔══██╗██╔══██╗██╔════╝██╔═══██╗██║ ██╔╝\n██████╔╝██║  ██║███████╗██║   ██║█████╔╝\n██╔══██╗██║  ██║╚════██║██║   ██║██╔═██╗\n██████╔╝██████╔╝███████║╚██████╔╝██║  ██╗\n╚═════╝ ╚═════╝ ╚══════╝ ╚═════╝ ╚═╝  ╚═╝\t0.0.1.0\n");
}
Console.Title = "BDSOK 0.0.1.0";
#if DEBUG
Console.WriteLine("______________________________  __________   ______  ___     _________\n___  __ \\__  ____/__  __ )_  / / /_  ____/   ___   |/  /___________  /____\n__  / / /_  __/  __  __  |  / / /_  / __     __  /|_/ /_  __ \\  __  /_  _ \\\n_  /_/ /_  /___  _  /_/ // /_/ / / /_/ /     _  /  / / / /_/ / /_/ / /  __/\n/_____/ /_____/  /_____/ \\____/  \\____/      /_/  /_/  \\____/\\__,_/  \\___/\n");
Console.Title = "BDSOK 0.0.1.0 DEBUG";
#endif
string data = "[]";
if (File.Exists("data.json"))
{
    Logger.Trace("本地数据可能无法保证最新，请尽量使用开放源", Logger.LogLevel.WARN);
    data = File.ReadAllText("data.json");
    Logger.Trace("开始从本地源获取信息");
}
else
{
    Logger.Trace("警惕可能夹带私货的开放源！", Logger.LogLevel.WARN);
    string link = argsList.Contains("-link") ? argsList[argsList.IndexOf("-link") + 1] : "http://api.cldem.top/BDSOK/data.json";
    try
    {
        data = Encoding.UTF8.GetString(Encoding.Default.GetBytes(webClient.DownloadString(link)));
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        Logger.Trace("请按任意键退出. . .");
        Console.ReadKey(true);
    }
    Logger.Trace($"开始从{link}获取信息");
}
List<Data> webDatas = JsonConvert.DeserializeObject<List<Data>>(data);
Directory.CreateDirectory("BDSOK_cache");
foreach (Data webData in webDatas)
{
    try
    {
        foreach (string runArgs in webData.run)
        {
            string[] runArg = runArgs.Split(' ');
            switch (runArg[0])
            {
                case "show":
                    Logger.Trace($"[{webData.name}] {runArgs[5..]}", Logger.LogLevel.INFO);
                    break;
                case "downloadmatch":
                    Logger.Trace($"[{webData.name}] 开始从{runArg[1]}获取下载链接");
                    HttpWebRequest httpWebRequest = WebRequest.CreateHttp(runArg[1]);
                    httpWebRequest.UserAgent = "User-Agent:BDSOK/0.0.1";
                    string link = Regex.Match(Encoding.UTF8.GetString(Encoding.Default.GetBytes(new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd())), runArg[2]).Value;
                    // string link = Regex.Match(Encoding.UTF8.GetString(Encoding.Default.GetBytes(webClient.DownloadString(runArg[1]))), runArg[2]).Value;
                    Logger.Trace($"[{webData.name}] 开始从{link}下载至{runArg[3]}");
                    webClient.DownloadFile(link, runArg[3]);
                    break;
                case "download":
                    Logger.Trace($"[{webData.name}] 开始从{runArg[1]}下载至{runArg[2]}");
                    webClient.DownloadFile(runArg[1], runArg[2]);
                    break;
                case "unzip":
                    Logger.Trace($"[{webData.name}] 开始解压{runArg[1]}到{runArg[2]}");
                    new FastZip().ExtractZip(runArg[1], runArg[2], null);
                    break;
                case "copyfile":
                    Logger.Trace($"[{webData.name}] 开始复制文件{runArg[1]}到{runArg[2]}");
                    File.Copy(runArg[1], runArg[2]);
                    break;
                case "copydir":
                    Logger.Trace($"[{webData.name}] 开始复制文件夹{runArg[1]}内文件到{runArg[2]}");
                    CopyDir(runArg[1], runArg[2]);
                    break;
                case "writefile":
                    Logger.Trace($"[{webData.name}] 开始覆写{runArg[1]}");
                    File.WriteAllText(runArg[1], runArgs[(11 + runArg[1].Length)..]);
                    break;
                case "appendfile":
                    Logger.Trace($"[{webData.name}] 开始写入{runArg[1]}");
                    File.AppendAllText(runArg[1], runArgs[(12 + runArg[1].Length)..]);
                    break;
                case "mkdir":
                    Logger.Trace($"[{webData.name}] 开始新建{runArg[1]}");
                    Directory.CreateDirectory(runArg[1]);
                    break;
                case "delfile":
                    Logger.Trace($"[{webData.name}] 开始删除文件{runArg[1]}");
                    File.Delete(runArg[1]);
                    break;
                case "deldir":
                    Logger.Trace($"[{webData.name}] 开始删除文件夹{runArg[1]}");
                    Directory.Delete(runArg[1]);
                    break;
                case "start":
                    Logger.Trace($"[{webData.name}] 开始启动{runArg[1]}");
                    Process.Start(runArg[1]);
                    break;
            }
        }
    }
    catch (Exception ex)
    {
        Logger.Trace(ex.Message, Logger.LogLevel.ERROR);
    }
}
Directory.Delete("BDSOK_cache", true);
Logger.Trace("已全部完毕！");
if (!argsList.Contains("-nopkc"))
{
    Logger.Trace("请按任意键退出. . .");
    Console.ReadKey(true);
}

void CopyDir(string sourceFolder, string destFolder)
{
    if (!Directory.Exists(destFolder))
    {
        Directory.CreateDirectory(destFolder);
    }
    foreach (string file in Directory.GetFiles(sourceFolder))
    {
        File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)));
    }
    foreach (string folder in Directory.GetDirectories(sourceFolder))
    {
        CopyDir(folder, Path.Combine(destFolder, Path.GetFileName(folder)));
    }
}

public struct Data
{
    public string name { get; set; }
    public List<string> run { get; set; }
}
