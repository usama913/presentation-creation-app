// Smoke-test / demo: builds a sample PresentationDocument by hand (no AI
// call, no API keys needed) and renders it with the open-source PPTX and
// PDF renderers, writing both files to the current directory. Useful to
// sanity-check the rendering engines in isolation:
//
//   dotnet run --project src/Gamma.Samples

using Gamma.Core.Models;
using Gamma.Rendering.OpenSource.Pdf;
using Gamma.Rendering.OpenSource.Pptx;

var document = new PresentationDocument
{
    Title = "Competitive Intelligence, Simplified",
    Subtitle = "How Watchmycompetitor keeps you ahead",
    Theme = new ThemeOptions
    {
        PrimaryColorHex = "1F2933",
        AccentColorHex = "2563EB",
        BackgroundColorHex = "FFFFFF",
        FontFamily = "Calibri"
    },
    Slides = new List<Slide>
    {
        new() { Title = "Competitive Intelligence, Simplified", Layout = SlideLayout.TitleSlide },
        new()
        {
            Title = "The problem",
            Layout = SlideLayout.TitleAndBullets,
            Bullets = new List<string>
            {
                "Competitor changes go unnoticed for weeks",
                "Manually checking pricing pages doesn't scale",
                "Insights are scattered across tools and tabs"
            }
        },
        new() { Title = "Our approach", Layout = SlideLayout.SectionHeader },
        new()
        {
            Title = "What we track",
            Layout = SlideLayout.TitleAndBullets,
            Bullets = new List<string>
            {
                "Pricing and packaging changes",
                "Website and messaging updates",
                "Job postings and hiring signals",
                "Social and review sentiment"
            }
        },
        new()
        {
            Title = "Customer voice",
            Layout = SlideLayout.Quote,
            Body = "“We caught a competitor's price drop the same day it happened.”"
        },
        new()
        {
            Title = "Let's talk",
            Layout = SlideLayout.Closing,
            Body = "Book a walkthrough at watchmycompetitor.com"
        }
    }
};

var pptxBytes = new OpenXmlLitePresentationRenderer().RenderAsync(document).GetAwaiter().GetResult();
var pdfBytes = new SimplePdfRenderer().RenderAsync(document).GetAwaiter().GetResult();

var outDir = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
Directory.CreateDirectory(outDir);

var pptxPath = Path.Combine(outDir, "sample-deck.pptx");
var pdfPath = Path.Combine(outDir, "sample-deck.pdf");

File.WriteAllBytes(pptxPath, pptxBytes);
File.WriteAllBytes(pdfPath, pdfBytes);

Console.WriteLine($"Wrote {pptxBytes.Length:N0} bytes to {pptxPath}");
Console.WriteLine($"Wrote {pdfBytes.Length:N0} bytes to {pdfPath}");
