internal class Program
    {
        private static void Main()
        {
            //var path =  @"C:\TFS\PinpointIT\Atlas\ServiceScheduler\Trunk";
            var path = @"C:\TFS\PinpointIT\Atlas\Points Engine\Trunk";

            List<string> files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Where(
                    p =>
                        !p.Contains(@"\bin\") && !p.Contains(@"\obj\") && !p.Contains(@"\packages\") &&
                        !p.Contains(".suo") && !p.EndsWith(".zip") && !p.Contains(".nuget") &&
                        !p.Contains("BonusPartnerManager.") && !p.Contains("OperationsManager.") && !p.Contains("Rewards.") && !p.Contains(".StartProfileRun"))
                .ToList();

            var txt = new StringBuilder();

            files.ForEach(p =>
            {
                txt.Append("[START==" + p + "==START]");
                txt.Append(File.ReadAllText(p));
                txt.Append("[STOP==" + p + "==STOP]");
            });

            string result = txt.ToString();

            File.WriteAllText("out.txt", result);

            byte[] x = ZipStr(result);

            var builder = new StringBuilder();
            foreach (var b in x)
            {
                builder.AppendFormat(",{0}", b);
            }
            var w = builder.ToString().Trim(',');
            File.WriteAllText("out1.txt", w);

            var d = w.Split(',').ToList().ConvertAll(byte.Parse).ToArray();
            var e = UnZipStr(d);

            var y = UnZipStr(x);

            var t0 = result == y;
            var t1 = result == e;
        }

        public static byte[] ZipStr(String str)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip =
                    new DeflateStream(output, CompressionMode.Compress))
                {
                    using (var writer =
                        new StreamWriter(gzip, Encoding.UTF8))
                    {
                        writer.Write(str);
                    }
                }

                return output.ToArray();
            }
        }

        public static string UnZipStr(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            {
                using (var gzip =
                    new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (var reader =
                        new StreamReader(gzip, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
