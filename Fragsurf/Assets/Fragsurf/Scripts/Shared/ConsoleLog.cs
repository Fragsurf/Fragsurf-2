using System.IO;

namespace Fragsurf.Shared
{
    public class ConsoleLog : FSComponent
    {

        private string _buffer = string.Empty;
        private bool _writeLog;
        private int _idx;

        protected override void _Initialize()
        {
            if (LaunchParams.Contains("logconsole"))
            {
                _writeLog = true;
                DevConsole.OnMessageLogged += FSConsole_OnMessageLogged;
            }
        }

        protected override void _Update()
        {
            if(_buffer.Length >= 1000000)
            {
                WriteToFile();
                _buffer = string.Empty;
                _idx++;
            }
        }

        protected override void _Destroy()
        {
            if (!_writeLog)
                return;

            WriteToFile();
        }

        private void WriteToFile()
        {
            var path = Path.Combine(Structure.LogsPath, $"console{_idx}.log");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(_buffer);
                }
            }
        }

        private void FSConsole_OnMessageLogged(string message)
        {
            _buffer += $"\n{message}";
        }
    }
}

