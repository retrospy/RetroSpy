namespace GBPUpdaterX2
{
    public class OEMDriverInf
    {
        public OEMDriverInf(string driverPath, string driverVersion)
        {
            FileName = System.IO.Path.GetFileName(driverPath);
            DriverVer = driverVersion;
            Path = driverPath;
        }

        public string FileName { get; }
        public string DriverVer { get; }
        public string Path { get; }
    }
}