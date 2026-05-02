using Tesseract;

namespace DocumentProcessor.Services
{
    public class OcrService
    {
        private readonly string _tessDataPath;

        public OcrService()
        {
            _tessDataPath = @"C:\Program Files\Tesseract-OCR\tessdata";
        }

        public string ExtractTextFromImage(byte[] imageBytes)
        {
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);
            return page.GetText();
        }
    }
}