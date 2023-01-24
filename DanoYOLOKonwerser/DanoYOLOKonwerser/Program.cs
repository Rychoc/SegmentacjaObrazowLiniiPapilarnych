using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

class Item
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public double Angle { get; set; }
    public List<string> Points { get; set; } = null!;
}

class Program
{
    [SupportedOSPlatform("windows")]
    static void Main()
    {
        string pathData = "X:/Segmentacja/FingerprintsToYOLO/Dane";
        string pathResult = "X:/Segmentacja/FingerprintsToYOLO/Wynik";
        DirectoryInfo folderData = new(pathData); 
        foreach (var bmpFile in folderData.GetFiles("*.bmp"))
        {
            var fileName = bmpFile.Name.Remove(bmpFile.Name.Length - 4);
            Bitmap bitmap = new(bmpFile.FullName);
            bitmap.Save($"{pathResult}/{fileName}.jpg", ImageFormat.Jpeg);

            var widthOfImage = bitmap.Width;
            var heightOfImage = bitmap.Height;
            var txtData = File.ReadAllText($"{pathData}/{fileName}.txt");
            List<Item> Data = JsonConvert.DeserializeObject<List<Item>>(txtData);
            List<string> dataList = new();
            if (Data != null)
            {
                foreach (var item in Data)
                {
                    List<float> posXList = new();
                    List<float> posYList = new();
                    foreach (var point in item.Points)
                    {
                        string[] temp = point.Split(',');
                        posXList.Add(float.Parse(temp[0])/widthOfImage);
                        posYList.Add(float.Parse(temp[1])/heightOfImage);
                    }
                    var minX = posXList.Min();
                    var maxX = posXList.Max();
                    var minY = posYList.Min();
                    var maxY = posYList.Max();
                    string dataToYOLO = string.Empty;
                    switch (item.Name)
                    {
                        case "Fingerprint":
                            dataToYOLO = $"0 {(minX + maxX)/2} {(minY + maxY)/2} {maxX-minX} {maxY-minY}";
                            break;
                        case "Core":
                            dataToYOLO = $"1 {minX} {minY} 0.1 0.1";
                            break;
                        case "Delta":
                            dataToYOLO = $"2 {minX} {minY} 0.1 0.1";
                            break;
                        case "Scar":
                            dataToYOLO = $"3 {(minX + maxX)/2} {(minY + maxY)/2} {maxX-minX} {maxY-minY}";
                            break;
                        default:
                            break;
                    }
                    dataList.Add(dataToYOLO);
                }
            }
            var txtYOLO = string.Join("\n", dataList).Replace(",",".");
            if (txtYOLO.Length > 0)
                File.WriteAllText($"{pathResult}/{fileName}.txt", txtYOLO);
        }
    }
}