using System.IO.Compression;
using System.Text;

namespace MyApp;

interface IDataSource
{
    void WriteData(string data);
    string ReadData();
}

class FileDataSource : IDataSource
{

    public string _path;
    protected string _fileName { get; set; }

    public FileDataSource()
    {
        _fileName = "Notepad";
        _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), _fileName + ".txt");
    }

    public void WriteData(string data)
    {
        File.WriteAllText(_path, data);
    }
    public string ReadData()
    {

        return File.ReadAllText(_path);

    }
    class DataSourceDecorator : IDataSource
    {
        protected IDataSource _source;

        protected DataSourceDecorator(IDataSource source)
        {
            _source = source;
        }
        public virtual void WriteData(string data)
        {
            _source.WriteData(data);
        }

        public virtual string ReadData()
        {
            return _source.ReadData();

        }
    }
    class EncryptionDecorator : DataSourceDecorator
    {
        public EncryptionDecorator(IDataSource source) : base(source) { }
        public override void WriteData(string data)
        {
            var dataBytes = Encoding.Default.GetBytes(data);
            byte code = 4;
            for (int i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] ^= code;
            }
            _source.WriteData(Encoding.Default.GetString(dataBytes));
        }
        public override string ReadData()
        {
            var data = base.ReadData();
            var dataBytes = Encoding.Default.GetBytes(data);
            byte code = 4;
            for (int i = 0; i < dataBytes.Length; i++)
            {
                dataBytes[i] ^= code;
            }
            Console.WriteLine("Data sacsesfuly encripted");
            return Encoding.Default.GetString(dataBytes);
        }
    }
    class CompressionDecorator : DataSourceDecorator
    {
        public CompressionDecorator(IDataSource source) : base(source) { }
        public string CompressString(string text)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notepad.txt");
            FileInfo fileToCompress = new(path);
            using (FileStream originalFileStream = fileToCompress.OpenRead())
            {
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                {
                    using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                    {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            originalFileStream.CopyTo(compressionStream);
                            Console.WriteLine("Zip file created!");
                        }
                    }
                }
            }
            File.Delete(path);
            return "";
        }
        public string DecompressString()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notepad.txt" + ".gz");

            try
            {
                FileInfo fileToDecompress = new FileInfo(path);
                using (FileStream originalFileStream = fileToDecompress.OpenRead())
                {
                    string currentFileName = fileToDecompress.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                    using (FileStream decompressedFileStream = File.Create(newFileName))
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        decompressionStream.CopyTo(decompressedFileStream);

                    File.Delete(fileToDecompress.Name);
                }

            }
            catch (Exception) { return ""; }
            return "";
        }
        public override void WriteData(string data)
        {
            CompressString(data);

        }
        public override string ReadData()
        {
            DecompressString();
            return base.ReadData();

        }
    }
    class Program
    {
         public static void Words()
         {
             Console.WriteLine("If you want encript this file press 1!\n");
             Console.WriteLine("If you want Compress this file press 2!\n");
             Console.WriteLine("If you want end your work press e!\n");
         }
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ehter the data you want to write your Textbox:\n");
            Console.WriteLine("Please after writing press enter 2 times for program poperly work!!!");
            var data = Console.ReadLine();
            IDataSource dataSource = new FileDataSource();
            dataSource.WriteData(data);
            Console.WriteLine("\n\nData was successfully written to the file!\n\n");
            Words();            
            
            string? input = null;
            input = Console.ReadLine();
            char c = char.Parse(input);
            while (c != 'e')
            {
                Console.ReadKey();
                Console.Clear();
                if (c == '1')
                {
                   dataSource = new EncryptionDecorator(dataSource);
                   dataSource.WriteData(data);
                   Console.WriteLine("Data successfully encripted!");
                   //Console.ReadKey();
                }
                if(c == '2')
                {
                    try
                    {
                        dataSource = new CompressionDecorator(dataSource);                        
                        dataSource.WriteData(data);
                        Console.WriteLine(dataSource.ReadData());
                        Console.WriteLine("Data successfully compressed!");
                        //Console.ReadKey();
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                  
                }
                else
                {
                  Console.WriteLine("");
                }
               
                Words();
                input = String.Empty;
                input = Console.ReadLine();
                c = char.Parse(input);
            }
            }
        }
    }
