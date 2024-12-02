using Newtonsoft.Json;

namespace StudyResource.Models
{
    public class Keyword
    {
        public int Id { get; set; }
        [JsonProperty("value")]
        public string? Value { get; set; }
        public string? UnsignValue { get; set; }
        public int UsageCount { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<DocumentKeyword>? DocumentKeywords { get; set; } 
    }
}
