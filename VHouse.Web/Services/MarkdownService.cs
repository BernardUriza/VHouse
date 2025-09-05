using Markdig;
using System.Text.RegularExpressions;

namespace VHouse.Web.Services;

public interface IMarkdownService
{
    string ConvertToHtml(string markdownContent);
    (string html, List<string> mermaidDiagrams) ConvertWithMermaidExtraction(string markdownContent);
}

public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseDiagrams()
            .Build();
    }

    public string ConvertToHtml(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
            return string.Empty;

        return Markdown.ToHtml(markdownContent, _pipeline);
    }

    public (string html, List<string> mermaidDiagrams) ConvertWithMermaidExtraction(string markdownContent)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
            return (string.Empty, new List<string>());

        var mermaidDiagrams = new List<string>();
        var mermaidId = 0;

        // Extract Mermaid diagrams and replace with native mermaid divs
        var processedMarkdown = Regex.Replace(
            markdownContent,
            @"```mermaid\s*(.*?)\s*```",
            match =>
            {
                var diagramCode = match.Groups[1].Value.Trim();
                mermaidDiagrams.Add(diagramCode);
                return $"<div class=\"mermaid\" id=\"mermaid-{mermaidId++}\">{diagramCode}</div>";
            },
            RegexOptions.Singleline | RegexOptions.IgnoreCase
        );

        var html = Markdown.ToHtml(processedMarkdown, _pipeline);

        return (html, mermaidDiagrams);
    }
}