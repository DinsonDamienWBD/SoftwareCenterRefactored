using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SoftwareCenter.UIManager.Services
{
    public class UiTemplateService
    {
        private readonly string _webRootPath;
        private Dictionary<string, string> _templateCache;

        public UiTemplateService(string webRootPath)
        {
            _webRootPath = webRootPath;
        }

        public async Task<string> GetZoneHtmlAsync(string zoneName)
        {
            var path = Path.Combine(_webRootPath, "Html", $"{zoneName.ToLower()}-zone.html");
            if (!File.Exists(path)) return $"";
            return await File.ReadAllTextAsync(path);
        }

        public async Task<string> GetTemplateHtmlAsync(string templateId)
        {
            if (_templateCache == null) await LoadTemplatesAsync();

            return _templateCache.ContainsKey(templateId)
                ? _templateCache[templateId]
                : $"";
        }

        private async Task LoadTemplatesAsync()
        {
            _templateCache = new Dictionary<string, string>();
            var path = Path.Combine(_webRootPath, "Html", "standard-templates.html");

            if (!File.Exists(path)) return;

            var content = await File.ReadAllTextAsync(path);
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            // Extract all <template> tags
            foreach (var node in doc.DocumentNode.Descendants("template"))
            {
                var id = node.GetAttributeValue("id", "");
                if (!string.IsNullOrEmpty(id))
                {
                    _templateCache[id] = node.InnerHtml;
                }
            }
        }
    }
}