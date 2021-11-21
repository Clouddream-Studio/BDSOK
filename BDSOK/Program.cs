using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BDSOK
{
    internal class Program
    {
        public struct Data
        {
            public string name { get; set; }
            public List<string> run { get; set; }
        }
        private static void Main(string[] args)
        {
            List<string> argsList = new List<string>(args);
            WebClient webClient = new WebClient();
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
                Output(1, "本地数据可能无法保证最新，请尽量使用开放源");
                data = File.ReadAllText("data.json");
                Output(0, "开始从本地源获取信息");
            }
            else
            {
                Output(1, "警惕可能夹带私货的开放源！");
                data = Encoding.UTF8.GetString(Encoding.Default.GetBytes(webClient.DownloadString(argsList.Contains("-link") ? argsList[argsList.IndexOf("-link") + 1] : "http://api.cldem.top:44380/BDSOK/data.json")));
                Output(0, $"开始从{(argsList.Contains("-link") ? argsList[argsList.IndexOf("-link") + 1] : "http://cldem.top:44380/BDSOK/data.json")}获取信息");
            }
            List<Data> webDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Data>>(data);
            Directory.CreateDirectory("BDSOK_cache");
            foreach (Data webData in webDatas)
            {
                Output(0, $"开始运行步骤{webData.name}");
                try
                {
                    foreach (string runArgs in webData.run)
                    {
                        string[] runArg = runArgs.Split(' ');
                        switch (runArg[0])
                        {
                            case "show":
                                Output(5, runArgs.Substring(5));
                                break;
                            case "downloadmatch":
                                Output(0, $"开始从{runArg[1]}获取下载链接");
                                HttpWebRequest httpWebRequest = WebRequest.CreateHttp(runArg[1]);
                                httpWebRequest.UserAgent = "User-Agent:BDSOK/0.0.1";
                                string link = Regex.Match(Encoding.UTF8.GetString(Encoding.Default.GetBytes(new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd())), runArg[2]).Value;
                                Output(0, $"开始从{link}下载至{runArg[3]}");
                                webClient.DownloadFile(link, runArg[3]);
                                break;
                            case "download":
                                Output(0, $"开始从{runArg[1]}下载至{runArg[2]}");
                                webClient.DownloadFile(runArg[1], runArg[2]);
                                break;
                            case "unzip":
                                Output(0, $"开始解压{runArg[1]}到{runArg[2]}");
                                new FastZip().ExtractZip(runArg[1], runArg[2], null);
                                break;
                            case "copyfile":
                                Output(0, $"开始复制文件{runArg[1]}到{runArg[2]}");
                                File.Copy(runArg[1], runArg[2]);
                                break;
                            case "copydir":
                                Output(0, $"开始复制文件夹{runArg[1]}内文件到{runArg[2]}");
                                CopyDir(runArg[1], runArg[2]);
                                break;
                            case "writefile":
                                Output(0, $"开始覆写{runArg[1]}");
                                File.WriteAllText(runArg[1], runArgs.Substring(11 + runArg[1].Length));
                                break;
                            case "appendfile":
                                Output(0, $"开始写入{runArg[1]}");
                                File.AppendAllText(runArg[1], runArgs.Substring(12 + runArg[1].Length));
                                break;
                            case "mkdir":
                                Output(0, $"开始新建{runArg[1]}");
                                Directory.CreateDirectory(runArg[1]);
                                break;
                            case "delfile":
                                Output(0, $"开始删除文件{runArg[1]}");
                                File.Delete(runArg[1]);
                                break;
                            case "deldir":
                                Output(0, $"开始删除文件夹{runArg[1]}");
                                Directory.Delete(runArg[1]);
                                break;
                            case "start":
                                Output(0, $"开始启动{runArg[1]}");
                                Process.Start(runArg[1]);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Output(2, ex.Message);
                }
            }
            Directory.Delete("BDSOK_cache", true);
            Output(0, "已全部完毕！");
            if (!argsList.Contains("-nopkc"))
            {
                Output(1, "按任意键退出");
                Console.ReadKey(true);
            }
        }
        private static void Output(int info, string msg)
        {
            Console.WriteLine($"[{DateTime.Now} {(info == 0 ? "INFO" : info == 1 ? "WARN" : info == 2 ? "ERROR" : info == 3 ? "FATAL" : info == 4 ? "DEBUG" : info == 5 ? "MSG" : string.Empty)}] {msg}");
        }
        private static void CopyDir(string sourceFolder, string destFolder)
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
    }
}
